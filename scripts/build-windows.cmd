dotnet publish .. -f net8.0-windows10.0.19041.0 -c Release -o ../build/windows -p:RuntimeIdentifierOverride=win10-x64 -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true /p:CSharpier_Bypass=true
pause