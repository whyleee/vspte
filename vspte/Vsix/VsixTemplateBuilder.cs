using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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
        private const string VSIX_XMLNS = "http://schemas.microsoft.com/developer/vsx-schema/2011";
        private readonly XNamespace VSIX_DESIGN_XMLNS = "http://schemas.microsoft.com/developer/vsx-schema-design/2011";

        private Project _vsixProject;
        private string _templateName;
        private string _vsixProjectDirPath;
        private string _templateExtractPath;

        public void Build(Project vsixProject, string templateZipPath)
        {
            // unzip files in template
            _templateName = Path.GetFileNameWithoutExtension(templateZipPath);
            _vsixProject = vsixProject;
            _vsixProjectDirPath = Path.GetDirectoryName(vsixProject.FullName);
            _templateExtractPath = Path.Combine(_vsixProjectDirPath, _templateName);

            ZipFile.ExtractToDirectory(templateZipPath, _templateExtractPath);

            // include to vsix project
            var rootVsixTemplateDirItem = vsixProject.ProjectItems.AddFromDirectory(_templateExtractPath);
            UpdateVsixItemProps(rootVsixTemplateDirItem);
            vsixProject.Save();

            // Extensions
            AddInstallScriptSupport();
            AddNuGetPackages();

            // build
            var msbuildExe = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe");
            var msbuildArgs = string.Format("{0} /p:Configuration=Release /m", vsixProject.FullName);
            var msbuild = System.Diagnostics.Process.Start(msbuildExe, msbuildArgs);
            msbuild.WaitForExit();

            // cleanup
            rootVsixTemplateDirItem.Remove();
            CleanupInstallScriptSupport();
            vsixProject.Save();
            try
            {
                Directory.Delete(_templateExtractPath, recursive: true);
            }
            catch (IOException)
            {
                // could probably be locked by some other process for a while, try wait a second
                System.Threading.Thread.Sleep(1000);
                Directory.Delete(_templateExtractPath, recursive: true);
            }
        }

        private void UpdateVsixItemProps(ProjectItem item)
        {
            var isFile = item.Properties.Cast<Property>().Any(p => p.Name == "BuildAction");
            if (isFile)
            {
                item.Properties.Item("BuildAction").Value = prjBuildAction.prjBuildActionContent;
                item.Properties.Item("CopyToOutputDirectory").Value = (uint)__COPYTOOUTPUTSTATE.COPYTOOUTPUTSTATE_Always;
                item.Properties.Item("VsixContentItemObjectExtender.IncludeInVSIX").Value = true;
            }

            foreach (var subItem in item.ProjectItems.Cast<ProjectItem>())
            {
                UpdateVsixItemProps(subItem);
            }
        }

        private void AddInstallScriptSupport()
        {
            var currentDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var wizardsDllPath = Path.Combine(currentDirPath, "vspte.Wizards.dll");
            var wizardsAssemblyName = AssemblyName.GetAssemblyName(wizardsDllPath).ToString();

            // Add wizard extension to vstemplate
            using (var vstemplate = Edit.Vstemplate(@in: _templateExtractPath))
            {
                var wizardExtension = new XElement("WizardExtension",
                    new XElement("Assembly", wizardsAssemblyName),
                    new XElement("FullClassName", "vspte.Wizards.RunInstallScriptWizard")
                ).FixNamespace(VSTEMPLATE_XMLNS);

                vstemplate.Doc.Root.Add(wizardExtension);
            }

            // Add wizard dll to project
            var wizardDllItem = _vsixProject.ProjectItems.AddFromFile(wizardsDllPath);
            UpdateVsixItemProps(wizardDllItem);
            _vsixProject.Save();

            // Add wizard dll as an asset to vsixmanifest
            using (var vsixmanifest = Edit.Vsixmanifest(@in: _vsixProjectDirPath))
            {
                var wizardAsset = new XElement("Asset",
                    new XAttribute("Type", "Microsoft.VisualStudio.Assembly"),
                    new XAttribute(VSIX_DESIGN_XMLNS + "Source", "File"),
                    new XAttribute("Path", "vspte.Wizards.dll"),
                    new XAttribute("AssemblyName", wizardsAssemblyName)
                ).FixNamespace(VSIX_XMLNS);

                vsixmanifest.Doc.Root.Element(XName.Get("Assets", VSIX_XMLNS)).Add(wizardAsset);
            }
        }

        private void CleanupInstallScriptSupport()
        {
            // remove wizards asset from vsixmanifest
            using (var vsixmanifest = Edit.Vsixmanifest(@in: _vsixProjectDirPath))
            {
                vsixmanifest.Doc.Root.Element(XName.Get("Assets", VSIX_XMLNS))
                    .Elements(XName.Get("Asset", VSIX_XMLNS))
                    .Where(e => e.Attribute("Type").Value == "Microsoft.VisualStudio.Assembly" &&
                                e.Attribute("AssemblyName").Value.Contains("vspte.Wizards"))
                    .Remove();
            }

            // remove wizards dll item
            var wizardsDllItem = _vsixProject.ProjectItems.Cast<ProjectItem>()
                .FirstOrDefault(item => item.Name == "vspte.Wizards.dll");

            if (wizardsDllItem != null)
            {
                wizardsDllItem.Remove();
            }
        }

        private void AddNuGetPackages()
        {
            // skip if there are no NuGet packages in the template
            if (!Directory.GetFiles(_templateExtractPath, "*.nupkg").Any())
            {
                return;
            }

            // get installed packages xml
            var nugetPackagesConfigPath = Path.Combine(_templateExtractPath, "packages.config");
            var nugetPackagesConfig = XDocument.Load(nugetPackagesConfigPath);

            // add nuget wizard extention and data
            using (var vstemplate = Edit.Vstemplate(@in: _templateExtractPath))
            {
                var wizardExtension = new XElement("WizardExtension",
                    new XElement("Assembly", "NuGet.VisualStudio.Interop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    new XElement("FullClassName", "NuGet.VisualStudio.TemplateWizard")
                ).FixNamespace(VSTEMPLATE_XMLNS);

                var wizardData = new XElement("WizardData").FixNamespace(VSTEMPLATE_XMLNS);
                nugetPackagesConfig.Root.Add(new XAttribute("repository", "template"));
                wizardData.Add(nugetPackagesConfig.Root.FixNamespace(VSTEMPLATE_XMLNS)); // TODO: replace .NET versions

                vstemplate.Doc.Root.Add(wizardExtension);
                vstemplate.Doc.Root.Add(wizardData);
            }
        }

        private static class Edit
        {
            public static EditDoc Vstemplate(string @in)
            {
                var vstemplatePath = Path.Combine(@in, "MyTemplate.vstemplate");
                return new EditDoc(XDocument.Load(vstemplatePath), vstemplatePath);
            }

            public static EditDoc Vsixmanifest(string @in)
            {
                var vsixmanifestPath = Path.Combine(@in, "source.extension.vsixmanifest");
                return new EditDoc(XDocument.Load(vsixmanifestPath), vsixmanifestPath);
            }
        }

        private class EditDoc : IDisposable
        {
            private readonly string _docLocation;

            public XDocument Doc { get; private set; }

            public EditDoc(XDocument doc, string location)
            {
                Doc = doc;
                _docLocation = location;
            }

            public void Dispose()
            {
                Doc.Save(_docLocation);
            }
        }
    }
}
