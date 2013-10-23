using EnvDTE;
using Microsoft.VisualStudio.ExportTemplate;
using Microsoft.Win32;

namespace vspte.Export
{
    public class TemplateTypePagePublic
    {
        public static string GetNameFromProject(Project proj)
        {
            string result = null;
            try
            {
                string relativeRegKeyPath = "\\Projects\\" + proj.Kind;
                RegistryKey vSMachineRegistryKey = ExportTemplatePackage.GetVSMachineRegistryKey(relativeRegKeyPath, false);
                if (vSMachineRegistryKey != null)
                {
                    object value = vSMachineRegistryKey.GetValue("Language(VsTemplate)");
                    if (value != null)
                    {
                        result = value.ToString();
                    }
                }
            }
            catch
            {
            }
            return result ?? "CSharp";
        }
    }
}
