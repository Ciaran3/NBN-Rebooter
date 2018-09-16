using System;

namespace NBNRebooter
{
    public class Fast3864AC : Modem
    {
        public override string GetModemName()
        {
            return "F@ST 3864AC";
        }

        public override string GetStatisticsUrl()
        {
            return "http://{0}/info.html";
        }

        public override string GetRebootUrl()
        {
            return "http://{0}/mngdevicestatus.cmd?action=reboot";
        }

        public override Boolean GetSyncRates(AppProperties aProperties)
        {
            string sPage = GetPageHtml(aProperties, string.Format(GetStatisticsUrl(), aProperties.ModemIP));
            Uprate = "";
            Downrate = "";

            if (!string.IsNullOrEmpty(sPage))
            {
                aProperties.LastStatCheck = DateTime.Now;
                Downrate = ExtractText(sPage, "B0 Line Rate - Downstream (Kbps):</td>");
                Uprate = ExtractText(sPage, "B0 Line Rate - Upstream (Kbps):</td>");
                UpTime = ExtractText(sPage, "Uptime:");
                UpTimeHours = UpTimeToHours(UpTime);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int UpTimeToHours(string aUptime)
        {            
            int iDayIndex = aUptime.IndexOf("D", StringComparison.Ordinal);
            int iHourIndex = aUptime.IndexOf("H", StringComparison.Ordinal);
            int iDays = 0;
            int iHours = 0;

            if ((iDayIndex > -1) & (iHourIndex > -1))
            {
                string sDays = aUptime.Substring(0, iDayIndex).Trim();
                int.TryParse(sDays, out iDays);
                string sHours = aUptime.Substring(iDayIndex + 1, iHourIndex - 2).Trim();
                int.TryParse(sHours, out iHours);
            }

            return (iDays * 24) + iHours;
        }
        
        private static string ExtractText(string aHtml, string aSearchText)
        {
            // Find the text in the page
            int iTextStart = aHtml.IndexOf(aSearchText, StringComparison.Ordinal);
            const string cellStart = "<td>";
            const string cellEnd = "</td>";

            if (iTextStart > -1)
            {
                int iTextEnd = aHtml.IndexOf(cellEnd, iTextStart + aSearchText.Length + 1, StringComparison.Ordinal);
                string sText = aHtml.Substring(iTextStart + aSearchText.Length, iTextEnd - iTextStart).Trim();

                // Now extract the value
                int iStart = sText.IndexOf(cellStart, StringComparison.Ordinal);
                int iEnd = sText.IndexOf(cellEnd, iStart, StringComparison.Ordinal);

                return sText.Substring(iStart + cellStart.Length, iEnd - iStart - cellStart.Length).Trim();
            }
            else
                return "Unknown";
        }
    }
}
