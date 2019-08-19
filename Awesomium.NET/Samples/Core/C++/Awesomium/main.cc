/********************************************************************************
 *    Project   : Awesomium.NET (Awesomium)
 *    File      : main.cc
 *    Version   : 1.7.4.1
 *    Date      : 03/04/2014
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
 *    Custom native Awesomium child process.
 *
 *
 ********************************************************************************/
#include <Awesomium/ChildProcess.h>

#if defined(__WIN32__) || defined(_WIN32)
int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE, wchar_t*, int) {
#ifndef _DEBUG
	__try {
#endif
		return Awesomium::ChildProcessMain(hInstance);
#ifndef _DEBUG
	} __except(EXCEPTION_EXECUTE_HANDLER) {
		// Suppress crash handler on Windows.
		ExitProcess(1);
	}
	return 0;
#endif
}
#else
int main(int argc, const char** argv) {
	return Awesomium::ChildProcessMain(argc, (char**)argv);
}
#endif
