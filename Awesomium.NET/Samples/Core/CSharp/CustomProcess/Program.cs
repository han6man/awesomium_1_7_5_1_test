/********************************************************************************
 *    Project   : Awesomium.NET (CustomProcess)
 *    File      : program.cs
 *    Version   : 1.7.4.1
 *    Date      : 03/05/2014
 *    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
 *    Copyright : ©2014 Awesomium Technologies LLC
 *    
 *    This code is provided "AS IS" and for demonstration purposes only,
 *    without warranty of any kind.
 *     
 *-------------------------------------------------------------------------------
 *
 *    Notes     :
 *
 *    Custom managed Awesomium child process.
 *    
 *    
 ********************************************************************************/

using System;
using System.Linq;
using Awesomium.Core;
using System.Runtime.ExceptionServices;

namespace CustomProcess
{
    class Program
    {
        [HandleProcessCorruptedStateExceptions]
        static int Main( string[] args )
        {
#if !DEBUG
            try
            {
#endif
                return WebCore.ChildProcessMain();
#if !DEBUG
            }
            catch
            {
                // Suppress crash handler on Windows.
                return 1;
            }
#endif
        }
    }
}
