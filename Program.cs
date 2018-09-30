using System;
using System.Reflection;
using System.Timers;

namespace NBNRebooter
{
    class Program
    {
        static void Main(string[] args)
        {
            AppProperties oProperties = new AppProperties();

            // Parse the command line arguments
            var oParser = new SimpleCommandLineParser();
            oParser.Parse(args);

            // Currently the only modem supported
            oProperties.CurrentModem = new Fast3864AC();

            // Show startup text
            ShowIntro(oProperties);

            // Read the command line parameters
            oProperties.ReadParameters(oParser, oProperties);

            // One minute timer
            Timer oTimer = new Timer {Interval = 60000};
            oTimer.Elapsed += (sender, e) => { HandleTimerElapsed(oProperties); };
            oTimer.Enabled = !String.IsNullOrEmpty(oProperties.ModemIP);

            if (oTimer.Enabled)
            {
                DoTimedEvents(oProperties);
            }

            Console.Read();
            oTimer.Dispose();
        }

        private static void ShowIntro(AppProperties aProperties)
        {
            Console.WriteLine(
                $"Started {"NBN Rebooter"}. Modem: {aProperties.CurrentModem.GetModemName()}. Version: {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        private static void HandleTimerElapsed(AppProperties aProperties)
        {
            DoTimedEvents(aProperties);
        }

        private static void DoTimedEvents(AppProperties aProperties)
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

            if (aProperties.ScheduleReboot || aProperties.MaxUpTimeReboot)
            {
                CheckForRebootRequired(aProperties);
            }
        }

        private static int MinutesSinceLastReboot(AppProperties aProperties)
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

        private static void CheckForRebootRequired(AppProperties aProperties)
        {
            TimeSpan dTimeNow = DateTime.Now.TimeOfDay;
            Boolean bPerformReboot = false;

            // 5 minute period that we should try to reboot within
            TimeSpan dEndTime = aProperties.RebootTime.Add(TimeSpan.FromMinutes(5));

            // Ensure we are within the reboot time and have not just already sent a reboot command.
            Boolean bScheduleTimeReached = aProperties.ScheduleReboot && (dTimeNow >= aProperties.RebootTime && dTimeNow <= dEndTime) && (MinutesSinceLastReboot(aProperties) > 15);

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
                    aProperties.RebootOnStart = false;
                }
            }
        }
    }
}
