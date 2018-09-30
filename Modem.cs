using System;
using System.Net;

namespace NBNRebooter
{
    public abstract class Modem
    {
        public abstract string GetModemName();
        public int UpTimeHours { get; set; }

        protected abstract string GetStatisticsUrl();
        protected abstract string GetRebootUrl();
        protected string Uprate { get; set; }
        protected string Downrate { get; set; }
        protected string UpTime { get; set; }

        protected abstract Boolean GetSyncRates(AppProperties aProperties);

        public void LogSyncRates(AppProperties aProperties)
        {
            // Only log if stats have changed
            if (GetSyncRates(aProperties) & (!String.Equals(Downrate + Uprate, aProperties.LastStats, StringComparison.OrdinalIgnoreCase)))
            {
                // Remember these stats
                aProperties.LastStats = Downrate + Uprate;
                aProperties.LogMessage(aProperties,
                    $"Downstream (Kbps): {Downrate}, Upstream (Kbps): {Uprate}, Uptime: {UpTime}");
            }
        }

        public Boolean PerformReboot(AppProperties aProperties)
        {
            GetSyncRates(aProperties);

            // Log the current stats just before the reboot
            aProperties.LogMessage(aProperties,
                $"Sending Reboot Command. Current Stats: Downstream (Kbps): {Downrate}, Upstream (Kbps): {Uprate}, Uptime: {UpTime}");

            string sRebootHtml = GetPageHtml(aProperties, string.Format(GetRebootUrl(), aProperties.ModemIP));
            return !string.IsNullOrEmpty(sRebootHtml);
        }

        public string GetPageHtml(AppProperties aProperties, string aUrl)
        {
            string sHtml = "";
            using (WebClient oClient = new WebClient())
            {
                if (aProperties.UseAuth)
                {
                    oClient.UseDefaultCredentials = true;
                    oClient.Credentials = new NetworkCredential(aProperties.Username, aProperties.Password);
                }
                try
                {
                    sHtml = oClient.DownloadString(aUrl);
                }
                catch (WebException ex)
                {
                    HandleWebException(ex, aUrl);
                }
            }
            return sHtml;
        }

        private void HandleWebException(WebException aException, string aUrl)
        {
            Console.WriteLine($"An error occurred loading the URL: {aUrl}. Error: {aException.Message}");
        }
    }
}
