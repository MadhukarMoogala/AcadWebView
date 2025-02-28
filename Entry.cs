using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;

[assembly: ExtensionApplication(typeof(AcadWebView.Entry))]
[assembly: CommandClass(typeof(AcadWebView.Entry))]

namespace AcadWebView
{
    [SupportedOSPlatform("windows")]
    public class Entry : IExtensionApplication
    {
        public static PaletteSet ps;
        public static bool shouldBeVisible = true;
        public static string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string AcadPath = AppContext.BaseDirectory;

        public void Initialize()
        {
            ps = new PaletteSet("Web View", new Guid("D3D3D3D3-D3D3-D3D3-D3D3-D3D3D3D3D3D3"))
            {
                Style = PaletteSetStyles.ShowCloseButton,
                Size = new System.Drawing.Size(800, 800),
                DockEnabled = DockSides.Left | DockSides.Right
            };
            ps.Load += PsOnLoad;
            ps.Save += PsOnSave;

            Application.Idle += ApplicationOnIdle;
        }

        private void ApplicationOnIdle(object sender, EventArgs e)
        {
            var ribbonControl = ComponentManager.Ribbon;
            if (ribbonControl == null) return;

            Application.Idle -= ApplicationOnIdle;
        }

        private void PsOnSave(object sender, PalettePersistEventArgs e)
        {
            e.ConfigurationSection.WriteProperty("IssuesVisibility", ((PaletteSet)sender).Visible);
        }

        private void PsOnLoad(object sender, PalettePersistEventArgs e)
        {
            shouldBeVisible = (bool)e.ConfigurationSection.ReadProperty("IssuesVisibility", true);
        }

        public void Terminate()
        {
            Application.Idle -= ApplicationOnIdle;
        }

        [CommandMethod("WEBVIEWPAL")]
        public void WebViewPal()
        {
            if (ps is null)
            {
                Application.ShowAlertDialog("PaletteSet is null");
                return;
            }

            var dataJson = DataExtractionHelper.ExtractDataExtraction();
            if (string.IsNullOrEmpty(dataJson) || !File.Exists(dataJson))
            {
                Application.ShowAlertDialog("Failed to extract data.");
                return;
            }

            string jsonOutput = Path.ChangeExtension(dataJson, ".json");
            File.Copy(dataJson, jsonOutput,true);
            string html = Path.Combine(pluginPath, "Web","dashboard.html");
            var webControl = new WebControl(html, jsonOutput);
            ps.AddVisual("Web View", webControl);
            ps.Visible = true;
        }

        public class DataExtractionHelper
        {
            private static string BFMigrator => Path.Combine(AcadPath,"BFMigrator","BFMigrator.exe");
            private const string DATA_EXTRACTION_ADAPTER = "Autodesk.AutoCAD.DataExtraction.DxDataLinkAdapter";

            public static string ExtractDataExtraction()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc is null || !doc.IsNamedDrawing) return string.Empty;
                HostApplicationServices hostApplicationServices = HostApplicationServices.Current;
                if (hostApplicationServices is null) return string.Empty;
                string drawingPath = hostApplicationServices.FindFile(doc.Name, doc.Database, FindFileHint.Default);

                Database db = doc.Database;
                DataLinkManager dlm = db.DataLinkManager;
                string dxeFile = null;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    ObjectIdCollection dlIds = dlm.GetDataLink();
                    foreach (ObjectId id in dlIds)
                    {
                        var dl = tr.GetObject(id, OpenMode.ForRead) as DataLink;
                        if (dl is not null && dl.DataAdapterId.Equals(DATA_EXTRACTION_ADAPTER))
                        {
                            dxeFile = dl.ConnectionString;                            
                            break;
                        }
                    }
                    tr.Commit();
                }
                if(dxeFile.Equals(Path.GetFileName(dxeFile), StringComparison.OrdinalIgnoreCase))
                    dxeFile = Path.Combine(Path.GetDirectoryName(drawingPath), dxeFile);
                
                if (!string.IsNullOrEmpty(dxeFile) && File.Exists(dxeFile))
                {
                    string ext = Path.GetExtension(dxeFile);
                    if (ext.Equals(".dxe", StringComparison.OrdinalIgnoreCase))
                    {
                        return LaunchBFMigrator(dxeFile);
                    }
                }
                return string.Empty;
            }

            private static string LaunchBFMigrator(string dxeFile)
            {
                try
                {
                    if (!File.Exists(BFMigrator))
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nBFMigrator.exe not found.");
                        return "";
                    }

                    string dxexFile = Path.ChangeExtension(dxeFile, ".dxex");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = BFMigrator,
                        Arguments = $"\"{dxeFile}\" \"{dxexFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process process = new Process { StartInfo = psi };
                    process.Start();
                    process.WaitForExit(); // Ensure migration completes before returning

                    if (File.Exists(dxexFile))
                    {
                        return dxexFile;
                    }
                    else
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nDXEX file not created.");
                    }
                }
                catch (System.Exception ex)
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nError launching BFMigrator: {ex.Message}");
                }
                return string.Empty;
            }
        }
    }

}

