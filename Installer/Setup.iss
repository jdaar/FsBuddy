; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "FsBuddy"
#define MyAppVersion "1.0"
#define MyAppPublisher "Jhonatan David Asprilla Arango"
#define MyAppURL "https://alertbud.co/fsbuddy"
#define MyAppExeName "Client.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{A90A4C64-1376-4636-9563-6FEB37396F1A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline
OutputDir=C:\Users\root\source\repos\fsBuddy\Installer
OutputBaseFilename=FsBuddySetup
SetupIconFile=C:\Users\root\Downloads\fsbuddy_icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
AlwaysRestart=yes  

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\root\source\repos\fsBuddy\Client\bin\Release\net6.0-windows7.0\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\root\source\repos\fsBuddy\Client\bin\Release\net6.0-windows7.0\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\root\source\repos\fsBuddy\Service\bin\Release\net6.0\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{sys}\sc.exe"; Parameters: "create fsbuddy start=auto binPath=""{app}\Service.exe""" ; Flags: runhidden

[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop fsbuddy" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "delete fsbuddy" ; Flags: runhidden
