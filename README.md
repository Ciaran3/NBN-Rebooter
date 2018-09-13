# NBN Rebooter
Schedule daily reboots of your NBN modem as well as logging sync rate statistics.

## Description:
NBN Rebooter allows for logging of modem sync rates as well as scheduled reboot of the modem.

If you have the NBN FTTN (Fibre to the Node) solution and a poor quality connection, NBN Co may apply dynamic line provisioning (DLM) to stabilise your line.

This also means you sync rates (and therefore download/upload speed) will drop over time. Rebooting of your modem (should) reset your sync rate to its original maximum speed. The statistics provided by NBN Rebooter can also help you determine if your issue is getting worse over time.

## Modem Compatibility:
* F@ST™ 3864AC (Optus NBN supplied modem)

Please let me know if it works with other modems. If it does not, you can log an issue to request support.

## Download:
######  Pre-Compiled EXE:
 [Coming Soon]

######  Manually compile (Optional):
If you do not want to run the pre-complied EXE file, or would like to make your own modifications, you can grab the source code in compile it yourself.

1. Install Visual Studio. https://visualstudio.microsoft.com/vs/community/
2. Download the source code.
3. Open the NBNRebooter.sln file.
4. Build in Release mode.
5. Ready to go!

## Installation:
You must run NBN Rebooter on a machine that is on the same network as your modem. To test this, ensure that you can access your modem page
(http://192.168.0.1 by default) through your web browser.

## Windows:
 [Coming Soon]
## Raspberry Pi / Linux:
 [Coming Soon]

## Usage:

I run this application on a Raspberry Pie that runs 24/7 to ensure my modem is rebooted daily at 6am. I also log the sync statistics so I can monitor the line issue. To run in this configuration the command line would be:

```
-ip 192.168.0.1 -reboot 06:00 -log Rebooter.Log
```
