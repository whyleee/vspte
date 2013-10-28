# build
& $env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe vspte/vspte.csproj /p:Configuration=Release /m
& $env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe vspte.Wizards/vspte.Wizards.csproj /p:Configuration=Release /m

# ilmerge and copy to bin-2013
mkdir -f bin-2013 > $null
cp vspte\bin\Release\vspte.exe.config bin-2013\
cp vspte\bin\Release\vspte.Wizards.dll bin-2013\
echo 'Running ILMerge...'
packages\ilmerge.2.13.0307\ILMerge.exe /ndebug /lib:"C:\Program Files (x86)\Common Files\microsoft shared\MSEnv\PublicAssemblies" /out:bin-2013/vspte.exe vspte/bin/Release/vspte.exe vspte/bin/Release/CommandLine.dll vspte/bin/Release/Microsoft.WizardFramework.dll vspte/bin/Release/Microsoft.WizardFrameworkVS.dll vspte/bin/Release/Microsoft.VisualStudio.ExportTemplate.dll

echo 'Build complete'