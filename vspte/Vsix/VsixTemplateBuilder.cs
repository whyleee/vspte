using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using VSLangProj;
using VSLangProj80;
using vspte.Aids;

namespace vspte.Vsix
{
    public class VsixTemplateBuilder
    {
        private const string VSTEMPLATE_XMLNS = "http://schemas.microsoft.com/developer/vstemplate/2005";

        public void Build(Project vsixProject, string templateZipPath)
        {
            // unzip files in template
            var templateName = Path.GetFileNameWithoutExtension(templateZipPath);
            var vsixProjectDirPath = Path.GetDirectoryName(vsixProject.FullName);
            var templateExtractPath = Path.Combine(vsixProjectDirPath, templateName);

            ZipFile.ExtractToDirectory(templateZipPath, templateExtractPath);

            // include to vsix project
            var rootVsixTemplateDirItem = vsixProject.ProjectItems.AddFromDirectory(templateExtractPath);
            UpdateVsixItemProps(rootVsixTemplateDirItem);
            vsixProject.Save();

            //AddNuGetPackages(templateExtractPath);

            // build
            var msbuildExe = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe");
            var msbuildArgs = string.Format("{0} /p:Configuration=Release /m", vsixProject.FullName);
            var msbuild = System.Diagnostics.Process.Start(msbuildExe, msbuildArgs);
            msbuild.WaitForExit();

            // cleanup
            rootVsixTemplateDirItem.Remove();
            vsixProject.Save();
            Directory.Delete(templateExtractPath, recursive: true);
        }

        private void UpdateVsixItemProps(ProjectItem item)
        {
            foreach (var subItem in item.ProjectItems.Cast<ProjectItem>())
            {
                var isFile = subItem.Properties.Cast<Property>().Any(p => p.Name == "BuildAction");
                if (isFile)
                {
                    subItem.Properties.Item("BuildAction").Value = prjBuildAction.prjBuildActionContent;
                    subItem.Properties.Item("CopyToOutputDirectory").Value = (uint)__COPYTOOUTPUTSTATE.COPYTOOUTPUTSTATE_Always;
                    subItem.Properties.Item("VsixContentItemObjectExtender.IncludeInVSIX").Value = true;
                }

                UpdateVsixItemProps(subItem);
            }
        }

        private void AddNuGetPackages(string templateExtractPath)
        {
            // add nuget packages. NOTE: we're using nuget restore, so skip it for now.
            var nugetPackagesConfigPath = Path.Combine(templateExtractPath, "packages.config");
            var nugetPackagesConfig = XDocument.Load(nugetPackagesConfigPath);

            var vstemplatePath = Path.Combine(templateExtractPath, "MyTemplate.vstemplate");
            var vstemplate = XDocument.Load(vstemplatePath);
            var wizardExtension = XElement.Parse(
@"<WizardExtension>
    <Assembly>NuGet.VisualStudio.Interop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</Assembly>
    <FullClassName>NuGet.VisualStudio.TemplateWizard</FullClassName>
  </WizardExtension>").FixNamespace(VSTEMPLATE_XMLNS);
            vstemplate.Root.Add(wizardExtension);

            var wizardData = new XElement("WizardData").FixNamespace(VSTEMPLATE_XMLNS);
            wizardData.Add(nugetPackagesConfig.Root.FixNamespace(VSTEMPLATE_XMLNS));
            vstemplate.Root.Add(wizardData);

            vstemplate.Save(vstemplatePath);

            // TODO: not completed
        }
    }
}
