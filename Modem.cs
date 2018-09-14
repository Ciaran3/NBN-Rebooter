using System;
using System.IO;
using System.Net;

namespace NBNRebooter
{
    public abstract class Modem
    {
        public abstract string ModemName { get; }
        public abstract string StatisticsURL { get; }
        public abstract string RebootURL { get; }
        public string Uprate { get; set; }
        public string Downrate { get; set; }
        public string UpTime { get; set; }


        public abstract Boolean GetSyncRates(AppProperties AProperties);

        public void ResetStats()
        {
            Uprate = "";
            Downrate = "";
            UpTime = "";
        }

        public void LogMessage(AppProperties AProperties, string AMessage)
        {
            string sOutput = string.Format("{0}, {1}", DateTime.Now.ToString(), AMessage);
            Console.WriteLine(sOutput);

            if (!String.IsNullOrEmpty(AProperties.LogPath))
            {
                try
                {
                    File.AppendAllText(AProperties.LogPath, sOutput + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("An error occurred writing to the log: {0}. Error: {1}", AProperties.LogPath, ex.Message));
                }
            }
        }

        public void LogSyncRates(AppProperties AProperties)
        {
            // Only log if stats have changed
            if (GetSyncRates(AProperties) & (!String.Equals(Downrate + Uprate, AProperties.LastStats, StringComparison.OrdinalIgnoreCase)))
            {
                // Remeber these stats
                AProperties.LastStats = Downrate + Uprate;

                LogMessage(AProperties, string.Format("Downstream (Kbps): {0}, Upstream (Kbps): {1}, Uptime: {2}", Downrate, Uprate, UpTime));
            }
        }

        public void PerformReboot(AppProperties AProperties)
        {
            GetSyncRates(AProperties);

            // Log the current stats just before the reboot
            LogMessage(AProperties, string.Format("Sending Reboot Command. Current Stats: Downstream (Kbps): {0}, Upstream (Kbps): {1}, Uptime: {2}", Downrate, Uprate, UpTime));

            string sRebootHTML = GetPageHTML(AProperties, string.Format(RebootURL, AProperties.ModemIP));

            if (!String.IsNullOrEmpty(sRebootHTML))
            {
                AProperties.LastReboot = DateTime.Now;
                AProperties.HasReboot = true;
                AProperties.RebootOnStart = false;
            }
        }

        public string GetPageHTML(AppProperties AProperties, string AURL)
        {
            using (System.Net.WebClient oClient = new System.Net.WebClient())
            {
                if (AProperties.UseAuth)
                {
                    oClient.UseDefaultCredentials = true;
                    oClient.Credentials = new NetworkCredential(AProperties.Username, AProperties.Password);
                }

                try
                {
                    return oClient.DownloadString(AURL);
                }
                catch (WebException ex)
                {
                    HandleWebException(ex, AURL);
                    return "";
                }
            }
        }

        public void HandleWebException(WebException AException, string AURL)
        {
            Console.WriteLine(string.Format("An error occurred loading the URL: {0}. Error: {1}", AURL, AException.Message));
        }
    }
}
