﻿using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    internal static class WindowExtensions
    {
        // from winuser.h
	    private const int GWL_STYLE = -16,
	                      WS_SYSMENU = 0x80000,
	                      WS_MAXIMIZEBOX = 0x10000,
	                      WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

        internal static void HideMinimizeAndMaximizeButtons(this Window window)
        {
            // http://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }

        internal static void HideMinimizeButton(this Window window)
        {
            // http://stackoverflow.com/questions/339620/how-do-i-remove-minimize-and-maximize-from-a-resizable-window-in-wpf
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
        }

        internal static void HideCloseButton(this Window window)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
			SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_SYSMENU);
        }


        public static void CenterWindow(this Window me, Window main)
        {
            var newLeft = main.Left + (main.Width / 2) - (me.Width / 2);
            var newTop = main.Top + (main.Height / 2) - (me.Height / 2);
            me.Left = newLeft;
            me.Top = newTop;
            if (main.WindowState == WindowState.Maximized)
            {
                var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)me.Left, (int)me.Top));

                var left = screen.WorkingArea.Left;
                var top = screen.WorkingArea.Top;
                var width = screen.WorkingArea.Width;
                var height = screen.WorkingArea.Height;

                newLeft = left + (width / 2) - (me.Width / 2);
                newTop = top + (height / 2) - (me.Height / 2);

                me.Left = newLeft;
                me.Top = newTop;
            }
        }


    }
}
