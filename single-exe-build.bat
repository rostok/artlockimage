dotnet build -p:GenerateAssemblyInfo=false
::dotnet publish --output "out" --runtime win-x64 --configuration Debug -p:PublishTrimmed=true -p:PublishSingleFile=true --self-contained
dotnet publish --output "out" --runtime win-x64 --configuration Release -p:PublishTrimmed=true -p:PublishSingleFile=true --self-contained
del out\*.pdb

