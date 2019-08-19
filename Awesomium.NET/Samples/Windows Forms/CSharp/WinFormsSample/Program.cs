/********************************************************************************
 *    Project   : Awesomium.NET (WinFormsSample)
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
 *    Entry and exit point of the WinFormsSample application.
 *    
 *    
 ********************************************************************************/

using System;
using Awesomium.Core;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WinFormsSample
{

    static class Program
    {
        [DllImport( "kernel32.dll" )]
        internal static extern int AllocConsole();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main( string[] args )
        {
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
