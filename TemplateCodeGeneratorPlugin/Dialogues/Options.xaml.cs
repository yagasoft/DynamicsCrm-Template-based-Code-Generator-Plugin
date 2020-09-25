#region Imports

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using CrmCodeGenerator.VSPackage.Helpers;
using Yagasoft.CrmCodeGenerator;
using Yagasoft.CrmCodeGenerator.Connection;
using Yagasoft.CrmCodeGenerator.Connection.OrgSvcs;
using Yagasoft.CrmCodeGenerator.Helpers;
using Yagasoft.CrmCodeGenerator.Models.Cache;
using Yagasoft.CrmCodeGenerator.Models.Settings;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;
using Application = System.Windows.Forms.Application;
using Yagasoft.Libraries.Common;
using MetadataHelpers = Yagasoft.CrmCodeGenerator.Helpers.MetadataHelpers;

#endregion

namespace CrmCodeGenerator.VSPackage.Dialogs
{
	/// <summary>
	///     Interaction logic for Filter.xaml
	/// </summary>
	public partial class Options : INotifyPropertyChanged
	{
		private readonly IConnectionManager<IDisposableOrgSvc> connectionManager;
		private readonly MetadataCache metadataCache;
		private readonly WorkerHelper workerHelper;

		#region Properties

		public Settings Settings { get; set; }

		public ObservableCollection<string> GlobalActionNames
		{
			get => globalActionNames;
			set
			{
				globalActionNames = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<string> SelectedGlobalActions
		{
			get => selectedGlobalActions;
			set
			{
				selectedGlobalActions = value;
				OnPropertyChanged();
			}
		}

		public bool IsGlobalActionsVisible
		{
			get => isGlobalActionsVisible;
			set
			{
				isGlobalActionsVisible = value;
				OnPropertyChanged();
				OnPropertyChanged("IsGlobalActionsNotVisible");
			}
		}

		public bool IsGlobalActionsNotVisible => !IsGlobalActionsVisible;

		#endregion

		private ObservableCollection<string> globalActionNames = new ObservableCollection<string>();
		private ObservableCollection<string> selectedGlobalActions = new ObservableCollection<string>();
		private bool isGlobalActionsVisible;

		#region Property events

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Init

		public Options(Settings settings, IConnectionManager<IDisposableOrgSvc> connectionManager, MetadataCache metadataCache,
			WorkerHelper workerHelper)
		{
			InitializeComponent();

			this.connectionManager = connectionManager;
			this.metadataCache = metadataCache;
			this.workerHelper = workerHelper;

			Settings = settings;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			DataContext = Settings;

			GlobalActionsSection.DataContext = this;
			CheckBoxGenerateGlobalActions.DataContext = Settings;

			TextBoxThreads.Text = Settings.Threads.ToString();
			TextBoxEntitiesPerThread.Text = Settings.EntitiesPerThread.ToString();

			ComboBoxClearMode.ItemsSource = Enum.GetValues(typeof(ClearModeEnum)).Cast<ClearModeEnum>();
		}

		#endregion

		#region UI events

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			var regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void LoadGlobalActions_Click(object sender, RoutedEventArgs e)
		{
			workerHelper.WorkerAction("Loading Global Actions ...",
				progressReporter =>
				{
					try
					{
						var actions = MetadataHelpers.RetrieveActionNames(Settings, connectionManager, metadataCache).ToArray();
						Dispatcher.Invoke(
							() =>
							{
								GlobalActionNames = new ObservableCollection<string>(actions);
								SelectedGlobalActions = new ObservableCollection<string>(Settings.SelectedGlobalActions?.Intersect(actions)
									?? Array.Empty<string>());
								IsGlobalActionsVisible = true;
							});
					}
					catch (Exception ex)
					{
						Status.PopException(Dispatcher, ex);
					}
				}, null);
		}

		public void Save()
		{
			if (TextBoxThreads.Text.IsFilled())
			{
				Settings.Threads = int.Parse(TextBoxThreads.Text);
			}

			if (TextBoxEntitiesPerThread.Text.IsFilled())
			{
				Settings.EntitiesPerThread = int.Parse(TextBoxEntitiesPerThread.Text);
			}

			if (IsGlobalActionsVisible)
			{
				Settings.SelectedGlobalActions = SelectedGlobalActions.ToArray();
			}
		}

		#endregion
	}
}
