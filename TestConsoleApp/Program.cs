using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //AutomationSample main = new AutomationSample();
            //main.update();

            WebConfig config = WebConfig.Default;
            
            WebCore.Initialize(config);
            Uri url = new Uri("http://www.google.com");

            WebPreferences prefs = WebPreferences.Default;
            //prefs.ProxyConfig = "198.1.99.26:3128";
            //prefs.CustomCSS = "body { overflow:hidden; }";
            //prefs.WebSecurity = false;
            //prefs.DefaultEncoding = "UTF-8";

            using (WebSession session = WebCore.CreateWebSession(prefs))
            {
                // WebView implements IDisposable. Here we demonstrate
                // wrapping it in a using statement.
                using (WebView webView = WebCore.CreateWebView(1366, 768, session, WebViewType.Offscreen))
                {
                    bool finishedLoading = false;
                    bool finishedResizing = false;

                    Console.WriteLine(String.Format("Loading: {0} ...", url));

                    // Load a URL.
                    webView.Source = url;

                    // This event is fired when a frame in the
                    // page finished loading.
                    webView.LoadingFrameComplete += (s, e) =>
                    {
                        Console.WriteLine(String.Format("Frame Loaded: {0}", e.FrameId));

                        // The main frame usually finishes loading last for a given page load.
                        if (e.IsMainFrame)
                            finishedLoading = true;
                    };

                    while (!finishedLoading)
                    {
                        Thread.Sleep(100);
                        // A Console application does not have a synchronization
                        // context, thus auto-update won't be enabled on WebCore.
                        // We need to manually call Update here.
                        WebCore.Update();
                    }

                    // Print some more information.
                    Console.WriteLine(String.Format("Page Title: {0}", webView.Title));
                    Console.WriteLine(String.Format("Loaded URL: {0}", webView.Source));
                    //webView.Render().SaveToPNG("result.png", true);
                    //System.Diagnostics.Process.Start("result.png");
                    BitmapSurface surface = (BitmapSurface)webView.Surface;
                    surface.SaveToPNG("result.png", true);
                    System.Diagnostics.Process.Start("result.png");

                } // Destroy and dispose the view.
            } // Release and dispose the session.            

            // Shut down Awesomium before exiting.
            WebCore.Shutdown();

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }
    }
}
