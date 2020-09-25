using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CrmCodeGenerator.VSPackage.Dialogs
{
	public class Extensions
	{
		public static readonly DependencyProperty BangProperty;

		public static bool GetBang(DependencyObject obj)
		{
			return (bool) obj.GetValue(BangProperty);
		}

		public static void SetBang(DependencyObject obj, bool value)
		{
			obj.SetValue(BangProperty, value);
		}

		static Extensions()
		{
			//register attached dependency property
			var metadata = new FrameworkPropertyMetadata(false);
			BangProperty = DependencyProperty.RegisterAttached("Bang",
				typeof(bool),
				typeof(Extensions), metadata);
		}
	}
}
