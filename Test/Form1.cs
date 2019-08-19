using Awesomium.Windows.Forms;
using MDH.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        private bool loadingFrameFailed = false;
        private bool translated;
        private int framesLoaded = 1;
        private bool isDomReady;
        private bool isMainFrame = false;
        //GoogleTranslate googleTranslate = new GoogleTranslate();

        public Form1()
        {
            InitializeComponent();
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrame(object sender, Awesomium.Core.LoadingFrameEventArgs e)
        {
            loadingFrameFailed = false;
            toolStripStatusLabel3.Text = string.Empty;

            webControl1.BackColor = Color.FromArgb(255, 0, 34, 51);
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrameComplete(object sender, Awesomium.Core.FrameEventArgs e)
        {
            richTextBox1.AppendText("WebControl_LoadingFrameComplete webControl1.IsDocumentReady = " + webControl1.IsDocumentReady + " webControl1.IsLoading " + webControl1.IsLoading + " e.IsMainFrame " + e.IsMainFrame + "\n");
            isMainFrame = e.IsMainFrame;

            if (framesLoaded < 3)
                framesLoaded++;
            else
            {
                framesLoaded = 1;
                isDomReady = true;
                //if (isDomReady)
                //    source = webBrowser1.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString();
                //btnGetXML.Enabled = true;
            }

            if (webControl1.IsLoading || !webControl1.IsDocumentReady)
            {
                richTextBox1.AppendText("WebControl_LoadingFrameComplete return\n");
                return;
            }

            //if (e.IsMainFrame)
            //    Translate();

            Parse();
            
        }

        private void Awesomium_Windows_Forms_WebControl_DocumentReady(object sender, Awesomium.Core.DocumentReadyEventArgs e)
        {
            richTextBox1.AppendText("WebControl_DocumentReady webControl1.IsDocumentReady = " + webControl1.IsDocumentReady + " webControl1.IsLoading " + webControl1.IsLoading + "\n");
            if (webControl1.IsLoading || !webControl1.IsDocumentReady)
            {
                richTextBox1.AppendText("WebControl_DocumentReady return\n");
                return;
            }

            //Translate();            
        }

        private void Awesomium_Windows_Forms_WebControl_AddressChanged(object sender, Awesomium.Core.UrlEventArgs e)
        {
            richTextBox1.AppendText("Awesomium_Windows_Forms_WebControl_AddressChanged webControl1.IsDocumentReady = " + webControl1.IsDocumentReady + " webControl1.IsLoading " + webControl1.IsLoading + "\n");
            if (webControl1.IsLoading || !webControl1.IsDocumentReady)
            {
                richTextBox1.AppendText("Awesomium_Windows_Forms_WebControl_AddressChanged return\n");
                return;
            }
            webControl1.Source = webControl1.Source;
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrameFailed(object sender, Awesomium.Core.LoadingFrameFailedEventArgs e)
        {
            toolStripStatusLabel3.Text = e.EventName;
            loadingFrameFailed = true;
            MessageBox.Show(e.EventName);
        }

        private void Awesomium_Windows_Forms_WebControl_TitleChanged(object sender, Awesomium.Core.TitleChangedEventArgs e)
        {
            this.Text = e.Title;
        }

        private void Awesomium_Windows_Forms_WebControl_TargetURLChanged(object sender, Awesomium.Core.UrlEventArgs e)
        {
            //mouse hover link mouse leave link on web page
            toolStripStatusLabel1.Text = e.Url.ToString();
        }

        private void AddressBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void AddressBox1_Navigate(object sender, Awesomium.Core.UrlEventArgs e)
        {
            webControl1.Source = e.Url;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = string.Empty;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text == string.Empty)
                translated = false;
            else
                translated = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string url = String.Format("https://translate.google.com/#view=home&op=translate&sl={0}&tl={1}&text={2}", "auto", "iw", "Text");
            //WebClient webClient = new WebClient();            
            //webClient.Encoding = System.Text.Encoding.UTF8;            
            //string result = webClient.DownloadString(url);
            //string url = String.Format("https://translate.google.com/#view=home&op=translate&sl={0}&tl={1}&text={2}", "auto", "ru", textBox1.Text);
            //MessageBox.Show(googleTranslate.Translate(textBox1.Text, "en", "ru"));
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webControl1.Source = new Uri("https://translate.google.com/");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            webControl1.ExecuteJavascript("document.getElementById('source').value='Mein Name ist Raimund'");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string sResult = webControl1.ExecuteJavascriptWithResult("document.getElementById('result_box').textContent");
            MessageBox.Show(sResult);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text = Translate(textBox1.Text, "ru");
        }


        //string p = "^.*<span title=\"\">";
        //Regex regex = new Regex(p);
        //MatchCollection matches = regex.Matches(s);
        //MatchCollection m = Regex.Matches(s, p, RegexOptions.None);
        //
        //int c = matches.Count;
        //if (c > 0)
        //{
        //    foreach (Match match in matches)
        //        ;//textBox2.Text = textBox2.Text + match.Value;
        //}
        private void Parse()
        {
            string s = webControl1.HTML;

            //Clipboard.Clear();
            //webControl1.SelectAll();
            //webControl1.CopyHTML();
            //s = Clipboard.GetText();

            int substringIndex = s.IndexOf("<span title=\"\">");
            if (substringIndex == -1)
            {
                return;
            }
            s = s.Substring(substringIndex + 15);

            substringIndex = s.IndexOf("</span></span>");
            if (substringIndex == -1)
            {
                richTextBox1.AppendText("Translate() return\n");
                return;
            }
            s = s.Remove(substringIndex);

            s = s.Replace("<br>", "\r\n");
            s = s.Replace("</span>", "");
            s = s.Replace("<span title=\"\">", "");

            textBox2.Text = s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textToTranslate"></param>
        /// <param name="fromLng"></param>
        /// <param name="toLng"></param>
        /// <returns></returns>
        private string Translate(string textToTranslate, string fromLng, string toLng)
        {
            Uri url = new Uri(String.Format("https://translate.google.com/#view=home&op=translate&sl={0}&tl={1}&text={2}", fromLng, toLng, textToTranslate));
            webControl1.Source = url;
            webControl1.Source = url;

            while (!translated)
            {
                Application.DoEvents();
            }

            return textBox2.Text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textToTranslate"></param>
        /// <param name="toLng"></param>
        /// <returns></returns>
        private string Translate(string textToTranslate, string toLng)
        {
            Uri url = new Uri(String.Format("https://translate.google.com/#view=home&op=translate&sl={0}&tl={1}&text={2}", "auto", toLng, textToTranslate));
            webControl1.Source = url;
            webControl1.Source = url;
            while (!translated)
            {
                Application.DoEvents();
            }

            return textBox2.Text;
        }

        private void Awesomium_Windows_Forms_WebControl_Crashed(object sender, Awesomium.Core.CrashedEventArgs e)
        {
            switch (e.Status)
            {
                case Awesomium.Core.TerminationStatus.Abnormal:
                    //NewMessage(this, "Abnormal Termination of browser ", this.GetBrowserIndex());
                    break;
                case Awesomium.Core.TerminationStatus.Crashed:
                    //NewMessage(this, "Crashed Termination of browser ", this.GetBrowserIndex());
                    break;
                case Awesomium.Core.TerminationStatus.Killed:
                    //NewMessage(this, "Killed Termination of browser ", this.GetBrowserIndex());
                    break;
                case Awesomium.Core.TerminationStatus.None:
                    //NewMessage(this, "None Termination of browser ", this.GetBrowserIndex());
                    break;
                case Awesomium.Core.TerminationStatus.Normal:
                    //NewMessage(this, "Normal Termination of browser ", this.GetBrowserIndex());
                    break;
                case Awesomium.Core.TerminationStatus.StillRunning:
                    //NewMessage(this, "Browser Still Running ", this.GetBrowserIndex());
                    break;
                default:
                    break;
            }
        }

        public static void StyleWebcontrolScrollbars(ref Awesomium.Windows.Forms.WebControl webcontrol)
        {
            var script = @" var css = ""::-webkit-scrollbar { width: 12px; } ::-webkit-scrollbar-track { background-color: #111111;	} ::-webkit-scrollbar-thumb { background-color: #444444; } ::-webkit-scrollbar-thumb:hover { background-color: #5e5e5e;	}"";
                     var style = document.createElement('style');
                     if (style.styleSheet)
                     {
                         style.styleSheet.cssText = css;
                     }
                     else
                     {
                         style.appendChild(document.createTextNode(css));
                     }
                     document.getElementsByTagName('head')[0].appendChild(style);
                     ";
            webcontrol.ExecuteJavascript(script);
        }

        /// <summary>
        /// Show some basic page information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Awesomium_Windows_Forms_WebControl_ShowPageInfo(object sender, Awesomium.Core.PageInfoEventArgs e)
        {
            string res = "Certificate Error: " + e.Info.CertError.ToString();
            res += Environment.NewLine;
            res += e.Info.ContentStatus.ToString();
            res += Environment.NewLine;
            res += e.Info.SecurityStatus.ToString();
            MessageBox.Show(res);
        }

        private void Awesomium_Windows_Forms_WebControl_ShowCreatedWebView(object sender, Awesomium.Core.ShowCreatedWebViewEventArgs e)
        {

        }
    }
}
