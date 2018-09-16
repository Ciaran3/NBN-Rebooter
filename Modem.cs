using System;
using System.Net;

namespace NBNRebooter
{
    public abstract class Modem
    {
        public abstract string GetModemName();

        public abstract string GetStatisticsUrl();

        public abstract string GetRebootUrl();

        public string Uprate { get; set; }
        public string Downrate { get; set; }
        public string UpTime { get; set; }
        public int UpTimeHours { get; set; }

        public abstract Boolean GetSyncRates(AppProperties aProperties);

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

        public void PerformReboot(AppProperties aProperties)
        {
            GetSyncRates(aProperties);

            // Log the current stats just before the reboot
            aProperties.LogMessage(aProperties,
                $"Sending Reboot Command. Current Stats: Downstream (Kbps): {Downrate}, Upstream (Kbps): {Uprate}, Uptime: {UpTime}");

            string sRebootHtml = GetPageHtml(aProperties, string.Format(GetRebootUrl(), aProperties.ModemIP));

            if (!string.IsNullOrEmpty(sRebootHtml))
            {
                aProperties.LastReboot = DateTime.Now;
                aProperties.HasReboot = true;
                aProperties.RebootOnStart = false;
            }
        }

        public string GetPageHtml(AppProperties aProperties, string aUrl)
        {
            using (WebClient oClient = new WebClient())
            {
                if (aProperties.UseAuth)
                {
                    oClient.UseDefaultCredentials = true;
                    oClient.Credentials = new NetworkCredential(aProperties.Username, aProperties.Password);
                }

                try
                {
                    return oClient.DownloadString(aUrl);
                }
                catch (WebException ex)
                {
                    HandleWebException(ex, aUrl);
                    return "";
                }
            }
        }

        public void HandleWebException(WebException aException, string aUrl)
        {
            Console.WriteLine($"An error occurred loading the URL: {aUrl}. Error: {aException.Message}");
        }
    }
}
