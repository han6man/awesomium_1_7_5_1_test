using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleLogin
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //}
        public static void Main(String[] args)
        {
            Console.WriteLine("Started....");

            WebView wv = WebCore.CreateWebView(1024, 600);

            wv.Source = new Uri("https://accounts.google.com/");

            FrameEventHandler handler = null;
            handler = (s, e) =>
            {
                if (e.IsMainFrame)
                {
                    // we have finished loading main page,
                    // let's unhook ourselves
                    wv.LoadingFrameComplete -= handler;

                    LoginAndTakeScreenShot(wv);
                }
            };

            wv.LoadingFrameComplete += handler;

            WebCore.Run();
        }

        private static void LoginAndTakeScreenShot(WebView wv)
        {
            dynamic document = (JSObject)wv.ExecuteJavascriptWithResult("document");

            using (document)
            {
                //Works
                var tbox = document.getElementById("Email");
                tbox.value = "XXXXXXXX@gmail.com";

                //Works
                var pbox = document.getElementById("Passwd");
                pbox.value = "**********";

                FrameEventHandler handler = null;
                handler = (sender, args) =>
                {
                    if (args.IsMainFrame)
                    {
                        wv.LoadingFrameComplete -= handler;

                        BitmapSurface surface = (BitmapSurface)wv.Surface;
                        surface.SaveToPNG("result.png", true);
                        System.Diagnostics.Process.Start("result.png");

                        WebCore.Shutdown();
                    }
                };

                wv.LoadingFrameComplete += handler;

                var sbox = document.getElementById("signIn");
                sbox.click();
            }
        }
    }
}
