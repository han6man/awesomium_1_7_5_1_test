using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class AutomationSample
    {
        WebView webView;
        bool running = true;
        public bool finishedLoading = false;

        public AutomationSample()
        {
            // Setup WebCore
            //WebCore.Config conf = new WebCore.Config();
            WebConfig conf = WebConfig.Default;
            WebCore.Initialize(conf);

            // Setup WebView
            //webView = WebCore.CreateWebview(1024, 768);
            webView = WebCore.CreateWebView(1024, 768);

            // Setup Callbacks
            //webView.OnFinishLoading += onFinishLoading;
            //webView.OnBeginLoading += onBeginLoading;
            webView.LoadingFrameComplete += WebView_LoadingFrameComplete;
            webView.LoadingFrame += WebView_LoadingFrame;

            Console.WriteLine("Loading Form...");
            //webView.LoadURL("http://form.jotform.com/form/11023750372");
            webView.Source = new Uri("http://form.jotform.com/form/11023750372");
        }

        

        public void update()
        {
            while (running == true)
            {
                WebCore.Update();
                if (finishedLoading == true)
                {
                    runCommands();
                }
                Console.WriteLine("Waiting...");
                Thread.Sleep(4000);  // Wait 4 seconds between each command
            }
        }

        static int commandNumber = 1;
        public void runCommands()
        {
            switch (commandNumber)
            {
                case 1:
                    {
                        Console.WriteLine("Typing Username...");
                        webView.ExecuteJavascript("document.getElementById('input_3').value='Username'");
                        break;
                    }

                case 2:
                    {
                        Console.WriteLine("Typing Password...");
                        webView.ExecuteJavascript("document.getElementById('input_4').value='Password'");
                        break;
                    }

                case 3:
                    {
                        Console.WriteLine("Typing First Name...");
                        click(410, 150);
                        typeKeys("John");
                        break;
                    }

                case 4:
                    {
                        Console.WriteLine("Typing Last Name...");
                        click(410, 197);
                        typeKeys("Doe");
                        break;
                    }

                case 5:
                    {
                        Console.WriteLine("Selecting Gender as Male...");
                        webView.ExecuteJavascript("document.getElementById('input_7_0').checked=true");
                        break;
                    }

                case 6:
                    {
                        Console.WriteLine("Selecting 'Search Engine' in combo box...");
                        webView.ExecuteJavascript("document.getElementById('input_8').value='Search Engine'");
                        break;
                    }

                default:
                    {
                        //webView.Render().SaveToPNG("result.png", true);
                        BitmapSurface surface = (BitmapSurface)webView.Surface;
                        surface.SaveToPNG("result.png", true);
                        System.Diagnostics.Process.Start("result.png");
                        Console.WriteLine("Done running all commands, shutting down webcore...");
                        WebCore.Shutdown();
                        running = false;
                        break;
                    }
            }
            commandNumber++;
        }

        public void click(int x, int y)
        {
            webView.InjectMouseMove(x, y);
            webView.InjectMouseDown(MouseButton.Left);
            webView.InjectMouseMove(x, y);
            webView.InjectMouseUp(MouseButton.Left);
        }

        public void typeKeys(string key)
        {
            for (int i = 0; i < key.Length; i++){

                WebKeyboardEvent keyEvent = new WebKeyboardEvent();

                //keyEvent.type = WebKeyType.KeyDown;
                keyEvent.Type = WebKeyboardEventType.KeyDown;
                //keyEvent.virtualKeyCode = key[i];
                keyEvent.VirtualKeyCode = (VirtualKey) key[i];
                webView.InjectKeyboardEvent(keyEvent);

                //keyEvent.type = WebKeyType.Char;
                keyEvent.Type = WebKeyboardEventType.Char;
                //keyEvent.text = new ushort[] { key[i], 0, 0, 0 };
                keyEvent.Text = new ushort[] { key[i], 0, 0, 0 }.ToString();
                webView.InjectKeyboardEvent(keyEvent);

                //keyEvent.type = WebKeyType.KeyUp;
                keyEvent.Type = WebKeyboardEventType.KeyUp;
                //keyEvent.virtualKeyCode = key[i];
                keyEvent.VirtualKeyCode = (VirtualKey)key[i];
                webView.InjectKeyboardEvent(keyEvent);
            }
        }

        //public void onFinishLoading(object sender, WebView.FinishLoadingEventArgs e)
        //{
        //    finishedLoading = true;
        //}

        private void WebView_LoadingFrameComplete(object sender, FrameEventArgs e)
        {
            finishedLoading = true;
        }

        //public void onBeginLoading(object sender, WebView.BeginLoadingEventArgs e)
        //{
        //    finishedLoading = false;
        //}

        private void WebView_LoadingFrame(object sender, LoadingFrameEventArgs e)
        {
            finishedLoading = false;
        }

        //public static void Main(string[] args)
        //{
        //    AutomationSample main = new AutomationSample();
        //    main.update();
        //}

    }
}
