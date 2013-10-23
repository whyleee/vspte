using CommandLine;
using CommandLine.Text;
using System;

namespace vspte
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            using (var vs = VisualStudio.Create(logTo: Console.Out))
            {
                vs.OpenSolution(options.SlnPath);
                vs.ExportTemplate(options.ProjectName);

                if (options.CreateVsix)
                {
                    vs.CreateVsix(options.ProjectName);
                }
            }

            Console.WriteLine("Everything is OK!");
        }
    }

    class Options
    {
        [Option('s', "sln", Required = true, HelpText = "Path to .sln file containing the project you wish to export a template from")]
        public string SlnPath { get; set; }

        [Option('p', "project", Required = true, HelpText = "The name of a project for template export")]
        public string ProjectName { get; set; }

        [Option('x', "vsix", HelpText = "Create VSIX package with project template")]
        public bool CreateVsix { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = HelpText.AutoBuild(this, current =>
                {
                    current.MaximumDisplayWidth = int.MaxValue;
                    HelpText.DefaultParsingErrorsHandler(this, current);
                });

            return help;
        }
    }
}
