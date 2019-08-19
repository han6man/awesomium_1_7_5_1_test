/********************************************************************************
 *    Project   : Awesomium.NET (WPFJavascriptSample)
 *    File      : App.xaml.cs
 *    Version   : 1.7.0.0 
 *    Date      : 08/18/2014
 *    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
 *    Copyright : ©2014 Awesomium Technologies LLC
 *-------------------------------------------------------------------------------
 *
 *    Notes     :
 *
 *    Entry and exit point of the WPFJavascriptSample application.
 *    
 *    
 ********************************************************************************/

using System;
using System.Linq;
using Awesomium.Core;
using System.Windows;
using System.Collections.Generic;

namespace WPFJavascriptSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
            // Initialization must be performed here,
            // before creating a WebControl.
            if ( !WebCore.IsInitialized )
            {
                WebCore.Initialize( new WebConfig()
                {
                    LogLevel = LogLevel.Verbose,
                    RemoteDebuggingPort = 9033
                } );
            }

            base.OnStartup( e );
        }

        protected override void OnExit( ExitEventArgs e )
        {
            // Make sure we shutdown the core last.
            if ( WebCore.IsInitialized )
                WebCore.Shutdown();

            base.OnExit( e );
        }
    }
}
