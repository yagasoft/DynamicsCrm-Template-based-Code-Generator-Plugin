using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Threading;
using Yagasoft.CrmCodeGenerator.Models.Messages;
using Yagasoft.Libraries.Common;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public static class Status
    {
		public static void PopException(Dispatcher dispatcher, string message)
		{
			MessageBox.Show(message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void PopException(Dispatcher dispatcher, Exception exception)
		{
			var message = exception.Message
				+ (exception.InnerException != null ? "\n" + exception.InnerException.Message : "");
			MessageBox.Show(message, exception.GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
		}
    }
}
