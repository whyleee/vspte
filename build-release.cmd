"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" vspte/vspte.csproj /p:Configuration=Release /m
mkdir bin
copy vspte\bin\Release\vspte.exe.config bin\
packages\ilmerge.2.13.0307\ILMerge.exe /ndebug /lib:"C:\Program Files (x86)\Common Files\microsoft shared\MSEnv\PublicAssemblies" /out:bin/vspte.exe vspte/bin/Release/vspte.exe vspte/bin/Release/CommandLine.dll vspte/bin/Release/Microsoft.WizardFramework.dll vspte/bin/Release/Microsoft.WizardFrameworkVS.dll vspte/bin/Release/Microsoft.VisualStudio.ExportTemplate.dll