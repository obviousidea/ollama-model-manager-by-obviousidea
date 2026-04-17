#define MyAppName "Ollama Model Manager"
#define MyAppPublisher "ObviousIdea"
#define MyAppExeName "OllamaModelManagerByObviousIdea.exe"
#define MyAppVersion "2026.04.17.3"
#define MyAppSourceDir "C:\Users\fabri\OneDrive\Documents\codex\ollama optimisation\OllamaModelManagerByObviousIdea\release\win-x64"

[Setup]
AppId={{7E9A36FC-E850-4CA4-955A-2B1F4D7DDF44}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=C:\Users\fabri\OneDrive\Documents\codex\ollama optimisation\OllamaModelManagerByObviousIdea\release\installer
OutputBaseFilename=OllamaModelManagerSetup-{#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#MyAppSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
