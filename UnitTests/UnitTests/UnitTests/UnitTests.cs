using System;
using System.Globalization;
using System.IO;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBNRebooter;

namespace UnitTests
{

    class UnitTestModem : Fast3864AC
    {

        public int PeformRebootCallCount { get; set; }

        public override Boolean PerformReboot(AppProperties aProperties)
        {
            PeformRebootCallCount = PeformRebootCallCount + 1;
            return true; // Don't actually perform a reboot in the unit tests...
        }
    }


    class UnitTestApp : MainApp
    {
        public int TestHour { get; set; }
        public int TestMinutes { get; set; }
        public Boolean OverrideMinutesSinceReboot { get; set; }

        protected override Modem GetModemClass()
        {
            return new UnitTestModem();
        }

        protected override int MinutesSinceLastReboot(AppProperties aProperties)
        {
            if (OverrideMinutesSinceReboot)
            {
                return 60;
            }
            else
            {
               return base.MinutesSinceLastReboot(aProperties);
            }
        }

        protected override void GetCurrentTime(out int aHour, out int aMinutes)
        {
            aHour = TestHour;
            aMinutes = TestMinutes;
        }

        protected override void DoConsoleRead()
        {
            // Exit the test after the first run
        }
    }


    [TestClass]
    public class UnitTests
    {
        UnitTestApp oNBNRebooter;
        private StringWriter oConsoleOutput;


        [TestInitialize]
        public void TestInitialize()
        {
            oNBNRebooter = new UnitTestApp();
            oConsoleOutput = new StringWriter();
            Console.SetOut(oConsoleOutput);
        }

        [TestCleanup]
        public void TestClean()
        {
            oConsoleOutput = null;
            oNBNRebooter = null;
        }

        private void SetTestTime(int aHour, int aMinutes)
        {
            oNBNRebooter.TestHour = aHour;
            oNBNRebooter.TestMinutes = aMinutes;
        }

        [TestMethod]
        [Timeout(600000)]
        public void NoIP()
        {
            string[] oArgsStrings = new[] { "-rebootnow" };
            oNBNRebooter.Run(oArgsStrings);
            Assert.IsTrue(oConsoleOutput.ToString().Contains("No modem IP provide, please see the help for parameter usage."));
        }

        [TestMethod]
        [Timeout(600000)]
        public void RebootOnStart()
        {
            string[] oArgsStrings = new[] { "-ip", "192.168.0.1", "-rebootnow", "-reboot", "06:30"};
            SetTestTime(7, 35);
            oNBNRebooter.Run(oArgsStrings);
            UnitTestModem oTestModem = (UnitTestModem)oNBNRebooter.RebooterProperties.CurrentModem;
            Assert.IsFalse(oConsoleOutput.ToString().Contains("No modem IP provide, please see the help for parameter usage."), "Check IP was found");
            Assert.IsTrue(oConsoleOutput.ToString().Contains("Reboot will occur immediately..."), "Check reboot command was found");
            Assert.AreEqual(1, oTestModem.PeformRebootCallCount, "Check start up reboot command was sent");

            // Simulate that last reboot was 30 minutes ago.
            oNBNRebooter.OverrideMinutesSinceReboot = true;
            SetTestTime(6, 31);
            oNBNRebooter.DoTimedEvents(oNBNRebooter.RebooterProperties);
            Assert.AreEqual(2, oTestModem.PeformRebootCallCount, "Check scheduled reboot command was sent");

        }

        [TestMethod]
        [Timeout(600000)]
        public void RebootOnSchedule()
        {
            string[] oArgsStrings = new[] { "-ip", "192.168.0.1", "-reboot", "06:30"};
            SetTestTime(6, 32);
            oNBNRebooter.Run(oArgsStrings);
            UnitTestModem oTestModem = (UnitTestModem)oNBNRebooter.RebooterProperties.CurrentModem;
            Assert.AreEqual(1, oTestModem.PeformRebootCallCount, "Check reboot command was sent");
        }
    }
}
