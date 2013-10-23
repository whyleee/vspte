using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ExportTemplate;
using System;
using System.Linq;
using System.Reflection;
using vspte.Com;
using vspte.Export;
using vspte.Vsix;

namespace vspte
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Specify full .sln file path and project name to export the template");
                Environment.Exit(0);
            }

            var slnPath = args[0];
            var projectName = args[1];
            var vsix = args.Length > 2 && args[2] == "vsix";

            using (new MessageFilter())
            {
                var template = new ExportTemplatePackage();
                //var package = ExportTemplatePackage.PackageInstance;
                typeof(ExportTemplatePackage).GetField("staticPackage", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, template);

                var dteComClassName = Type.GetTypeFromProgID("VisualStudio.DTE.12.0", true);
                DTE2 dTE = null;
                Solution2 solution = null;
                try
                {
                    Console.Write("Loading Visual Studio...");
                    dTE = (DTE2)Activator.CreateInstance(dteComClassName);
                    solution = (Solution2)dTE.Solution;
                    Console.WriteLine(" OK");

                    Console.Write("Loading solution...");
                    solution.Open(slnPath);
                    Console.WriteLine(" OK");

                    Console.Write("Exporting project template...");
                    var project = solution.Projects.Cast<Project>().First(p => p.Name == projectName);
                    var wizard = new StandaloneTemplateWizardForm();
                    wizard.SetUserData("DTE", dTE);
                    wizard.SetUserData("IsProjectExport", true);
                    wizard.SetUserData("Project", project);
                    wizard.SetUserData("TemplateName", project.Name);
                    wizard.SetUserData("AutoImport", false);
                    wizard.SetUserData("ExplorerOnZip", false);

                    //typeof(ExportTemplateWizard).GetMethod("GenProjectXMLFile", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(wizard, null);
                    //wizard.OnFinish();
                    var ok = wizard.GetProjectXMLFile();
                    Console.WriteLine(ok ? " OK" : " FAIL");

                    if (!ok)
                    {
                        Console.WriteLine("Finished with error");
                        Environment.Exit(0);
                    }

                    if (vsix)
                    {
                        Console.Write("Creating VSIX package...");
                        var vsixProject = solution.Projects.Cast<Project>().First(p => p.Name == "Package");
                        var myDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var templateZipPath = Path.Combine(myDocsPath, "My Exported Templates", project.Name + ".zip");

                        new VsixTemplateBuilder().Build(vsixProject, templateZipPath);
                        Console.WriteLine(" OK");
                    }
                }
                finally
                {
                    if (solution != null)
                    {
                        solution.Close();
                    }
                    if (dTE != null)
                    {
                        dTE.Quit();
                    }
                }

                Console.WriteLine("Everything is OK!");
            }
        }
    }
}
