using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBNRebooter
{
    public class Fast3864AC : Modem
    {
        public override string ModemName => "F@ST 3864AC";
        public override string StatisticsURL => "http://{0}/info.html";
        public override string RebootURL => "http://{0}/mngdevicestatus.cmd?action=reboot";

        public override Boolean GetSyncRates(AppProperties AProperties)
        {
            string sPage = GetPageHTML(AProperties, string.Format(StatisticsURL, AProperties.ModemIP));
            ResetStats();

            if  (!String.IsNullOrEmpty(sPage))
            {
                AProperties.LastStatCheck = DateTime.Now;
                Downrate = ExtractText(sPage, "B0 Line Rate - Downstream (Kbps):</td>");
                Uprate = ExtractText(sPage, "B0 Line Rate - Upstream (Kbps):</td>");
                UpTime = ExtractText(sPage, "Uptime:");
                return true;
            }
            else
            {
                return false;
            }
        }


        private static string ExtractText(string AHMLPage, string ASearchText)
        {
            // Find the text in the page
            int iTextStart = AHMLPage.IndexOf(ASearchText);
            const string CELL_START = "<td>";
            const string CELL_END = "</td>";

            if (iTextStart > -1)
            {
                int iTextEnd = AHMLPage.IndexOf(CELL_END, iTextStart + ASearchText.Length + 1);
                string sText = AHMLPage.Substring(iTextStart + ASearchText.Length, iTextEnd - iTextStart).Trim();

                // Now extract the value
                int iStart = sText.IndexOf(CELL_START);
                int iEnd = sText.IndexOf(CELL_END, iStart);

                return sText.Substring(iStart + CELL_START.Length, iEnd - iStart - CELL_START.Length).Trim();
            }
            else
                return "Unknown";
        }
    }
}
