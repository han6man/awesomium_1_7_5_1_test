﻿'*******************************************************************************
'    Project   : Awesomium.NET (VBWPFSample)
'    File      : GlowTextBlock.vb
'    Version   : 1.7.0.0 
'    Date      : 3/5/2013
'    Author    : Perikles C. Stephanidis (perikles@awesomium.com)
'    Copyright : ©2013 Awesomium Technologies LLC
'    
'    This code is provided "AS IS" and for demonstration purposes only,
'    without warranty of any kind.
'     
'-------------------------------------------------------------------------------
'
'    Notes     :
'
'    A custom TextBlock element that presents glowing text.
'    
'    
'*******************************************************************************

Imports System.ComponentModel

Friend Class GlowTextBlock
    Inherits Control

    Shared Sub New()
        'This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
        'This style is defined in themes\generic.xaml
        DefaultStyleKeyProperty.OverrideMetadata(GetType(GlowTextBlock), New FrameworkPropertyMetadata(GetType(GlowTextBlock)))
    End Sub

    <Category("Common")> _
    Public Property Text As String
        Get
            Return TryCast(Me.GetValue(GlowTextBlock.TextProperty), String)
        End Get
        Set(ByVal value As String)
            Me.SetValue(GlowTextBlock.TextProperty, value)
        End Set
    End Property

    Public Shared ReadOnly TextProperty As DependencyProperty = _
                           DependencyProperty.Register("Text", _
                           GetType(String), GetType(GlowTextBlock), _
                           New FrameworkPropertyMetadata(Nothing))

    Public Property TextTrimming As TextTrimming
        Get
            Return CType(Me.GetValue(GlowTextBlock.TextTrimmingProperty), TextTrimming)
        End Get
        Set(ByVal value As TextTrimming)
            Me.SetValue(GlowTextBlock.TextTrimmingProperty, value)
        End Set
    End Property

    Public Shared ReadOnly TextTrimmingProperty As DependencyProperty = _
                           DependencyProperty.Register("TextTrimming", _
                           GetType(TextTrimming), GetType(GlowTextBlock), _
                           New FrameworkPropertyMetadata(TextTrimming.None))

    <Browsable(False)> _
    Friend Property GlowVisibility As Visibility
        Get
            Return CType(Me.GetValue(GlowTextBlock.GlowVisibilityProperty), Visibility)
        End Get
        Set(ByVal value As Visibility)
            Me.SetValue(GlowTextBlock.GlowVisibilityProperty, value)
        End Set
    End Property

    Public Shared ReadOnly GlowVisibilityProperty As DependencyProperty = _
                           DependencyProperty.Register("GlowVisibility", _
                           GetType(Visibility), GetType(GlowTextBlock), _
                           New FrameworkPropertyMetadata(Visibility.Hidden))
End Class
