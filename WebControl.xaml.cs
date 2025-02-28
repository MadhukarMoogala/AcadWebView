using Autodesk.AutoCAD.GraphicsInterface;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcadWebView
{
    /// <summary>
    /// Interaction logic for WebControl.xaml
    /// </summary>
    /// 
    [SupportedOSPlatform("windows")]
    public partial class WebControl : UserControl
    {
        private string _source; // Store source URL for later use
        private string _json;

        public WebControl(string source, string json)
        {
            InitializeComponent();
            _source = source; // Store the URL
            _json = json;
            InitializeAsync(); // Call async initializer
        }

        private async void InitializeAsync()
        {
            // Use LocalAppData instead of Roaming AppData
            string appDataFolderACAD = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcadWebView");
            Directory.CreateDirectory(appDataFolderACAD);

            // Create WebView2 environment
            var env = await CoreWebView2Environment.CreateAsync(null, appDataFolderACAD);

            // Ensure WebView2 is ready before setting Source
            await Wv.EnsureCoreWebView2Async(env);
            Wv.CoreWebView2.Settings.IsWebMessageEnabled = true;

            // Now that WebView2 is initialized, set the Source URL
            Wv.CoreWebView2.Navigate(new Uri(_source).AbsoluteUri);
            Wv.CoreWebView2.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    await SendJsonToWebView();
                }
                else
                {
                    Wv.CoreWebView2.PostWebMessageAsJson("{\"error\": \"Failed to load the web page\"}");
                }
            };

        }
        private async Task SendJsonToWebView()
        {
            if (!File.Exists(_json))
            {
                Console.WriteLine("JSON file not found.");
                string errorJson = "{\"error\": \"JSON file not found\"}";
                Wv.CoreWebView2.PostWebMessageAsString(errorJson);
                return;
            }

            string jsonContent = await File.ReadAllTextAsync(_json);

            try
            {
                // Parse JSON to verify it's valid
                var parsedJson = System.Text.Json.JsonSerializer.Deserialize<object>(jsonContent);
                string formattedJson = System.Text.Json.JsonSerializer.Serialize(parsedJson);

               
                Wv.CoreWebView2.PostWebMessageAsString(formattedJson);
            }
            catch (Exception)
            {
                
                Wv.CoreWebView2.PostWebMessageAsString("{\"error\": \"Invalid JSON format\"}");
            }
        }


    }
}
