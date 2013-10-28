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
        private string _installScriptPath;
        private Dictionary<string, string> _replacements;

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            _replacements = replacementsDictionary;
            var vstemplateDirPath = Path.GetDirectoryName((string) customParams[0]);

            _installScriptPath = Directory.EnumerateFiles(vstemplateDirPath)
                .FirstOrDefault(filePath =>
                    filePath.EndsWith("install.cmd") ||
                    filePath.EndsWith("install.bat") ||
                    filePath.EndsWith("install.ps1"));
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

            if (isPsScript)
            {
                var psCommand = string.Format(@"cd '{0}'; .\install.ps1", projectPath);
                var psArgs = string.Format("-NoProfile -ExecutionPolicy Unrestricted -Command \"{0}\"", psCommand);
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
