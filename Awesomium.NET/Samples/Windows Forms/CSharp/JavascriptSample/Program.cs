/********************************************************************************
 *    Project   : Awesomium.NET (JavascriptSample)
 *    File      : Program.cs
 *    Version   : 1.7.0.0 
 *    Date      : 3/5/2013
 *    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
 *    Copyright : ©2013 Awesomium Technologies LLC
 *    
 *    This code is provided "AS IS" and for demonstration purposes only,
 *    without warranty of any kind.
 *    
 *-------------------------------------------------------------------------------
 *
 *    Notes     :
 *
 *    Entry and exit point of the JavascripSample application.
 *    
 *    
 ********************************************************************************/

using System;
using System.Linq;
using Awesomium.Core;
using System.Windows.Forms;
using System.Collections.Generic;

namespace JavascriptSample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Checks if this is a child rendering process and if so,
            // transfers control of the process to Awesomium.
            if ( WebCore.IsChildProcess )
            {
                WebCore.ChildProcessMain();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.ApplicationExit += OnApplicationExit;
            Application.Run( new WebForm() );
        }

        private static void OnApplicationExit( object sender, EventArgs e )
        {
            // Make sure we shutdown the core last.
            if ( WebCore.IsInitialized )
                WebCore.Shutdown();
        }
    }
}
