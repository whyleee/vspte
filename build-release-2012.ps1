# update lib refs to VS2012
.nuget\NuGet.exe install Microsoft.VisualStudio.ExportTemplate -Version 11.0 -OutputDirectory packages
(Get-Content vspte/vspte.csproj) | ForEach-Object {
    $_ -replace 'Microsoft.VisualStudio.ExportTemplate, Version=12.0.0.0', 'Microsoft.VisualStudio.ExportTemplate, Version=11.0.0.0' `
       -replace 'Microsoft.VisualStudio.ExportTemplate.12.0', 'Microsoft.VisualStudio.ExportTemplate.11.0' `
       -replace 'Microsoft.WizardFrameworkVS, Version=12.0.0.0', 'Microsoft.WizardFrameworkVS, Version=11.0.0.0' `
       -replace 'Microsoft.VisualStudio.WizardFramework.12.0', 'Microsoft.VisualStudio.WizardFramework.11.0' `
       -replace 'Microsoft.VisualStudio.Shell.12.0, Version=12.0.0.0', 'Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0'
} | Set-Content vspte/vspte.csproj

# build
& $env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe vspte/vspte.csproj /p:Configuration=Release /m
& $env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe vspte.Wizards/vspte.Wizards.csproj /p:Configuration=Release /m

# ilmerge and copy to bin-2012
mkdir -f bin-2012 > $null
cp vspte\bin\Release\vspte.exe.config bin-2012\
cp vspte\bin\Release\vspte.Wizards.dll bin-2012\
echo 'Running ILMerge...'
packages\ilmerge.2.13.0307\ILMerge.exe /ndebug /lib:"C:\Program Files (x86)\Common Files\microsoft shared\MSEnv\PublicAssemblies" /out:bin-2012/vspte.exe vspte/bin/Release/vspte.exe vspte/bin/Release/CommandLine.dll vspte/bin/Release/Microsoft.WizardFramework.dll vspte/bin/Release/Microsoft.WizardFrameworkVS.dll vspte/bin/Release/Microsoft.VisualStudio.ExportTemplate.dll

# restore back lib refs to VS2013
(Get-Content vspte/vspte.csproj) | ForEach-Object {
    $_ -replace 'Microsoft.VisualStudio.ExportTemplate, Version=11.0.0.0', 'Microsoft.VisualStudio.ExportTemplate, Version=12.0.0.0' `
       -replace 'Microsoft.VisualStudio.ExportTemplate.11.0', 'Microsoft.VisualStudio.ExportTemplate.12.0' `
       -replace 'Microsoft.WizardFrameworkVS, Version=11.0.0.0', 'Microsoft.WizardFrameworkVS, Version=12.0.0.0' `
       -replace 'Microsoft.VisualStudio.WizardFramework.11.0', 'Microsoft.VisualStudio.WizardFramework.12.0' `
       -replace 'Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0', 'Microsoft.VisualStudio.Shell.12.0, Version=12.0.0.0'
    } | Set-Content vspte/vspte.csproj

echo 'Build complete'