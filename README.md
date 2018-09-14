# NBN Rebooter
Schedule daily reboots of your NBN modem as well as logging sync rate statistics.

![screenshot](https://raw.githubusercontent.com/Ciaran3/NBN-Rebooter/master/screenshot.PNG)

## Description:
NBN Rebooter is a console application that allows for logging of your modem's sync rates as well a daily reboot of the modem.

If you have the NBN FTTN (Fibre to the Node) solution and a poor quality connection, [NBN Co](https://www.nbnco.com.au/) may apply dynamic line management (DLM) to stabilise your line.

This also means you sync rates (and therefore download/upload speed) will drop over time. Rebooting of your modem (should) reset your sync rate to its original maximum speed. The statistics provided by NBN Rebooter can also help you determine if your issue is getting worse over time.

######  Read More ...
* [NBN Co plan to use DLM likened to 'putting lipstick on a pig'.](https://www.itwire.com/telecoms-and-nbn/78913-nbn-co-plan-to-use-dlm-likened-to-putting-lipstick-on-a-pig.html)
* [NBN's fibre-to-the-node connections won't provide top speeds to all consumers, figures show.](http://www.abc.net.au/news/2018-01-17/nbn-fttn-will-not-provide-top-speeds-to-three-quarters-consumers/9335602)
* [UNSW researchers find FTTP NBN worth the extra billions.](https://www.itnews.com.au/news/unsw-researchers-find-fttp-nbn-worth-the-extra-billions-512368)

## Modem Compatibility:
* F@ST™ 3864AC (Optus NBN supplied modem)
* More support possible, if requested.

Please let me know if it works with other modems. If it does not, you can log an issue to request support.

## Download:
######  Pre-Compiled EXE:
 [Coming Soon]

######  Manually compile (Optional):
If you do not want to run the pre-complied EXE file, or would like to make your own modifications, you can grab the source code in compile it yourself.

1. Install Visual Studio. https://visualstudio.microsoft.com/vs/community/
2. Download the source code.
3. Open the NBNRebooter.csproj file.
4. Build in Release mode.

Ready to go!

## Installation:
You must run NBN Rebooter on a machine that is on the same network as your modem. To test this, ensure that you can access your modem page
(http://192.168.0.1 by default) through your web browser.

## Windows:
1. Ensure you have .NET framework 4.5 or above installed (Windows Vista and above).
2. Download and extract the ZIP file above.
3. Open command prompt and navigate to the directory to run NBN Rebooter.
4. Leave the console window open.

Example:
```
cd C:\Downloads\NBNRebooter
NBNRebooter.exe -ip 192.168.0.1 -reboot 06:30 -log Rebooter.Log
```
 
## Raspberry Pi / Linux:
You will need to install *Mono* to allow NBN Rebooter to run. 
1. Follow these steps to setup *Mono*:
https://www.mono-project.com/download/stable/#download-lin-raspbian
2. Download and extract the ZIP file above.
3. Open command prompt and navigate to the directory to run NBN Rebooter.
4. Leave the console window open.

Example:
```
cd /home/pi/scripts/NBNRebooter 
mono NBNRebooter.exe -ip 192.168.0.1 -reboot 06:30 -log Rebooter.Log
```

## Usage:

I run this application on a Raspberry Pi that runs 24/7 to ensure my modem is rebooted daily at 6:30 am. I also save the sync statistics to a log file so that I can monitor the line issue. 

Perform reboot at 6.30 am and log statistics to "Rebooter.Log" in the same directory as the EXE.

```
-ip 192.168.0.1 -reboot 06:00 -log Rebooter.Log
```

Log statistics, but do not reboot:
```
-ip 192.168.0.1 -log Rebooter.Log
```

Reboot daily, but don't save statistics:
```
-ip 192.168.0.1 -reboot 06:00
```

Reboot immediately:
```
-ip 192.168.0.1 -rebootnow
```

## Parameters:
* ```-ip``` 
Enter the IP address to your modem (required).

* ```-log```
Enter path to where you want to save the statistics (optional).

* ```-reboot``` 
Enter the time you would like to schedule the reboot at (optional). 
Must be in 24 hour format. For Example:
'06:30' - 6.30 am.
'23:00' - 11 pm.

* ```-rebootnow```
Perform a reboot as soon as NBN Rebooter starts (optional). 

* ```-username```
Username to login to your modem page (optional).

* ```-password```
Password to login to your modem page (optional).
