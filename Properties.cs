using System;

namespace NBNRebooter
{
    public class AppProperties
    {
        public string ModemIP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Boolean UseAuth { get; set; }

        public string LastStats { get; set; }
        public TimeSpan RebootTime { get; set; }
        public string LogPath { get; set; }
        public DateTime LastReboot { get; set; }
        public DateTime LastStatCheck { get; set; }
        public Boolean InitialStatCheck { get; set; }
        public Boolean HasReboot { get; set; }
        public Boolean RebootOnStart { get; set; }
        public Boolean ScheduleReboot { get; set; }
        public Modem CurrentModem { get; set; }

        public void ReadParameters(SimpleCommandLineParser AParser, AppProperties AProperties)
        {
            AProperties.RebootOnStart = AParser.Arguments.ContainsKey("rebootnow");
            AProperties.ScheduleReboot = AParser.Arguments.ContainsKey("reboot");
            AProperties.InitialStatCheck = true;
            AProperties.RebootTime = TimeSpan.Zero;

            AProperties.UseAuth = AParser.Arguments.ContainsKey("username") & AParser.Arguments.ContainsKey("password");

            if (AProperties.UseAuth)
            {
                AProperties.Username = AParser.Arguments["username"][0];
                AProperties.Password = AParser.Arguments["password"][0];
            }

            AProperties.LogPath = (AParser.Arguments.ContainsKey("log")) ? AParser.Arguments["log"][0] : "";
            if (String.IsNullOrEmpty(AProperties.LogPath))
            {
                Console.WriteLine("No log path specified. Console output only enabled.");
            }

            AProperties.ModemIP = (AParser.Arguments.ContainsKey("ip")) ? AParser.Arguments["ip"][0] : "";
            if (String.IsNullOrEmpty(AProperties.ModemIP))
            {
                Console.WriteLine("No modem IP provide, please see the help for parameter usage.");
                AProperties.ModemIP = "";
                return;
            }

            if (AProperties.ScheduleReboot)
            {
                TimeSpan tsScheduleTime;
                if (TimeSpan.TryParse(AParser.Arguments["reboot"][0], out tsScheduleTime))
                {
                    AProperties.RebootTime = tsScheduleTime;
                }
                else
                {
                    AProperties.ScheduleReboot = false;
                    Console.WriteLine(string.Format("Unable to convert {0} to reboot hour.", AParser.Arguments["r"][0]));
                }
            }
            else
            {
                Console.WriteLine("No reboot hour set. Running in stat logging only mode.");
            }

            if (AProperties.RebootOnStart)
            {
                Console.WriteLine("Reboot parameter found. Reboot will occur immediately");
            }
            else
            if (AProperties.ScheduleReboot & (AProperties.RebootTime != TimeSpan.Zero))
            {
                Console.WriteLine(string.Format("Next reboot will occur at {0}", AProperties.RebootTime.ToString()));
            }
        }

    }
}
