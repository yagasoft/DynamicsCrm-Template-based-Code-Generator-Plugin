#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CrmCodeGenerator.VSPackage.Helpers;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

#endregion

namespace CrmCodeGenerator.VSPackage.Dialogs
{
	/// <summary>
	///     Interaction logic for Filter.xaml
	/// </summary>
	public partial class PopupSelector : INotifyPropertyChanged
	{
		#region Properties

		public ObservableCollection<string> Data
		{
			get => data;
			set
			{
				data = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<string> SelectedData
		{
			get => selectedData;
			set
			{
				selectedData = value;
				OnPropertyChanged();
			}
		}

		#endregion

		private ObservableCollection<string> data;
		private ObservableCollection<string> selectedData;

		private readonly Action<IEnumerable<string>> callback;

		#region Property events

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Init

		public PopupSelector(IEnumerable<string> data, IEnumerable<string> selectedData,
			Action<IEnumerable<string>> callback, double? x = null, double? y = null)
		{
			InitializeComponent();

			Actions.DataContext = this;
			Data = new ObservableCollection<string>(data);
			SelectedData = new ObservableCollection<string>(selectedData);

			this.callback = callback;

			if (x == null || y == null)
			{
				WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}
			else
			{
				Left = x.Value - 248;
				Top = y.Value - 20;
			}
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			this.HideCloseButton();
			this.HideMinimizeAndMaximizeButtons();
		}

		#endregion

		#region UI events

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			callback(SelectedData);
			Dispatcher.Invoke(Close);
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Dispatcher.Invoke(Close);
		}

		#endregion
	}
}
