vspte (Visual Studio Project Template Exporter)
===============================================

A command line tool to create Visual Studio project templates for existing projects (like *File->Export Template...* feature in Visual Studio, but in command line, thus can be easily automated).


Features
--------

In addition to project template generation, the tool provides next features:

 - **VSIX support**: generates VSIX package, for one-click install your project template into Visual Studio as an extension.
 
 - **NuGet support**: includes all project NuGet packages to auto-install them during project creation.
 
 - **Install scripts support**: runs install.ps1 or install.cmd scripts during project creation (yyep, like lovely NuGet feature!).
 
 - **Custom wizards support**: adds your wizards to the package and calls them during project creation.
 
 - **Different .NET and VS support**: automatically replaces .NET and VS versions in the target project according to user's selection.
 
 - **Small fixes for template generation**: project items as links with relative paths support, .ps1/.cmd file replacements etc.
 
 - **Deployed via NuGet**.


Download
--------

Install via NuGet (Visual Studio 2013):

    Install-Package vspte
    
Or if you're using Visual Studio 2012:

    Install-Package vspte.vs2012


Example of usage
----------------

Run in terminal:

    vspte -s MvcCmsTemplate.sln -p Website --vsix Package --nuget

This command will create a template for `Website` project in solution by path `.\MvcCmsTemplate.sln`, and build VSIX package including NuGet packages, using the manifest from `Package` project in the same solution.


Command-line reference
----------------------

Call `vspte` or `vspte --help` to see this help screen:

    -s, --sln        Required. Path to .sln file containing the project you wish to export a template from
    
    -p, --project    Required. The name of a project for template export
    
    --vsix           Create VSIX package with project template
    
    --nuget          Include NuGet packages to the project template
    
    --help           Display the help screen


Credits
-------

Pavel Nezhencev, 2013
