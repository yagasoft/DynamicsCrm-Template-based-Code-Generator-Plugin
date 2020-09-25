using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Forms;
using CrmCodeGenerator.VSPackage.Dialogs;
using Microsoft.Xrm.Sdk;
using Yagasoft.CrmCodeGenerator.Connection;
using Yagasoft.CrmCodeGenerator.Connection.OrgSvcs;
using Yagasoft.CrmCodeGenerator.Models.Cache;
using Yagasoft.CrmCodeGenerator.Models.Settings;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public partial class EntitySelectorForms : UserControl
	{
		public Settings Settings
		{
			get => settings;
			set
			{
				settings = value;
				ResetList();
			}
		}

		private readonly IConnectionManager<IDisposableOrgSvc> connectionManager;
		private readonly MetadataCache metadataCache;
		private readonly WorkerHelper workerHelper;

		private Settings settings;
		private EntitySelection currentEntitySelection;
		private FilterDetails currentFilterDetails;
		private Options currentOptions;

		public EntitySelectorForms(IConnectionManager<IDisposableOrgSvc> connectionManager, MetadataCache metadataCache,
			WorkerHelper workerHelper)
		{
			this.connectionManager = connectionManager;
			this.metadataCache = metadataCache;
			this.workerHelper = workerHelper;
			InitializeComponent();
		}

		public void SaveSelection()
		{
			currentOptions?.Save();
			currentFilterDetails?.SaveFilter();
			currentEntitySelection?.SaveFilter();
			ResetView();
		}

		public void ResetList()
		{
			var action = new Action(() => currentEntitySelection = Settings == null
				? null
				: new EntitySelection(Settings, connectionManager, metadataCache, this, workerHelper));

			if (IsHandleCreated)
			{
				Invoke(action);
			}
			else
			{
				action();
			}
			
			currentEntitySelection?.ResetList();
			ResetView();
		}

		public void ShowView(UIElement element)
		{
			currentOptions = element as Options;
			currentFilterDetails = element as FilterDetails;
			currentEntitySelection = element as EntitySelection ?? currentEntitySelection;

			var action = new Action(() => entitySelectorHost.Child = Settings == null ? null : element);

			if (IsHandleCreated)
			{
				Invoke(action);
			}
			else
			{
				action();
			}
		}

		public void ResetView()
		{
			ShowView(currentEntitySelection);
		}
	}
}
