[Setup]
AppName=ArtLockImage
AppVersion=1.0
DefaultDirName={commonpf}\ArtLockImage
DisableDirPage=yes
OutputBaseFilename=ArtLockImageInstaller
Compression=lzma
SolidCompression=yes
AppPublisher=rostok
AppPublisherURL=https://github.com/rostok/
AppSupportURL=https://github.com/rostok/artlockimage
PrivilegesRequired=admin
UninstallDisplayIcon={app}\artlockimage.exe

[Dirs]
Name: {app}; Permissions: everyone-modify

[Files]
Source: "out\artlockimage.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "out\urls"; DestDir: "{app}"; Flags: ignoreversion

[CustomMessages]
InstallScheduledTask=Install scheduled task for ArtLockImage
UnblurBackgroundLogonScreen=Disable blurred background logon screen
DisableWindowsSpotlight=Disable Windows spotlight
BlurBackgroundLogonScreen=Enable blurred background logon screen
EnableWindowsSpotlight=Enable Windows spotlight

[Tasks]
Name: "installtask"; Description: "{cm:InstallScheduledTask}"; Flags: checkedonce
Name: "unblur"; Description: "{cm:UnblurBackgroundLogonScreen}"; Flags: checkedonce
Name: "disablespotlight"; Description: "{cm:DisableWindowsSpotlight}"; Flags: checkedonce
Name: "firewallexception"; Description: "Add firewall exception for ArtLockImage"; Flags: checkedonce

[Code]
var
  InstallTask, UnblurBackground, DisableSpotlight: Boolean;
  BlurBackground, EnableSpotlight: Boolean;
  FirewallException: Boolean;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    if InstallTask then
    begin
      Exec(ExpandConstant('{app}\artlockimage.exe'), '-ct -t', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
    if UnblurBackground then
    begin
      RegWriteDWordValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Policies\Microsoft\Windows\System', 'DisableAcrylicBackgroundOnLogon', 1);
    end;
    if DisableSpotlight then
    begin
      RegWriteDWordValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Policies\Microsoft\Windows\CloudContent', 'DisableWindowsSpotlightFeatures', 1);
    end;
    if FirewallException then
    begin
      Exec('netsh', 'advfirewall firewall add rule name="ArtLockImage" dir=in action=allow program="' + ExpandConstant('{app}\artlockimage.exe') + '" enable=yes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    Exec(ExpandConstant('{app}\artlockimage.exe'), '-dt', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

    if BlurBackground then
    begin
      RegWriteDWordValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Policies\Microsoft\Windows\System', 'DisableAcrylicBackgroundOnLogon', 0);
    end;

    if EnableSpotlight then
    begin
      RegWriteDWordValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Policies\Microsoft\Windows\CloudContent', 'DisableWindowsSpotlightFeatures', 0);
    end;
  end;
end;

procedure InitializeWizard();
begin
  InstallTask := True;
  UnblurBackground := True;
  DisableSpotlight := True;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = wpSelectTasks then
  begin
    InstallTask := WizardIsTaskSelected('installtask');
    UnblurBackground := WizardIsTaskSelected('unblur');
    DisableSpotlight := WizardIsTaskSelected('disablespotlight');
    FirewallException := WizardIsTaskSelected('firewallexception');
  end
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[UninstallRun]
Filename: {uninstallexe}; Parameters: "{app}"; RunOnceId: "ArtLockImage";

[UninstallTasks]
Name: "blurbackground"; Description: "{cm:BlurBackgroundLogonScreen}"; Flags: unchecked
Name: "enablespotlight"; Description: "{cm:EnableWindowsSpotlight}"; Flags: unchecked

