# ArtLockImage
Sets Windows 10 lock screen background to random art or your chosen image.

# Usage
ArtLockImage changes logon screen background to either filename passed as argument or by reading and downloading random image from links provided in urls file.
The format for each line is: `link;author;title`.
If no urls file is avaiable ~1000 image links are downloaded from most-famous-paintings.com website.

syntax & options: 

    artlockimage [imagefile|options]

    options:

    imagefile  sets lock screen image without any transformation
    -ct        creates 'ArtLockImage' scheduled windows task triggerred logoff
    -t         use temporary path for image storage");
    -h         shows this help

# Installation
Place artlockimage.exe somewhere safe and run 
    
    artlockimage -ct

This should create task. To remove the task run

    schtasks /delete /tn ArtLockImage /f

The folder should be writable. In case it is not you can run `artlockimage -ct -t` and temprorary folder will be used for all images.

Note that Windows come with some weird logon screen options. 

## Unblurring background
If you do not want to have background blurred add this to registry:

    Windows Registry Editor Version 5.00

    [HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System]
    "DisableAcrylicBackgroundOnLogon"=dword:00000001


## Disabling Windows Spotlight

    Reg Add "HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent" /T REG_DWORD /V "DisableWindowsSpotlightFeatures" /D 1 /F  

# Building
Clone the repo and build running:

    dotnet build

However if you want a clean single exe run also:

    dotnet publish --output "out" --runtime win-x64 --configuration Release -p:PublishTrimmed=true -p:PublishSingleFile=true --self-contained 
    del out\*.pdb

Now you have artlockimage.exe in out folder. Move urls file there or it will be downladed automatically.

# License
MIT