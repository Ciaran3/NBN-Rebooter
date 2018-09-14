using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NBNRebooter
{
    class Program
    {
        static void Main(string[] args)
        {
            AppProperties oProperties = new AppProperties();

            // Parse the command line arguements
            var oParser = new SimpleCommandLineParser();
            oParser.Parse(args);

            // Currently the only modem supported
            oProperties.CurrentModem = new Fast3864AC();

            // Show startup text
            ShowIntro(oProperties);

            // Read the command line parameters
            oProperties.ReadParameters(oParser, oProperties);

            Timer oTimer = new Timer();
            oTimer.Interval = 60000; // One minute timer
            oTimer.Elapsed += (sender, e) => { HandleTimerElapsed(oProperties); };
            oTimer.Enabled = !String.IsNullOrEmpty(oProperties.ModemIP);

            if (oTimer.Enabled)
            {
                DoTimedEvents(oProperties);
            }

            Console.Read();
            oTimer.Dispose();
        }

        private static void ShowIntro(AppProperties AProperties)
        {
            Console.WriteLine(string.Format("Started {0}. Modem: {1}. Version: {2}", "NBN Rebooter", AProperties.CurrentModem.ModemName, Assembly.GetExecutingAssembly().GetName().Version));
        }

        private static void HandleTimerElapsed(AppProperties AProperties)
        {
            DoTimedEvents(AProperties);
        }

        private static void DoTimedEvents(AppProperties AProperties)
        {
            TimeSpan oStatDiff = DateTime.Now.Subtract(AProperties.LastStatCheck);

            // Only check for stats change every 30 minutes.
            if ((MinutesSinceLastReboot(AProperties) > 5) & (AProperties.InitialStatCheck | oStatDiff.Minutes > 30))
            {
                AProperties.CurrentModem.LogSyncRates(AProperties);
            }

            CheckForRebootRequired(AProperties);
        }

        private static int MinutesSinceLastReboot(AppProperties AProperties)
        {
            if (AProperties.HasReboot)
            {
                TimeSpan oTimeSpan = (DateTime.Now - AProperties.LastReboot);
                return oTimeSpan.Minutes;
            }
            else
            {
                // We have not rebooted yet - so do a reboot if the time is right.
                return 60;
            }
        }

        private static void CheckForRebootRequired(AppProperties AProperties)
        {
            TimeSpan dTimeNow = DateTime.Now.TimeOfDay;
            // 5 minute period that we should try to reboot within
            TimeSpan dEndTime = AProperties.RebootTime.Add(TimeSpan.FromMinutes(5));

            // Ensure we are within the reboot time and have not just already sent a reboot command.
            Boolean bTimeToReboot = (dTimeNow >= AProperties.RebootTime && dTimeNow <= dEndTime) & (MinutesSinceLastReboot(AProperties) > 15);

            if (AProperties.RebootOnStart | bTimeToReboot)
            {
                AProperties.CurrentModem.PerformReboot(AProperties);
            }
        }
    }
}
