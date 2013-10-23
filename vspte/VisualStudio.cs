using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.ExportTemplate;
using vspte.Com;
using vspte.Export;
using vspte.Vsix;

namespace vspte
{
    public class VisualStudio : IDisposable
    {
        private IMessageFilter _messageFilter;
        private DTE _dte;

        private TextWriter Log { get; set; }

        private VisualStudio(TextWriter log)
        {
            Log = log ?? TextWriter.Null;
        }

        public static VisualStudio Create(TextWriter logTo = null)
        {
            var vs = new VisualStudio(logTo);
            vs.Log.Write("Loading Visual Studio...");

            vs._messageFilter = new MessageFilter();

            var dteComClassName = Type.GetTypeFromProgID("VisualStudio.DTE.12.0", true); // TODO: without version?
            vs._dte = (DTE) Activator.CreateInstance(dteComClassName);

            vs.Log.WriteLine(" OK");
            return vs;
        }

        public virtual void OpenSolution(string slnPath)
        {
            Log.Write("Loading solution...");

            slnPath = Path.GetFullPath(slnPath);
            _dte.Solution.Open(slnPath);

            Log.WriteLine(" OK");
        }

        public virtual void ExportTemplate(string projectName)
        {
            Log.Write("Exporting project template...");

            var template = new ExportTemplatePackage();
            //var package = ExportTemplatePackage.PackageInstance;
            typeof(ExportTemplatePackage)
                .GetField("staticPackage", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, template);

            var project = _dte.Solution.Projects.Cast<Project>().First(p => p.Name == projectName);
            var wizard = new StandaloneTemplateWizardForm();
            wizard.SetUserData("DTE", _dte);
            wizard.SetUserData("IsProjectExport", true);
            wizard.SetUserData("Project", project);
            wizard.SetUserData("TemplateName", project.Name);
            wizard.SetUserData("AutoImport", false);
            wizard.SetUserData("ExplorerOnZip", false);

            //typeof(ExportTemplateWizard)
            //    .GetMethod("GenProjectXMLFile", BindingFlags.Instance | BindingFlags.NonPublic)
            //    .Invoke(wizard, null);
            //wizard.OnFinish();

            wizard.GetProjectXMLFile();
            Log.WriteLine(" OK");
        }

        public virtual void CreateVsix(string templateName)
        {
            Log.Write("Creating VSIX package...");

            var vsixProject = _dte.Solution.Projects.Cast<Project>().First(p => p.Name == "Package"); // TODO: hardcoded name
            var myDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var templateZipPath = Path.Combine(myDocsPath, "My Exported Templates", templateName + ".zip");

            new VsixTemplateBuilder().Build(vsixProject, templateZipPath);
            Log.WriteLine(" OK");
        }

        public void Dispose()
        {
            if (_dte.Solution != null)
            {
                _dte.Solution.Close();
            }
            if (_dte != null)
            {
                _dte.Quit();
            }
            if (_messageFilter is IDisposable)
            {
                ((IDisposable) _messageFilter).Dispose();
            }
        }
    }
}
