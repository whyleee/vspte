using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.ExportTemplate.ExportTemplateWizard;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace vspte.Export
{
    // Decompiled and fixed from Microsoft.VisualStudio.ExportTemplate.dll
    public class StandaloneTemplateWizardForm : TemplateWizardForm
    {
        private int m_uniqueID;

        public void GetProjectXMLFile()
        {
            XmlDocument xmlDocument = new XmlDocument();
            DTE dTE = (DTE)GetUserData("DTE");
            Project project = (Project)GetUserData("Project");
            List<string> list = new List<string>();
            IVsProject vsProject = null;
            IntPtr zero = IntPtr.Zero;
            try
            {
                var serviceProvider = dTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                Guid gUID = typeof(IVsSolution).GUID;
                Guid gUID2 = typeof(SVsSolution).GUID;
                if (serviceProvider.QueryService(ref gUID2, ref gUID, out zero) == 0)
                {
                    IVsSolution vsSolution = (IVsSolution)Marshal.GetObjectForIUnknown(zero);
                    IVsHierarchy vsHierarchy = null;
                    if (vsSolution.GetProjectOfUniqueName(project.UniqueName, out vsHierarchy) == 0)
                    {
                        vsProject = (IVsProject)vsHierarchy;
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                }
            }
            //string text = wizard.CreateTempDirectory();
            string text = Path.GetTempFileName();
            File.Delete(text);
            Directory.CreateDirectory(text);
            string rootNamespace = "";
            try
            {
                rootNamespace = (string)project.Properties.Item("RootNamespace").Value;
            }
            catch
            {
            }
            var targetFramework = (string) project.Properties.Item("TargetFrameworkMoniker").Value;
            var frameworkVersion = targetFramework.Substring(targetFramework.IndexOf("Version=v") + "Version=v".Length);
            var visualStudioVersion = dTE.Version;
            XmlNode xmlNode = xmlDocument.CreateElement("VSTemplate");
            XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("Version");
            xmlAttribute.Value = "3.0.0";
            xmlNode.Attributes.Append(xmlAttribute);
            XmlAttribute xmlAttribute2 = xmlDocument.CreateAttribute("xmlns");
            xmlAttribute2.Value = "http://schemas.microsoft.com/developer/vstemplate/2005";
            xmlNode.Attributes.Append(xmlAttribute2);
            XmlAttribute xmlAttribute3 = xmlDocument.CreateAttribute("Type");
            xmlAttribute3.Value = "Project";
            xmlNode.Attributes.Append(xmlAttribute3);
            xmlNode = xmlDocument.AppendChild(xmlNode);
            XmlNode xmlNode2 = xmlDocument.CreateElement("TemplateData");
            xmlNode2 = xmlNode.AppendChild(xmlNode2);
            XmlNode xmlNode3 = xmlDocument.CreateElement("Name");
            xmlNode3.InnerText = (string)GetUserData("TemplateName");
            xmlNode3 = xmlNode2.AppendChild(xmlNode3);
            XmlNode xmlNode4 = xmlDocument.CreateElement("Description");
            xmlNode4.InnerText = (string)GetUserData("TemplateDescription");
            xmlNode4 = xmlNode2.AppendChild(xmlNode4);
            XmlNode xmlNode5 = xmlDocument.CreateElement("ProjectType");
            string nameFromProject = TemplateTypePagePublic.GetNameFromProject(project);
            xmlNode5.InnerText = nameFromProject;
            xmlNode5 = xmlNode2.AppendChild(xmlNode5);
            XmlNode xmlNode6 = xmlDocument.CreateElement("ProjectSubType");
            xmlNode6.InnerText = (string)GetUserData("TemplateCategory");
            xmlNode6 = xmlNode2.AppendChild(xmlNode6);
            XmlNode xmlNode7 = xmlDocument.CreateElement("SortOrder");
            xmlNode7.InnerText = "1000";
            xmlNode7 = xmlNode2.AppendChild(xmlNode7);
            XmlNode xmlNode8 = xmlDocument.CreateElement("CreateNewFolder");
            xmlNode8.InnerText = "true";
            xmlNode8 = xmlNode2.AppendChild(xmlNode8);
            XmlNode xmlNode9 = xmlDocument.CreateElement("DefaultName");
            xmlNode9.InnerText = (string)GetUserData("TemplateName");
            xmlNode9 = xmlNode2.AppendChild(xmlNode9);
            XmlNode xmlNode10 = xmlDocument.CreateElement("ProvideDefaultName");
            xmlNode10.InnerText = "true";
            xmlNode10 = xmlNode2.AppendChild(xmlNode10);
            XmlNode xmlNode11 = xmlDocument.CreateElement("LocationField");
            xmlNode11.InnerText = "Enabled";
            xmlNode11 = xmlNode2.AppendChild(xmlNode11);
            XmlNode xmlNode12 = xmlDocument.CreateElement("EnableLocationBrowseButton");
            xmlNode12.InnerText = "true";
            xmlNode12 = xmlNode2.AppendChild(xmlNode12);
            XmlNode xmlNode13 = xmlDocument.CreateElement("TemplateContent");
            xmlNode13 = xmlNode.AppendChild(xmlNode13);
            XmlNode xmlNode14 = xmlDocument.CreateElement("Project");
            xmlNode14 = xmlNode13.AppendChild(xmlNode14);
            XmlAttribute xmlAttribute4 = xmlDocument.CreateAttribute("TargetFileName");
            string text2;
            if (TemplateTypePage.IsWebProject(project))
            {
                text2 = (xmlAttribute4.Value = "ProjectName.webproj");
                using (StreamWriter streamWriter = new StreamWriter(Path.Combine(text, text2)))
                {
                    string text3 = (string)project.Properties.Item("CurrentWebSiteLanguage").Value;
                    if (string.Compare(text3, "Visual Basic", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        text3 = "VB";
                    }
                    else
                    {
                        if (string.Compare(text3, "Visual C#", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            text3 = "C#";
                        }
                    }
                    streamWriter.Write(text3);
                    streamWriter.Write("\n$targetframeworkversion$");
                    streamWriter.Close();
                    goto IL_442;
                }
            }
            string fileName = Path.GetFileName(project.FullName);
            text2 = GetZipSafeName(fileName, text);
            //var xmlGenerationResult = typeof(TemplateWizardForm).GetMethod("MakeReplacements", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(wizard, new object[] {true, true, true, rootNamespace, project.FullName, Path.Combine(text, text2)}).ToString();
            var xmlGenerationResult = MakeReplacements(true, true, true, rootNamespace, project.FullName, Path.Combine(text, text2), frameworkVersion, visualStudioVersion);
            if (xmlGenerationResult != "OK")
            {
                throw new InvalidOperationException("MakeReplacements finished with error");
            }
            xmlAttribute4.Value = fileName;
        IL_442:
            xmlAttribute4 = xmlNode14.Attributes.Append(xmlAttribute4);
            XmlAttribute xmlAttribute5 = xmlDocument.CreateAttribute("File");
            xmlAttribute5.Value = text2;
            xmlAttribute5 = xmlNode14.Attributes.Append(xmlAttribute5);
            list.Add(text2);
            XmlAttribute xmlAttribute6 = xmlDocument.CreateAttribute("ReplaceParameters");
            xmlAttribute6.Value = "true";
            xmlAttribute6 = xmlNode14.Attributes.Append(xmlAttribute6);
            //var xmlGenerationResult2 = typeof(TemplateWizardForm).GetMethod("WalkProject", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { TemplateWizardForm.GetSccEnlistmentPathTranslation(dTE), vsProject, nameFromProject, text, "", project.ProjectItems, xmlDocument, xmlNode14, list, TemplateTypePage.IsWebProject(project) }).ToString();
            var xmlGenerationResult2 = WalkProject(GetSccEnlistmentPathTranslation(dTE), vsProject, nameFromProject, text, "", project.ProjectItems, xmlDocument, xmlNode14, list, TemplateTypePage.IsWebProject(project), frameworkVersion, visualStudioVersion);
            if (xmlGenerationResult2 != "OK")
            {
                throw new InvalidOperationException("WalkProject finished with error");
            }
            typeof(TemplateWizardForm).GetMethod("SaveIcon", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { text, xmlDocument, xmlNode2, list });
            //wizard.SaveIcon(text, xmlDocument, xmlNode2, list);
            typeof(TemplateWizardForm).GetMethod("SavePreviewImage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { text, xmlDocument, xmlNode2, list });
            //wizard.SavePreviewImage(text, xmlDocument, xmlNode2, list);
            xmlDocument.Save(Path.Combine(text, "MyTemplate.vstemplate"));
            list.Add("MyTemplate.vstemplate");
            // NUGET
            var includeNuGetPackages = (bool) GetUserData("IncludeNuGetPackages");
            if (includeNuGetPackages)
            {
                var packagesDirPath = Path.Combine(Path.GetDirectoryName(dTE.Solution.FullName), "packages");
                // TODO: will only work, if all NuGet packages installed locally
                var nupkgs = Directory.GetFiles(packagesDirPath, "*.nupkg", SearchOption.AllDirectories);
                foreach (var nupkg in nupkgs)
                {
                    var nupkgName = Path.GetFileName(nupkg);
                    File.Copy(nupkg, Path.Combine(text, nupkgName));
                    list.Add(nupkgName);
                }
            }
            // ENDOF NUGET
            // delete existing template
            var templateZipPath = Path.Combine(GetExportedTemplatesDirectory(), (string) GetUserData("TemplateName") + ".zip");
            if (File.Exists(templateZipPath))
            {
                File.Delete(templateZipPath);
            }
            // endof delete existing template
            var zipresult = typeof(TemplateWizardForm).GetMethod("ExportZipFiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { false, project, text, list }).ToString();
            if (zipresult != "OK")
            {
                throw new InvalidOperationException("ExportZipFiles finished with error");
            }
            //return wizard.ExportZipFiles(false, project, text, list);
        }

        private string GetZipSafeName(string itemName, string outputDirectory)
        {
            string text = DeleteToDot(itemName);
            bool flag = IsLegalZipFileName(itemName);
            if (!flag)
            {
                while (!IsLegalZipFileName(text))
                {
                    text = DeleteToDot(text.Substring(1));
                }
            }
            if (!flag || File.Exists(Path.Combine(outputDirectory, itemName)))
            {
                itemName = GenerateUniqueName(outputDirectory, text);
            }
            return itemName;
        }

        internal static bool IsLegalZipFileName(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c >= '\u0080')
                {
                    return false;
                }
            }
            return true;
        }

        private static string DeleteToDot(string fileName)
        {
            int num = fileName.IndexOf('.');
            if (num == -1)
            {
                return "";
            }
            return fileName.Substring(num);
        }

        private string GenerateUniqueName(string outputDirectory, string extension)
        {
            string text;
            do
            {
                string arg_24_0 = "vstg";
                int num = ++m_uniqueID;
                text = arg_24_0 + num.ToString("d4") + extension;
            }
            while (File.Exists(Path.Combine(outputDirectory, text)));
            return text;
        }

        private string MakeReplacements(bool isAProject, bool creatingProject, bool fDoReplacements, string rootNamespace, string source, string dest, string frameworkVersion, string visualStudioVersion)
        {
            if (fDoReplacements)
            {
                string text = "";
                //uint num = 0u;
                //try
                //{
                    //typeof(TemplateWizardForm).GetMethod("GetFileDataAndEncoding", BindingFlags.Instance | BindingFlags.Public).Invoke(wizard, new object[] { source, text, num });
                    //GetFileDataAndEncoding(wizard, source, out text, out num);
                    text = File.ReadAllText(source);
                //}
                //catch (Exception ex)
                //{
                //    ResourceManager resourceManager = new ResourceManager("Strings", Assembly.GetExecutingAssembly());
                //    string @string = resourceManager.GetString("appTitle");
                //    string text2 = resourceManager.GetString("readFailed");
                //    text2 = string.Format(text2, ex.Message);
                //    MessageBox.Show(text2, @string, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //    return "DontFail";
                //}
                if (isAProject)
                {
                    if (!string.IsNullOrEmpty(rootNamespace))
                    {
                        text = text.Replace("<StartupObject>" + rootNamespace + ".", "<StartupObject>$safeprojectname$.");
                        text = text.Replace("<DocumentationFile>" + rootNamespace + ".", "<DocumentationFile>$safeprojectname$.");
                        text = text.Replace("<RootNamespace>" + rootNamespace + "</RootNamespace>", "<RootNamespace>$safeprojectname$</RootNamespace>");
                        text = text.Replace("<AssemblyName>" + rootNamespace + "</AssemblyName>", "<AssemblyName>$safeprojectname$</AssemblyName>");
                        text = text.Replace("<XapFilename>" + rootNamespace + ".", "<XapFilename>$safeprojectname$.");
                        text = text.Replace("<SilverlightAppEntry>" + rootNamespace + ".", "<SilverlightAppEntry>$safeprojectname$.");
                        text = text.Replace("<TargetFrameworkVersion>v" + frameworkVersion + "</TargetFrameworkVersion>", "<TargetFrameworkVersion>v$targetframeworkversion$</TargetFrameworkVersion>");
                        text = text.Replace("<Project ToolsVersion=\"" + GetToolsVersion(visualStudioVersion) + "\"", "<Project ToolsVersion=\"$toolsversion$\"");
                    }
                }
                else
                {
                    if (creatingProject)
                    {
                        Project project = (Project)GetUserData("Project");
                        string simpleProjectName = GetSimpleProjectName(project);
                        text = WordReplace(text, simpleProjectName, "$safeprojectname$");
                        if (!string.IsNullOrEmpty(rootNamespace))
                        {
                            text = WordReplace(text, rootNamespace, "$safeprojectname$");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(rootNamespace))
                        {
                            text = WordReplace(text, rootNamespace, "$rootnamespace$");
                        }
                        string text3 = Path.GetFileNameWithoutExtension(source);
                        int num2 = text3.LastIndexOf(".");
                        if (num2 != -1)
                        {
                            text3 = text3.Substring(0, num2);
                        }
                        text = WordReplace(text, text3, "$safeitemname$");
                    }

                    var loweredFileName = Path.GetFileName(source).ToLower();
                    var tfTemplate = "targetFramework=\"{0}\"";

                    // CONFIGS
                    if (loweredFileName == "app.config" || loweredFileName == "web.config")
                    {
                        text = WordReplace(text, string.Format(tfTemplate, frameworkVersion), string.Format(tfTemplate, "$targetframeworkversion$"));
                    }
                    // NUGET
                    if (loweredFileName == "packages.config")
                    {
                        var includeNuGetPackages = (bool) GetUserData("IncludeNuGetPackages");
                        if (includeNuGetPackages)
                        {
                            text = WordReplace(text, string.Format(tfTemplate, "net" + frameworkVersion.Replace(".", "")), string.Format(tfTemplate, "$nugettargetframeworkversion$"));
                        }
                    }
                    // MSBUILD
                    if (loweredFileName.EndsWith(".cmd") || loweredFileName.EndsWith(".bat") || loweredFileName.EndsWith(".ps1"))
                    {
                        text = WordReplace(text, "/p:VisualStudioVersion=" + visualStudioVersion, "/p:VisualStudioVersion=$visualstudioversion$");
                    }
                }
                //int num3 = (int)(num & 65535u);
                //bool flag = (num & 65536u) != 0u;
                //int num4 = num3;
                Encoding encoding = Encoding.UTF8;
                //switch (num4)
                //{
                //    case 1200:
                //        encoding = new UnicodeEncoding(false, flag);
                //        break;
                //    case 1201:
                //        encoding = new UnicodeEncoding(true, flag);
                //        break;
                //    default:
                //        if (num4 != 65001)
                //        {
                //            encoding = Encoding.GetEncoding(num3);
                //        }
                //        else
                //        {
                //            encoding = new UTF8Encoding(flag);
                //        }
                //        break;
                //}
                using (StreamWriter streamWriter = new StreamWriter(dest, false, encoding))
                {
                    streamWriter.Write(text);
                }
                if (isAProject)
                {
                    RemoveSourceControlAnnotations(dest);
                }
            }
            else
            {
                //try
                //{
                    File.Copy(source, dest);
                //}
                //catch (Exception ex2)
                //{
                //    ResourceManager resourceManager2 = new ResourceManager("Strings", Assembly.GetExecutingAssembly());
                //    string string2 = resourceManager2.GetString("appTitle");
                //    string text4 = resourceManager2.GetString("readFailed");
                //    text4 = string.Format(text4, ex2.Message);
                //    MessageBox.Show(text4, string2, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //    return "DontFail";
                //}
            }
            return "OK";
        }

        private string GetToolsVersion(string vsVersion)
        {
            if (vsVersion == "11.0" || vsVersion == "10.0") return "4.0";
            return vsVersion;
        }

        private string WalkProject(IVsSccEnlistmentPathTranslation vsSccEnlistmentPathTranslation, IVsProject vsProject, string projectTypeString, string zipFileRoot, string zipLocalPath, ProjectItems projItems, XmlDocument xmlDoc, XmlNode xmlProjectContentsNode, List<string> accumulatedFiles, bool bIsWebProj, string frameworkVersion, string visualStudioVersion)
        {
            var xmlGenerationResult = "OK";
            if (projItems != null)
            {
                foreach (ProjectItem projectItem in projItems)
                {
                    string text = Path.Combine(zipFileRoot, zipLocalPath);
                    string text2 = projectItem.get_FileNames(1);
                    string name = projectItem.Name;
                    if (!bIsWebProj || string.Compare(name, "Generated___Files", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        string zipSafeName = this.GetZipSafeName(name, text);
                        if (Directory.Exists(text2))
                        {
                            XmlNode xmlNode = xmlDoc.CreateElement("Folder");
                            xmlProjectContentsNode.AppendChild(xmlNode);
                            XmlAttribute xmlAttribute = xmlDoc.CreateAttribute("Name");
                            xmlAttribute.Value = zipSafeName;
                            xmlNode.Attributes.Append(xmlAttribute);
                            XmlAttribute xmlAttribute2 = xmlDoc.CreateAttribute("TargetFolderName");
                            xmlAttribute2.Value = name;
                            xmlNode.Attributes.Append(xmlAttribute2);
                            string text3 = Path.Combine(zipLocalPath, zipSafeName);
                            Directory.CreateDirectory(Path.Combine(zipFileRoot, text3));
                            xmlGenerationResult = this.WalkProject(vsSccEnlistmentPathTranslation, vsProject, projectTypeString, zipFileRoot, text3, projectItem.ProjectItems, xmlDoc, xmlNode, accumulatedFiles, bIsWebProj, frameworkVersion, visualStudioVersion);
                            if (xmlGenerationResult != "OK")
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (IsFileALink(vsSccEnlistmentPathTranslation, vsProject, projectItem))
                            {
                                xmlGenerationResult = this.WalkProject(vsSccEnlistmentPathTranslation, vsProject, projectTypeString, zipFileRoot, zipLocalPath, projectItem.ProjectItems, xmlDoc, xmlProjectContentsNode, accumulatedFiles, bIsWebProj, frameworkVersion, visualStudioVersion);
                                if (xmlGenerationResult != "OK")
                                {
                                    break;
                                }
                            }
                            else
                            {
                                string text4 = Path.GetDirectoryName(projectItem.get_FileNames(1)).ToUpperInvariant();
                                string text5 = Path.GetDirectoryName(projectItem.ContainingProject.FullName).ToUpperInvariant();
                                if (!text4.Equals(text5) && text4.StartsWith(text5) && projectTypeString.Equals("VC"))
                                {
                                    zipLocalPath = text4.Substring(text5.Length + 1);
                                    text = Path.Combine(zipFileRoot, zipLocalPath);
                                    if (!Directory.Exists(text))
                                    {
                                        Directory.CreateDirectory(text);
                                    }
                                    zipSafeName = this.GetZipSafeName(name, text);
                                }
                                if (!IsInRestrictedList(name))
                                {
                                    XmlNode xmlNode2 = xmlDoc.CreateElement("ProjectItem");
                                    xmlProjectContentsNode.AppendChild(xmlNode2);
                                    bool fDoReplacements = ValidMimeType(text2) && !projectTypeString.Equals("VC");
                                    XmlAttribute xmlAttribute3 = xmlDoc.CreateAttribute("ReplaceParameters");
                                    xmlAttribute3.Value = fDoReplacements.ToString().ToLowerInvariant();
                                    xmlNode2.Attributes.Append(xmlAttribute3);
                                    XmlAttribute xmlAttribute4 = xmlDoc.CreateAttribute("TargetFileName");
                                    xmlAttribute4.Value = GetTargetItemName(name, projectTypeString);
                                    xmlNode2.Attributes.Append(xmlAttribute4);
                                    if (projectTypeString.Equals("VC"))
                                    {
                                        xmlNode2.InnerText = Path.Combine(zipLocalPath, zipSafeName);
                                    }
                                    else
                                    {
                                        xmlNode2.InnerText = zipSafeName;
                                    }
                                    accumulatedFiles.Add(Path.Combine(zipLocalPath, zipSafeName));
                                    string rootNamespace = "";
                                    try
                                    {
                                        rootNamespace = (string)projectItem.ContainingProject.Properties.Item("RootNamespace").Value;
                                    }
                                    catch
                                    {
                                    }
                                    xmlGenerationResult = this.MakeReplacements(false, true, fDoReplacements, rootNamespace, text2, Path.Combine(text, zipSafeName), frameworkVersion, visualStudioVersion);
                                    if (xmlGenerationResult != "OK")
                                    {
                                        break;
                                    }
                                    xmlGenerationResult = this.WalkProject(vsSccEnlistmentPathTranslation, vsProject, projectTypeString, zipFileRoot, zipLocalPath, projectItem.ProjectItems, xmlDoc, xmlProjectContentsNode, accumulatedFiles, bIsWebProj, frameworkVersion, visualStudioVersion);
                                    if (xmlGenerationResult != "OK")
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return xmlGenerationResult;
        }

        //private static void GetFileDataAndEncoding(TemplateWizardForm wizard, string source, out string fileData, out uint encValue)
        //{
        //    DTE dTE = (DTE)wizard.GetUserData("DTE");
        //    var serviceProvider = dTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
        //    IntPtr zero = IntPtr.Zero;
        //    Guid guid = new Guid("{00000000-0000-0000-C000-000000000046}");
        //    Guid gUID = typeof(ILocalRegistry).GUID;
        //    ErrorHandler.ThrowOnFailure(serviceProvider.QueryService(ref gUID, ref guid, out zero));
        //    var unknown = GetObjectFromNativeUnknown(zero);
        //    ILocalRegistry localRegistry = (ILocalRegistry)unknown;
        //    IntPtr zero2 = IntPtr.Zero;
        //    Guid gUID2 = typeof(VsTextBufferClass).GUID;
        //    ErrorHandler.ThrowOnFailure(localRegistry.CreateInstance(gUID2, null, ref guid, 1u, out zero2));
        //    IVsTextBuffer vsTextBuffer = (IVsTextBuffer)GetObjectFromNativeUnknown(zero2);
        //    ((IObjectWithSite)vsTextBuffer).SetSite(serviceProvider);
        //    IVsUserData vsUserData = (IVsUserData)vsTextBuffer;
        //    Guid guid2 = new Guid("{17F375AC-C814-11d1-88AD-0000F87579D2}");
        //    ErrorHandler.ThrowOnFailure(vsUserData.SetData(ref guid2, false));
        //    IPersistFileFormat persistFileFormat = (IPersistFileFormat)vsTextBuffer;
        //    ErrorHandler.ThrowOnFailure(persistFileFormat.Load(source, 0u, 0));
        //    int num = 0;
        //    IVsTextStream vsTextStream = (IVsTextStream)vsTextBuffer;
        //    ErrorHandler.ThrowOnFailure(vsTextStream.GetSize(out num));
        //    IntPtr intPtr = Marshal.AllocCoTaskMem((num + 1) * 2);
        //    try
        //    {
        //        ErrorHandler.ThrowOnFailure(vsTextStream.GetStream(0, num, intPtr));
        //        object obj = null;
        //        Guid guid3 = new Guid("{16417F39-A6B7-4c90-89FA-770D2C60440B}");
        //        ErrorHandler.ThrowOnFailure(vsUserData.GetData(ref guid3, out obj));
        //        encValue = (uint)obj;
        //        fileData = Marshal.PtrToStringUni(intPtr);
        //    }
        //    finally
        //    {
        //        ((IVsPersistDocData)vsTextBuffer).Close();
        //        Marshal.FreeCoTaskMem(intPtr);
        //    }
        //}

        //private static object GetObjectFromNativeUnknown(IntPtr nativeUnknown)
        //{
        //    object result = null;
        //    if (nativeUnknown != IntPtr.Zero)
        //    {
        //        try
        //        {
        //            result = Marshal.GetObjectForIUnknown(nativeUnknown);
        //        }
        //        finally
        //        {
        //            Marshal.Release(nativeUnknown);
        //        }
        //    }
        //    return result;
        //}

        private static bool RemoveSourceControlAnnotations(string projectFile)
        {
            bool flag = false;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(projectFile);
                flag = RemoveSourceControlAnnotations(xmlDocument.FirstChild);
                if (flag)
                {
                    xmlDocument.Save(projectFile);
                }
            }
            catch (Exception)
            {
            }
            return flag;
        }

        private static bool RemoveSourceControlAnnotations(XmlNode xmlNode)
        {
            bool result = false;
            while (xmlNode != null)
            {
                XmlNode nextSibling = xmlNode.NextSibling;
                string name = xmlNode.Name;
                if (name == "SccProjectName" || name == "SccAuxPath" || name == "SccLocalPath" || name == "SccProvider")
                {
                    xmlNode.ParentNode.RemoveChild(xmlNode);
                    result = true;
                }
                else
                {
                    if (RemoveSourceControlAnnotations(xmlNode.FirstChild))
                    {
                        result = true;
                    }
                }
                xmlNode = nextSibling;
            }
            return result;
        }

        internal static bool IsFileALink(IVsSccEnlistmentPathTranslation vsSccEnlistmentPathTranslation, IVsProject vsProject, ProjectItem projItem)
        {
            string text = Path.GetDirectoryName(projItem.get_FileNames(1)).ToUpperInvariant();
            string text2 = Path.GetDirectoryName(projItem.ContainingProject.FullName).ToUpperInvariant();
            if (text.StartsWith(text2, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (text2.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || text2.StartsWith("https:", StringComparison.OrdinalIgnoreCase) || text2.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase))
            {
                string value = projItem.ContainingProject.FullName.ToUpperInvariant();
                try
                {
                    if (vsProject is IVsSccProjectProviderBinding)
                    {
                        IVsSccProjectProviderBinding vsSccProjectProviderBinding = (IVsSccProjectProviderBinding)vsProject;
                        int num = 0;
                        string text3;
                        if (vsSccProjectProviderBinding.TranslateEnlistmentPath(EnlistmentPathTranslation(vsSccEnlistmentPathTranslation, text), out num, out text3) >= 0)
                        {
                            text3 = text3.ToUpperInvariant();
                            if (text3.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                return true;
            }
            return true;
        }

        private static string EnlistmentPathTranslation(IVsSccEnlistmentPathTranslation vsSccEnlistmentPathTranslation, string filePath)
        {
            if (vsSccEnlistmentPathTranslation != null)
            {
                try
                {
                    string text;
                    string text2;
                    if (vsSccEnlistmentPathTranslation.TranslateProjectPathToEnlistmentPath(filePath, out text, out text2) >= 0)
                    {
                        filePath = text.ToUpperInvariant();
                    }
                }
                catch (Exception)
                {
                }
            }
            return filePath;
        }

        internal static bool IsInRestrictedList(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string[] array = new string[]
	            {
		            ".suo",
		            ".user",
		            ".ncb",
		            ".incr",
		            ".projdata",
		            ".tlb",
		            ".olb",
		            ".resources",
		            ".old",
		            ".exp",
		            ".lib",
		            ".obj",
		            ".pch",
		            ".idb"
	            };
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string strA = array2[i];
                if (string.Compare(strA, extension, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ValidMimeType(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string extensionMimeType = GetExtensionMimeType(extension);
            if (extensionMimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            string[] array = new string[]
	            {
		            ".cpp",
		            ".h",
		            ".cxx",
		            ".hxx",
		            ".cs",
		            ".txt",
		            ".vb",
		            ".resx",
		            ".aspx",
		            ".ascx",
		            ".asax",
		            ".css",
		            ".master",
		            ".skin",
		            ".cpp",
		            ".h",
		            ".jsl",
		            ".csproj",
		            ".vbproj",
		            ".vjsproj",
		            ".vcproj",
		            ".il",
		            ".settings",
		            ".myapp",
		            ".config",
		            ".reg",
		            ".rgs",
		            ".vstemplate",
		            ".vscontent",
		            ".xsd",
		            ".ashx",
		            ".datasource",
		            ".generictest",
		            ".java",
		            ".loadtest",
		            ".map",
		            ".htm",
		            ".html",
		            ".mht",
		            ".mtx",
		            ".orderedtest",
		            ".settings",
		            ".sql",
		            ".testrunconfig",
		            ".webtest",
		            ".wsdl",
		            ".wsf",
		            ".xml",
		            ".xslt",
		            ".inf",
		            ".ini",
		            ".xaml",
		            ".mcml",
		            ".js",
		            ".vbs",
		            ".c",
		            ".inl",
		            ".cshtml",
		            ".vbhtml",
                    // extended
                    ".cmd",
                    ".bat",
                    ".ps1"
	            };
            for (int i = 0; i < array.Length; i++)
            {
                string strB = array[i];
                if (string.Compare(extension, strB, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetExtensionMimeType(string ext)
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(ext, false);
            if (registryKey == null)
            {
                return "";
            }
            return ((string)registryKey.GetValue("Content Type", "")).ToUpperInvariant();
        }

        private string GetTargetItemName(string originalItemName, string projectTypeString)
        {
            if (projectTypeString.Equals("VC") && originalItemName.EndsWith(".vcxproj.filters", StringComparison.OrdinalIgnoreCase))
            {
                return "$projectname$.vcxproj.filters";
            }
            return originalItemName;
        }
    }
}
