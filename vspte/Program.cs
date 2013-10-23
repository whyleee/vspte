using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ExportTemplate;
using System;
using System.Linq;
using System.Reflection;
using vspte.Com;
using vspte.Export;

namespace vspte
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Specify full .sln file path and project name to export the template");
                Environment.Exit(0);
            }

            var slnPath = args.First();
            var projectName = args.Last();

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
                    dTE = (DTE2)Activator.CreateInstance(dteComClassName);
                    solution = (Solution2)dTE.Solution;
                    solution.Open(slnPath);
                    var project = dTE.Solution.Projects.Cast<Project>().First(p => p.Name == projectName);
                    var wizard = new StandaloneTemplateWizardForm();
                    wizard.SetUserData("DTE", dTE);
                    wizard.SetUserData("IsProjectExport", true);
                    wizard.SetUserData("Project", project);
                    wizard.SetUserData("TemplateName", project.Name);
                    wizard.SetUserData("AutoImport", false);

                    //typeof(ExportTemplateWizard).GetMethod("GenProjectXMLFile", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(wizard, null);
                    //wizard.OnFinish();
                    var ok = wizard.GetProjectXMLFile();
                    Console.WriteLine(ok ? "Project template is successfully exported" : "Error, project template is not exported");
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
            }
        }
    }
}
