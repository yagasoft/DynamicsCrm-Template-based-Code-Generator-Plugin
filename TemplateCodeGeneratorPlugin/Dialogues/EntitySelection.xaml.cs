#region Imports

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CrmCodeGenerator.VSPackage.Helpers;
using CrmCodeGenerator.VSPackage.ViewModels;
using Microsoft.Xrm.Sdk.Metadata;
using Yagasoft.CrmCodeGenerator;
using Yagasoft.CrmCodeGenerator.Connection;
using Yagasoft.CrmCodeGenerator.Connection.OrgSvcs;
using Yagasoft.CrmCodeGenerator.Helpers;
using Yagasoft.CrmCodeGenerator.Models.Cache;
using Yagasoft.CrmCodeGenerator.Models.Settings;
using Yagasoft.Libraries.Common;
using Yagasoft.TemplateCodeGeneratorPlugin.Control;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using InnerMetadataHelpers = Yagasoft.CrmCodeGenerator.Helpers.MetadataHelpers;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace CrmCodeGenerator.VSPackage.Dialogs
{
	/// <summary>
	///     Interaction logic for Filter.xaml
	/// </summary>
	public partial class EntitySelection : INotifyPropertyChanged
	{
		private readonly IConnectionManager<IDisposableOrgSvc> connectionManager;
		private readonly MetadataCache metadataCache;
		private readonly EntitySelectorForms entitySelectorForms;
		private readonly WorkerHelper workerHelper;

		#region Properties

		public Settings Settings { get; set; }

		public List<EntityMetadata> EntityMetadataCache;

		private bool displayFilter;

		public bool DisplayFilter
		{
			get => displayFilter;
			set
			{
				displayFilter = value;
				OnPropertyChanged();
			}
		}

		private bool entitiesSelectAll;

		public bool EntitiesSelectAll
		{
			get => entitiesSelectAll;
			set
			{
				entitiesSelectAll = value;
				Entities.ToList().ForEach(entity => entity.IsSelected = value);
				OnPropertyChanged();
			}
		}


		private bool isMetadataSelectAll;

		public bool IsMetadataSelectAll
		{
			get => isMetadataSelectAll;
			set
			{
				isMetadataSelectAll = value;
				Entities.ToList().ForEach(entity => entity.IsGenerateMeta = value);
				OnPropertyChanged();
			}
		}

		private bool isJsEarlySelectAll;

		public bool IsJsEarlySelectAll
		{
			get => isJsEarlySelectAll;
			set
			{
				isJsEarlySelectAll = value;
				Entities.ToList().ForEach(entity => entity.IsJsEarly = value);
				OnPropertyChanged();
			}
		}

		private bool isOptionsetLabelsSelectAll;

		public bool IsOptionsetLabelsSelectAll
		{
			get => isOptionsetLabelsSelectAll;
			set
			{
				isOptionsetLabelsSelectAll = value;
				Entities.ToList().ForEach(entity => entity.IsOptionsetLabels = value);
				OnPropertyChanged();
			}
		}

		private bool isLookupLabelsSelectAll;

		public bool IsLookupLabelsSelectAll
		{
			get => isLookupLabelsSelectAll;
			set
			{
				isLookupLabelsSelectAll = value;
				Entities.ToList().ForEach(entity => entity.IsLookupLabels = value);
				OnPropertyChanged();
			}
		}

		public string LogicalName { get; set; }

		public ObservableCollection<EntitySelectionGridRow> Entities { get; set; }

		#endregion

		#region Property events

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		private readonly ConcurrentBag<EntitySelectionGridRow> rowListSource = new ConcurrentBag<EntitySelectionGridRow>();

		#region Init

		public EntitySelection(Settings settings, IConnectionManager<IDisposableOrgSvc> connectionManager, MetadataCache metadataCache,
			EntitySelectorForms entitySelectorForms, WorkerHelper workerHelper)
		{
			InitializeComponent();

			this.connectionManager = connectionManager;
			this.metadataCache = metadataCache;
			this.entitySelectorForms = entitySelectorForms;
			this.workerHelper = workerHelper;

			Entities = new ObservableCollection<EntitySelectionGridRow>();

			Settings = settings;
		}

		public void ResetList()
		{
			workerHelper.WorkerAction("Fetching entity metadata ...",
				progressReporter =>
				{
					try
					{
						if (metadataCache.ProfileEntityMetadataCache.Any())
						{
							EntityMetadataCache = metadataCache.ProfileEntityMetadataCache;
						}
						else
						{
							RefreshEntityMetadata();
						}

						if (EntityMetadataCache == null)
						{
							return;
						}

						progressReporter(90, "Initialising ...");
						InitEntityList();

						Dispatcher.Invoke(
							() =>
							{
								DataContext = this;
								CheckBoxEntitiesSelectAll.DataContext = this;
								CheckBoxMetadataSelectAll.DataContext = this;
								CheckBoxJsEarlySelectAll.DataContext = this;
								CheckBoxOptionsetLabelsSelectAll.DataContext = this;
								CheckBoxLookupLabelsSelectAll.DataContext = this;
							});
					}
					catch (Exception ex)
					{
						Status.PopException(Dispatcher, ex);
					}
				}, null);
		}

		private void InitEntityList(List<string> filter = null)
		{
			Dispatcher.Invoke(Entities.Clear);

			var rowList = new ConcurrentBag<EntitySelectionGridRow>();

			var filteredEntities = EntityMetadataCache
				.Where(entity => filter == null || filter.Contains(entity.LogicalName)).ToArray();

			Parallel.ForEach(filteredEntities,
				entity =>
				{
					var entityAsync = entity;

					var profiles = Settings.CrmEntityProfiles
						.Where(e => e.LogicalName == entityAsync.LogicalName).ToArray();

					var  profile = profiles.FirstOrDefault();

					// clean redundant profiles
					if (profiles.Length > 1)
					{
						foreach (var profileQ in profiles.Skip(1))
						{
							Settings.CrmEntityProfiles.Remove(profileQ);
						}
					}

					var row = rowListSource.FirstOrDefault(r => r.Name == entityAsync.LogicalName)
						?? new EntitySelectionGridRow
						   {
							   Entity = entityAsync,
							   EntityProfile = profile?.Copy(),
							   IsSelected = Settings.EntitiesSelected.Contains(entity.LogicalName),
							   Name = entityAsync.LogicalName,
							   Rename = profile?.EntityRename,
							   Annotations = profile?.EntityAnnotations,
							   DisplayName =
								   entityAsync.DisplayName?.UserLocalizedLabel == null || !Settings.UseDisplayNames
									   ? Naming.GetProperHybridName(entityAsync.SchemaName, entityAsync.LogicalName)
									   : Naming.Clean(entityAsync.DisplayName.UserLocalizedLabel.Label),
							   IsGenerateMeta = Settings.PluginMetadataEntitiesSelected.Contains(entityAsync.LogicalName),
							   IsJsEarly = Settings.JsEarlyBoundEntitiesSelected.Contains(entityAsync.LogicalName),
							   IsEntityFiltered = Settings.EarlyBoundFilteredSelected.Contains(entityAsync.LogicalName),
							   IsLinkToContracts = Settings.EarlyBoundLinkedSelected.Contains(entityAsync.LogicalName),
							   IsOptionsetLabels = Settings.OptionsetLabelsEntitiesSelected.Contains(entityAsync.LogicalName),
							   IsLookupLabels = Settings.LookupLabelsEntitiesSelected.Contains(entityAsync.LogicalName),
							   SelectedActions = Settings.SelectedActions?.FirstNotNullOrDefault(entityAsync.LogicalName)
						   };

					rowList.Add(row);

					if (rowListSource.All(r => r.Name != row.Name))
					{
						rowListSource.Add(row);
					}
				});

			foreach (var row in rowList.OrderByDescending(row => row.IsSelected).ThenBy(row => row.Name))
			{
				Dispatcher.Invoke(() => Entities.Add(row));
			}

			// selectAll status based on selection count
			if (filteredEntities.All(entity => rowList.Where(row => row.IsSelected).Select(row => row.Name)
				.Contains(entity.LogicalName)))
			{
				Dispatcher.Invoke(() => EntitiesSelectAll = true);
			}

			if (filteredEntities.All(entity => rowList.Where(row => row.IsGenerateMeta).Select(row => row.Name)
				.Contains(entity.LogicalName)))
			{
				Dispatcher.Invoke(() => IsMetadataSelectAll = true);
			}

			if (filteredEntities.All(entity => rowList.Where(row => row.IsJsEarly).Select(row => row.Name)
				.Contains(entity.LogicalName)))
			{
				Dispatcher.Invoke(() => IsJsEarlySelectAll = true);
			}

			if (filteredEntities.All(entity => rowList.Where(row => row.IsOptionsetLabels).Select(row => row.Name)
				.Contains(entity.LogicalName)))
			{
				Dispatcher.Invoke(() => IsOptionsetLabelsSelectAll = true);
			}

			if (filteredEntities.All(entity => rowList.Where(row => row.IsLookupLabels).Select(row => row.Name)
				.Contains(entity.LogicalName)))
			{
				Dispatcher.Invoke(() => IsLookupLabelsSelectAll = true);
			}
		}

		#endregion

		public void SaveFilter()
		{
			foreach (var row in rowListSource)
			{
				var entity = row.Entity;

				if (row.IsSelected && !Settings.EntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.EntitiesSelected.Add(entity.LogicalName);
				}
				else if (!row.IsSelected && Settings.EntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.EntitiesSelected.Remove(entity.LogicalName);
				}

				if (row.IsEntityFiltered && !Settings.EarlyBoundFilteredSelected.Contains(entity.LogicalName))
				{
					Settings.EarlyBoundFilteredSelected.Add(entity.LogicalName);
				}
				else if (!row.IsEntityFiltered && Settings.EarlyBoundFilteredSelected.Contains(entity.LogicalName))
				{
					Settings.EarlyBoundFilteredSelected.Remove(entity.LogicalName);
				}

				if (row.IsLinkToContracts && !Settings.EarlyBoundLinkedSelected.Contains(entity.LogicalName))
				{
					Settings.EarlyBoundLinkedSelected.Add(entity.LogicalName);
				}
				else if (!row.IsLinkToContracts && Settings.EarlyBoundLinkedSelected.Contains(entity.LogicalName))
				{
					Settings.EarlyBoundLinkedSelected.Remove(entity.LogicalName);
				}

				var profile = Settings.CrmEntityProfiles
					.FirstOrDefault(e => e.LogicalName == entity.LogicalName);

				if (profile != null)
				{
					Settings.CrmEntityProfiles.Remove(profile);
				}

				profile = row.EntityProfile;

				if (profile?.IsBasicDataFilled != true && !row.IsEntityFiltered)
				{
					profile = row.EntityProfile = null;
				}

				if ((row.Rename.IsFilled() || row.Annotations.IsFilled() || row.IsEntityFiltered) && profile == null)
				{
					profile = row.EntityProfile = new EntityProfile(entity.LogicalName);
				}

				// copy attributes from contracts to CRM entity
				if (row.IsLinkToContracts)
				{
					profile = row.EntityProfile = profile ?? new EntityProfile(entity.LogicalName);

					var contracts = Settings.EntityProfilesHeaderSelector.EntityProfilesHeaders.SelectMany(p => p.EntityProfiles)
						.Where(p => p.LogicalName == entity.LogicalName).ToArray();
					profile.Attributes = contracts.SelectMany(p => p.Attributes).Union(profile.Attributes ?? new string[0]).Distinct().OrderBy(a => a).ToArray();
					profile.OneToN = contracts.SelectMany(p => p.OneToN).Union(profile.OneToN ?? new string[0]).Distinct().OrderBy(a => a).ToArray();
					profile.NToOne = contracts.SelectMany(p => p.NToOne).Union(profile.NToOne ?? new string[0]).Distinct().OrderBy(a => a).ToArray();
					profile.NToN = contracts.SelectMany(p => p.NToN).Union(profile.NToN ?? new string[0]).Distinct().OrderBy(a => a).ToArray();
				}

				if (profile == null)
				{
					Settings.EarlyBoundFilteredSelected.Remove(entity.LogicalName);
					Settings.EarlyBoundLinkedSelected.Remove(entity.LogicalName);
				}
				else
				{
					profile.EntityRename = row.Rename;
					profile.EntityAnnotations = row.Annotations;
					Settings.CrmEntityProfiles.Add(profile);
				}

				if (row.IsGenerateMeta && !Settings.PluginMetadataEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.PluginMetadataEntitiesSelected.Add(entity.LogicalName);
				}
				else if (!row.IsGenerateMeta && Settings.PluginMetadataEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.PluginMetadataEntitiesSelected.Remove(entity.LogicalName);
				}

				if (row.IsJsEarly && !Settings.JsEarlyBoundEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.JsEarlyBoundEntitiesSelected.Add(entity.LogicalName);
				}
				else if (!row.IsJsEarly && Settings.JsEarlyBoundEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.JsEarlyBoundEntitiesSelected.Remove(entity.LogicalName);
				}

				if (row.IsOptionsetLabels && !Settings.OptionsetLabelsEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.OptionsetLabelsEntitiesSelected.Add(entity.LogicalName);
				}
				else if (!row.IsOptionsetLabels && Settings.OptionsetLabelsEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.OptionsetLabelsEntitiesSelected.Remove(entity.LogicalName);
				}

				if (row.IsLookupLabels && !Settings.LookupLabelsEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.LookupLabelsEntitiesSelected.Add(entity.LogicalName);
				}
				else if (!row.IsLookupLabels && Settings.LookupLabelsEntitiesSelected.Contains(entity.LogicalName))
				{
					Settings.LookupLabelsEntitiesSelected.Remove(entity.LogicalName);
				}

				if (row.SelectedActions != null && row.ActionNames != null)
				{
					Settings.SelectedActions[entity.LogicalName] = row.SelectedActions.Intersect(row.ActionNames).ToArray();
				}
				
				if (row.SelectedActions?.Any() == false && Settings.SelectedActions.ContainsKey(entity.LogicalName))
				{
					Settings.SelectedActions.Remove(entity.LogicalName);
				}
			}
		}

		#region CRM

		private void RefreshEntityMetadata()
		{
			try
			{
				InnerMetadataHelpers.RefreshSettingsEntityMetadata(Settings, connectionManager, metadataCache);
				EntityMetadataCache = metadataCache.ProfileEntityMetadataCache;
			}
			catch (NullReferenceException)
			{
				// ignored
			}
		}

		#endregion

		#region UI events

		#region Grid stuff

		private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var cell = sender as DataGridCell;

			if (cell != null && !cell.IsEditing)
			{
				// enables editing on single click
				if (!cell.IsFocused)
				{
					cell.Focus();
				}
			}
		}

		/// <summary>
		///     Credit: http://stackoverflow.com/a/3833742/1919456
		/// </summary>
		private void DataGridRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var row = sender as DataGridRow;

			if (row == null)
			{
				return;
			}

			var grid = row.GetParent<DataGrid>();
			var gridName = grid.Name.Replace("Grid", "");

			// skip if an editable cell is clicked
			if (!e.IsCellClicked<Button>() && e.IsTextCellClicked())
			{
				// unselect all rows
				for (var i = 0; i < grid.Items.Count; i++)
				{
					var container = grid.ItemContainerGenerator.ContainerFromIndex(i);

					if (container == null)
					{
						continue;
					}

					var rowQ = (DataGridRow) container;

					if (rowQ.IsSelected)
					{
						rowQ.IsSelected = false;
					}
				}

				// select current row
				if (!row.IsSelected)
				{
					row.IsSelected = true;
				}

				return;
			}

			var d = (DependencyObject)e.OriginalSource;

			if (d != null && (d.IsCheckboxClickedParentCheck("IsGenerateMeta")
				|| d.IsCheckboxClickedParentCheck("IsGenerateMeta")))
			{
				// clicked on meta
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				rowDataCast.IsGenerateMeta = !rowDataCast.IsGenerateMeta;

				// selectAll value to false
				var field = GetType().GetField("IsMetadataSelectAll",
					BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);

				field?.SetValue(this, false);

				OnPropertyChanged("IsMetadataSelectAll");
			}
			else if (d != null && (d.IsCheckboxClickedParentCheck("IsJsEarly")
				|| d.IsCheckboxClickedParentCheck("IsJsEarly")))
			{
				// clicked on meta
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				rowDataCast.IsJsEarly = !rowDataCast.IsJsEarly;

				// selectAll value to false
				var field = GetType().GetField("IsJsEarlySelectAll",
					BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);

				field?.SetValue(this, false);

				OnPropertyChanged("IsJsEarlySelectAll");
			}
			else if (d != null && (d.IsCheckboxClickedParentCheck("IsEntityFiltered")
				|| d.IsCheckboxClickedParentCheck("IsEntityFiltered")))
			{
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				rowDataCast.IsEntityFiltered = !rowDataCast.IsEntityFiltered;
			}
			else if (d != null && (d.IsCheckboxClickedParentCheck("IsLinkToContracts")
				|| d.IsCheckboxClickedParentCheck("IsLinkToContracts")))
			{
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				rowDataCast.IsLinkToContracts = !rowDataCast.IsLinkToContracts;
			}
			else if (d != null && (d.IsCheckboxClickedParentCheck("IsOptionsetLabels")
				|| d.IsCheckboxClickedParentCheck("IsOptionsetLabels")))
			{
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				rowDataCast.IsOptionsetLabels = !rowDataCast.IsOptionsetLabels;

				// selectAll value to false
				var field = GetType().GetField("IsOptionsetLabelsSelectAll",
					BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);

				field?.SetValue(this, false);

				OnPropertyChanged("IsOptionsetLabelsSelectAll");
			}
			else if (d != null && (d.IsCheckboxClickedParentCheck("IsLookupLabels")
				|| d.IsCheckboxClickedParentCheck("IsLookupLabels")))
			{
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				rowDataCast.IsLookupLabels = !rowDataCast.IsLookupLabels;

				// selectAll value to false
				var field = GetType().GetField("IsLookupLabelsSelectAll",
					BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);

				field?.SetValue(this, false);

				OnPropertyChanged("IsLookupLabelsSelectAll");
			}
			else if (d != null && (d.IsCheckboxClickedParentCheck("LoadActions")
				|| d.IsCheckboxClickedParentCheck("LoadActions")))
			{
				var rowDataCast = (EntitySelectionGridRow)row.Item;
				var location = PointToScreen(Mouse.GetPosition(this));
				LoadActions(rowDataCast,
					() => new PopupSelector(rowDataCast.ActionNames, rowDataCast.SelectedActions,
						selectedActions => Dispatcher.Invoke(() => rowDataCast.SelectedActions = selectedActions),
						location.X, location.Y).ShowDialog());
			}
			else
			{
				// clicked select
				var rowData = (GridRow)row.Item;
				rowData.IsSelected = !rowData.IsSelected;

				// selectAll value to false
				var selectAllField = GetType().GetField(gridName + "SelectAll",
					BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);

				selectAllField?.SetValue(this, false);

				OnPropertyChanged(gridName + "SelectAll");
			}
		}

		private void DataGridRow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var row = sender as DataGridRow;

			if (row == null)
			{
				return;
			}

			var grid = row.GetParent<DataGrid>();
			var gridName = grid.Name.Replace("Grid", "");

			if (gridName.Contains("Entities"))
			{
				// enables editing on single click
				if (!row.IsFocused)
				{
					row.Focus();
				}

				var rowData = (EntitySelectionGridRow)row.Item;

				if (e.IsTextCellClicked())
				{
					// unselect all rows
					for (var i = 0; i < grid.Items.Count; i++)
					{
						var container = grid.ItemContainerGenerator.ContainerFromIndex(i);

						if (container == null)
						{
							continue;
						}

						var rowQ = (DataGridRow)container;

						if (rowQ.IsSelected)
						{
							rowQ.IsSelected = false;
						}

						if (Extensions.GetBang(rowQ))
						{
							Extensions.SetBang(rowQ, false);
						}
					}

					if (row.IsSelected)
					{
						return;
					}

					// select current row
					Extensions.SetBang(row, true);
					row.IsSelected = true;

					// get logical name and re-init
					LogicalName = rowData.Name;

					var entityProfile = rowData.EntityProfile ?? new EntityProfile(LogicalName);

					entitySelectorForms.ShowView(
						new FilterDetails(LogicalName, Settings, entityProfile,
							new ObservableCollection<GridRow>(Entities), connectionManager, metadataCache, workerHelper,
							() => Dispatcher.Invoke(() => rowData.EntityProfile = entityProfile.IsBasicDataFilled ? entityProfile : null),
							() => entitySelectorForms.ResetView()));
				}
			}
		}

		private void Grid_KeyUp(object sender, KeyEventArgs e)
		{
			ProcessGridKeyUp(sender, e);
		}

		private static void ProcessGridKeyUp(object sender, KeyEventArgs e)
		{
			var grid = sender as DataGrid;

			if (grid == null)
			{
				return;
			}

			for (var i = 0; i < grid.Items.Count; i++)
			{
				var item = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(i);

				if (item != null && item.IsEditing)
				{
					return;
				}
			}

			switch (e.Key)
			{
				case Key.Space:
					var isFirstItemSelected = ((GridRow)grid.SelectedItem).IsSelected;

					foreach (var item in grid.SelectedItems.Cast<GridRow>()
						.Where(item => item.IsSelected == isFirstItemSelected))
					{
						item.IsSelected = !isFirstItemSelected;
					}

					break;

				case Key.Delete:
					foreach (var item in grid.SelectedItems.Cast<GridRow>()
						.Where(item => !string.IsNullOrEmpty(item.Rename)))
					{
						item.Rename = "";
					}

					foreach (var item in grid.SelectedItems.Cast<GridRow>()
						.Where(item => !string.IsNullOrEmpty(item.Annotations)))
					{
						item.Annotations = "";
					}

					break;
			}
		}

		private void CheckBoxIsSelected_OnClick(object sender, RoutedEventArgs e)
		{
			// ignore check-box clicks
			var checkBox = sender as CheckBox;

			if (checkBox != null)
			{
				checkBox.IsChecked = !checkBox.IsChecked;
			}
		}

		#endregion

		private void LoadActions(EntitySelectionGridRow row, Action action)
		{
			workerHelper.WorkerAction($"Loading {row.Name} Actions ...",
				progressReporter =>
				{
					try
					{
						var actions = InnerMetadataHelpers.RetrieveActionNames(Settings,
							connectionManager, metadataCache, row.Name).ToArray();
						Dispatcher.Invoke(
							() =>
							{
								row.ActionNames = new ObservableCollection<string>(actions);
								row.SelectedActions = new ObservableCollection<string>(
									row.SelectedActions?.Intersect(actions) ?? Array.Empty<string>());
							});
						Dispatcher.InvokeAsync(action);
					}
					catch (Exception ex)
					{
						Status.PopException(Dispatcher, ex);
					}
				}, null);
		}

		#region Top bar stuff

		private void ButtonFilter_Click(object sender, RoutedEventArgs e)
		{
			SelectEntitiesByRegex();
		}

		private void ButtonFilterClear_Click(object sender, RoutedEventArgs e)
		{
			TextBoxFilter.Text = string.Empty;
			SelectEntitiesByRegex();
		}

		private void TextBoxFilter_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				SelectEntitiesByRegex();
			}
		}

		private void SelectEntitiesByRegex()
		{
			IEnumerable<string> customEntities = null;

			if (!string.IsNullOrEmpty(TextBoxFilter.Text))
			{

				// get all regex
				var prefixes = TextBoxFilter.Text.ToLower()
					.Split(',').Select(prefix => prefix.Trim())
					.Where(prefix => !string.IsNullOrEmpty(prefix))
					.Distinct();

				// get entity names that match any regex from the fetched list
				if (DisplayFilter)
				{
					customEntities =
						EntityMetadataCache
							.ToDictionary(key => key.LogicalName,
								value =>
								{
									var rename = Settings.CrmEntityProfiles
										.FirstOrDefault(filter => filter.LogicalName == value.LogicalName)?.EntityRename;

									return "("
										+ (string.IsNullOrEmpty(rename)
											? value.DisplayName?.UserLocalizedLabel == null || !Settings.UseDisplayNames
												? Naming.GetProperHybridName(value.SchemaName, value.LogicalName)
												: Naming.Clean(value.DisplayName.UserLocalizedLabel.Label)
											: rename)
										+ ")";
								})
							.Where(keyValue => prefixes.Any(
								prefix => Regex.IsMatch(keyValue.Value.ToLower().Replace("(", "").Replace(")", ""), prefix)))
							.Select(keyValue => keyValue.Key)
							.Distinct();
				}
				else
				{
					customEntities = Settings.EntityList
						.Where(entity => prefixes.Any(prefix => Regex.IsMatch(entity, prefix)))
						.Distinct();
				}
			}

			// filter entities
			workerHelper.WorkerAction("Filtering ...",
				progressReporter =>
				{
					try
					{
						InitEntityList(customEntities?.ToList());
						Dispatcher.Invoke(() => TextBoxFilter.Focus());
					}
					catch (Exception ex)
					{
						Status.PopException(Dispatcher, ex);
					}
				}, null);
		}

		#endregion

		#endregion
	}
}
