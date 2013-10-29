using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace vspte.Wizards
{
    public class RunInstallScriptWizard : IWizard
    {
        private DTE _dte;
        private string _installScriptPath;
        private Dictionary<string, string> _replacements;

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            _dte = (DTE) automationObject;
            _replacements = replacementsDictionary;
            var vstemplateDirPath = Path.GetDirectoryName((string) customParams[0]);

            _installScriptPath = Directory.EnumerateFiles(vstemplateDirPath)
                .FirstOrDefault(filePath =>
                    filePath.EndsWith("install.cmd") ||
                    filePath.EndsWith("install.bat") ||
                    filePath.EndsWith("install.ps1"));

            // VS versions
            _replacements.Add("$visualstudioversion$", _dte.Version);
            _replacements.Add("$toolsversion$", GetToolsVersion(_dte.Version));

            // NuGet support
            var targetFramework = _replacements["$targetframeworkversion$"];
            _replacements.Add("$nugettargetframeworkversion$", "net" + targetFramework.Replace(".", ""));
        }

        private string GetToolsVersion(string vsVersion)
        {
            if (vsVersion == "11.0" || vsVersion == "10.0") return "4.0";
            return vsVersion;
        }

        public void RunFinished()
        {
            if (_installScriptPath == null)
            {
                return;
            }

            var isPsScript = _installScriptPath.EndsWith("ps1");
            var projectPath = _replacements["$destinationdirectory$"];
            System.Diagnostics.Process installer;

            _dte.StatusBar.Text = "Running install script...";

            if (isPsScript)
            {
#if DEBUG
                var noExit = " -NoExit";
#else
                var noExit = "";
#endif
                var psCommand = string.Format(@"$VSVersion='{0}'; cd '{1}'; .\install.ps1", _dte.Version, projectPath);
                var psArgs = string.Format("-NoProfile{0} -ExecutionPolicy Unrestricted -Command \"{1}\"", noExit, psCommand);
                var powershell = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\SysNative\WindowsPowerShell\v1.0\powershell.exe");
                installer = System.Diagnostics.Process.Start(powershell, psArgs);
            }
            else
            {
                // TODO: not tested yet
                var args = string.Format("/C cd \"{0}\" & {1}", projectPath, Path.GetFileName(_installScriptPath));
                installer = System.Diagnostics.Process.Start("cmd", args);
            }

            if (installer != null)
            {
                installer.WaitForExit();
            }
        }

        // Not used
        public void ProjectFinishedGenerating(Project project) {}
        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {}
        public bool ShouldAddProjectItem(string filePath) {return true;}
        public void BeforeOpeningFile(ProjectItem projectItem) {}
    }
}
