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

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            var vstemplateDirPath = Path.GetDirectoryName((string) customParams[0]);
            var extensionDirPath = Directory.GetParent(vstemplateDirPath).FullName;

            _installScriptPath = Directory.EnumerateFiles(extensionDirPath)
                .FirstOrDefault(filePath =>
                    filePath.EndsWith("install.cmd") ||
                    filePath.EndsWith("install.bat") ||
                    filePath.EndsWith("install.ps1"));
        }

        public void ProjectFinishedGenerating(Project project)
        {
            if (_installScriptPath == null)
            {
                return;
            }

            var isPsScript = _installScriptPath.EndsWith("ps1");
            System.Diagnostics.Process installer;

            if (isPsScript)
            {
                var args = string.Format("-NoProfile -ExecutionPolicy Unrestricted -File \"{0}\"", _installScriptPath);
                installer = System.Diagnostics.Process.Start("powershell", args);
            }
            else
            {
                var args = string.Format("/C {0}", _installScriptPath);
                installer = System.Diagnostics.Process.Start("cmd", args);
            }

            if (installer != null)
            {
                installer.WaitForExit();
            }
        }

        // Not used
        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {}
        public bool ShouldAddProjectItem(string filePath) {return true;}
        public void BeforeOpeningFile(ProjectItem projectItem) {}
        public void RunFinished() {}
    }
}
