VBWPFSample is a WPF sample in VB using the new Awesomium WebControl.

This sample introduces the following new features:
	1. Almost no code behind. UI takes advantage of the provided
	   dependency properties of the WebControl.
	2. No direct calls to the control and no events handling.
	   Buttons use standard routed commands that are handled
	   by the control.
	3. Glass frame added that in Vista and newer Windows versions, resembles a
	   simple IE window.
	4. Updating and rendering is not needed here. The new WebControl
	   and the edited WebCore take care of this.