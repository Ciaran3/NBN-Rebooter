using System;
using System.Globalization;
using System.IO;
using static System.Console;

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
        public int MaxUpTime { get; set; }
        public string LogPath { get; set; }
        public DateTime LastReboot { get; set; }
        public DateTime LastStatCheck { get; set; }
        public Boolean InitialStatCheck { get; set; }
        public Boolean HasReboot { get; set; }
        public Boolean RebootOnStart { get; set; }
        public Boolean ScheduleReboot { get; set; }
        public Boolean MaxUpTimeReboot { get; set; }
        public Modem CurrentModem { get; set; }

        public void ReadParameters(SimpleCommandLineParser aParser, AppProperties aProperties)
        {
            MaxUpTime = 0;

            aProperties.RebootOnStart = aParser.Arguments.ContainsKey("rebootnow");
            aProperties.ScheduleReboot = aParser.Arguments.ContainsKey("reboot");
            aProperties.MaxUpTimeReboot = aParser.Arguments.ContainsKey("maxuptime");
            aProperties.InitialStatCheck = true;
            aProperties.RebootTime = TimeSpan.Zero;

            aProperties.UseAuth = aParser.Arguments.ContainsKey("username") & aParser.Arguments.ContainsKey("password");

            if (aProperties.UseAuth)
            {
                aProperties.Username = aParser.Arguments["username"][0];
                aProperties.Password = aParser.Arguments["password"][0];
            }

            if (aProperties.MaxUpTimeReboot)
            {
                if (int.TryParse(aParser.Arguments["maxuptime"][0], out int iMaxUpTime))
                {
                    MaxUpTime = iMaxUpTime;
                }
                else
                {
                    WriteLine($"Unable to convert {aParser.Arguments["maxuptime"][0]} to max up time.");
                }
            }

            aProperties.LogPath = (aParser.Arguments.ContainsKey("log")) ? aParser.Arguments["log"][0] : "";
            if (String.IsNullOrEmpty(aProperties.LogPath))
            {
                WriteLine("No log path specified. Console output only enabled.");
            }

            aProperties.ModemIP = (aParser.Arguments.ContainsKey("ip")) ? aParser.Arguments["ip"][0] : "";
            if (String.IsNullOrEmpty(aProperties.ModemIP))
            {
                WriteLine("No modem IP provide, please see the help for parameter usage.");
                aProperties.ModemIP = "";
                return;
            }

            if (aProperties.ScheduleReboot)
            {
                if (TimeSpan.TryParse(aParser.Arguments["reboot"][0], out var tsScheduleTime))
                {
                    aProperties.RebootTime = tsScheduleTime;
                }
                else
                {
                    aProperties.ScheduleReboot = false;
                    WriteLine($"Unable to convert {aParser.Arguments["reboot"][0]} to reboot hour.");
                }
            }

            if (aProperties.RebootOnStart)
            {
                WriteLine("Reboot will occur immediately...");
            }
            else
            if (aProperties.ScheduleReboot & (aProperties.RebootTime != TimeSpan.Zero))
            {
                WriteLine($"Next reboot will occur at {aProperties.RebootTime.ToString()}");
                if (MaxUpTime > 0)
                {
                    WriteLine($"AND when the modem uptime has reached {MaxUpTime.ToString()} hours.");
                }      
            }else if (MaxUpTime > 0)
            {
                WriteLine($"Next reboot will occur when the modem uptime has reached {MaxUpTime.ToString()} hours.");
            }else if ((!aProperties.ScheduleReboot) & !(MaxUpTime > 0))
            {
                WriteLine("No reboot schedule set. Only logging sync rate statistics.");
            }
        }

        public void LogMessage(AppProperties aProperties, string aMessage)
        {
            string sOutput = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}, {aMessage}";
            WriteLine(sOutput);

            if (!string.IsNullOrEmpty(aProperties.LogPath))
            {
                try
                {
                    File.AppendAllText(aProperties.LogPath, sOutput + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    WriteLine($"An error occurred writing to the log: {aProperties.LogPath}. Error: {ex.Message}");
                }
            }
        }
    }



}
