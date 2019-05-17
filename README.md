# HTTPTest
Source and deliverables for simple HTTP endpoints using JSON.

Assumptions: 
1)	You are executing on a PC machine running Windows 7 or higher.
2)	You have administrative rights with your Windows log in.
3)	You have downloaded the files HTTPTest.exe and Newtonsoft.Json.dll to a location on your computer included in both the PATH and     LIBPATH environment variables.

To run, navigate to the folder containing the executable, right click on it, and select “Run as Administrator”.

Alternatively, locate the command line executable (cmd.exe or command.exe, dependind on your Windows version), right click on it, select “Run as Administrator”, and navigate to the directory containing the executable HTTPTest.exe.  Type “httptest” to run the program.

To redirect the output from the program so that you can see it a screen at a time, type “httptest | more” and press spacebar to see the next screen.  To redirect the output from the program to a file so that you can read it at your leisure, type “httptest > output” and open the file named “output” in the current directory using something such as Notepad, or send the file named “output” to more using “type output | more”.

To see the program source code, download Program.cs.

