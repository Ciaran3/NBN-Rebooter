using System;
using System.Reflection;
using System.Timers;

namespace NBNRebooter
{
    public class MainApp
    {
        public AppProperties RebooterProperties { get; set; }

        public static void Main(string[] args)
        {
            (new MainApp()).Run(args);
        }

        public void Run(string[] args)
        {
            RebooterProperties = new AppProperties();

            // Parse the command line arguments
            var oParser = new SimpleCommandLineParser();
            oParser.Parse(args);

            RebooterProperties.CurrentModem = GetModemClass();

            // Show startup text
            ShowIntro(RebooterProperties);

            // Read the command line parameters
            RebooterProperties.ReadParameters(oParser, RebooterProperties);

            // One minute timer
            Timer oTimer = new Timer { Interval = 60000 };
            oTimer.Elapsed += (sender, e) => { HandleTimerElapsed(RebooterProperties); };
            oTimer.Enabled = !String.IsNullOrEmpty(RebooterProperties.ModemIP);

            if (oTimer.Enabled)
            {
                DoTimedEvents(RebooterProperties);
            }

            DoConsoleRead();
            oTimer.Dispose();
        }

        protected virtual void DoConsoleRead()
        {
            Console.ReadLine();

        }

        protected virtual Modem GetModemClass()
        {
            // Currently the only modem supported
            return new Fast3864AC();
        }

        private static void ShowIntro(AppProperties aProperties)
        {
            Console.WriteLine(
                $"Started {"NBN Rebooter"}. Modem: {aProperties.CurrentModem.GetModemName()}. Version: {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        private void HandleTimerElapsed(AppProperties aProperties)
        {
            DoTimedEvents(aProperties);
        }

        public void DoTimedEvents(AppProperties aProperties)
        {
            TimeSpan oStatDiff = DateTime.Now.Subtract(aProperties.LastStatCheck);
            // Minutes since the last reboot command was sent.
            int iMinutesSinceLastReboot = MinutesSinceLastReboot(aProperties);
            // Allow some time for the reboot to occur, then check the statistics.
            Boolean bJustRebooted = (iMinutesSinceLastReboot > 4) && (iMinutesSinceLastReboot < 10);

            if ((bJustRebooted) || (aProperties.InitialStatCheck || oStatDiff.Minutes > aProperties.UpdateInterval))
            {
                aProperties.CurrentModem.LogSyncRates(aProperties);
                aProperties.InitialStatCheck = false;
            }

            if (!bJustRebooted && ((aProperties.ScheduleReboot || aProperties.RebootOnStart || aProperties.MaxUpTimeReboot)))
            {
                CheckForRebootRequired(aProperties);
            }
        }

        protected virtual int MinutesSinceLastReboot(AppProperties aProperties)
        {
            if (aProperties.HasReboot)
            {
                TimeSpan oTimeSpan = (DateTime.Now - aProperties.LastReboot);
                return oTimeSpan.Minutes;
            }
            else
            {
                // We have not rebooted yet - so do a reboot if the time is right.
                return 60;
            }
        }

        protected virtual void GetCurrentTime(out int aHour, out int aMinutes)
        {
            aHour = DateTime.Now.TimeOfDay.Hours;
            aMinutes = DateTime.Now.TimeOfDay.Minutes;
        }

        private void CheckForRebootRequired(AppProperties aProperties)
        {
            GetCurrentTime(out int iHours, out int iMinutes);
            TimeSpan dTimeNow = new TimeSpan(0, iHours, iMinutes, 0);
            Boolean bPerformReboot = false;

            // 5 minute period that we should try to reboot within
            TimeSpan dEndTime = aProperties.RebootTime.Add(TimeSpan.FromMinutes(5));

            // Ensure we are within the reboot time
            Boolean bScheduleTimeReached = aProperties.ScheduleReboot && (dTimeNow >= aProperties.RebootTime && dTimeNow <= dEndTime);

            if (aProperties.MaxUpTimeReboot)
            {
                // Check if we have reached the maximum up time.
                Boolean bMaxupTimeReached = aProperties.MaxUpTimeReboot && (aProperties.CurrentModem.UpTimeHours >= aProperties.MaxUpTime);
                bPerformReboot = bMaxupTimeReached && bScheduleTimeReached;
                if (bPerformReboot)
                {
                    aProperties.LogMessage(aProperties, "Schedule reboot time and maximum uptime reached.");
                }
            }
            else if (aProperties.ScheduleReboot)
            {
                bPerformReboot = bScheduleTimeReached;
                if (bPerformReboot)
                {
                    aProperties.LogMessage(aProperties, "Schedule reboot time reached.");
                }
            }

            if (aProperties.RebootOnStart || bPerformReboot)
            {
                if (aProperties.CurrentModem.PerformReboot(aProperties))
                {
                    aProperties.LastReboot = DateTime.Now;
                    aProperties.HasReboot = true;
                    aProperties.RebootOnStart = false;
                }
                else
                {
                    aProperties.LogMessage(aProperties, "Failed to send reboot command.");
                }
            }
        }
    }
}
