dotnet build 
dotnet publish --output "out" --runtime win-x64 --configuration Release -p:PublishTrimmed=true -p:PublishSingleFile=true --self-contained 
del out\*.pdb

