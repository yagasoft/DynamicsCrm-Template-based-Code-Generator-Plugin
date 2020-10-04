#region Imports

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CrmCodeGenerator.VSPackage.Dialogs;
using CrmCodeGenerator.VSPackage.Helpers;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;
using Yagasoft.CrmCodeGenerator.Connection;
using Yagasoft.CrmCodeGenerator.Connection.OrgSvcs;
using Yagasoft.CrmCodeGenerator.Mapper;
using Yagasoft.CrmCodeGenerator.Models.Cache;
using Yagasoft.CrmCodeGenerator.Models.Mapper;
using Yagasoft.CrmCodeGenerator.Models.Messages;
using Yagasoft.CrmCodeGenerator.Models.Settings;
using Yagasoft.Libraries.Common;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;
using Yagasoft.TemplateCodeGeneratorPlugin.Model;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings.File;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModels;
using Yagasoft.TemplateCodeGeneratorPlugin.Templates;
using Label = System.Windows.Forms.Label;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public partial class PluginControl : PluginControlBase, IStatusBarMessenger
	{
		//private bool SettingsSaved
		//{
		//	get => settingsSaved;
		//	set
		//	{
		//		settingsSaved = value;
		//		buttonSaveSettings.Enabled = !value;
		//	}
		//}

		private ToolStrip toolBar;
		private ToolStripButton buttonCloseTool;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton buttonFetchData;
		private ToolStripButton buttonGenerate;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripButton buttonLoadSettings;
		private ToolStripButton buttonSaveAsSettings;
		private ToolStripButton buttonSaveSettings;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripButton buttonClearCache;
		private ToolStripSeparator toolStripSeparator5;
		private LinkLabel linkDownVs;

		private Settings settings;

		////private bool settingsSaved;
		private Button buttonCancel;

		private Mapper mapper;
		private readonly MetadataCache metadataCache = new MetadataCache();
		private TableLayoutPanel tableLayoutMainPanel;
		private TableLayoutPanel tableLayoutPanel2;
		private Panel panelNamespace;
		private Label labelNamespace;
		private TextBox textBoxNamespace;
		private Panel panelHost;

		private ToolStripSeparator toolStripSeparator4;
		private ToolStripButton buttonEntitySelector;
		private ToolStripButton buttonTemplateEditor;
		private Label labelSettingsPath;

		private ToolStripButton buttonOptions;
		private Panel panelLinks;
		private LinkLabel linkQuickGuide;
		private Panel panelToast;
		private Label labelToast;
		private ToolStripButton buttonHistory;
		private ToolStripButton buttonDefaultT4;
		private ToolStripLabel labelYagasoft;
		private Label labelSeparator1;

		// load T4 Generator required assemblies
		private RefreshMode dummy1 = RefreshMode.KeepChanges;

		private PluginSettings pluginSettings;

		private EntitySelectorForms entitySelector;

		private TemplateEditor templateEditor;
		private readonly TemplateViewModel templateViewModel;

		private readonly WorkerHelper workerHelper;
		private readonly ConnectionManager connectionManager;
		private readonly FileHelper fileHelper;
		private readonly UiHelper uiHelper;

		#region Base tool implementation

		public PluginControl()
		{
			InitializeComponent();
			LoadPluginSettings();
			ShowReleaseNotes();

			workerHelper = new WorkerHelper(
				(s, work, callback) => InvokeSafe(() => RunAsync(s, work, callback)),
				(s, work, callback) => InvokeSafe(() => RunAsync(s, work, callback)));
			connectionManager = new ConnectionManager(() => Service);
			fileHelper = new FileHelper(pluginSettings,
				(filetype, savedFileGroup) =>
				{
					var templateFile = savedFileGroup.SavedFiles.FirstNotNullOrDefault(SavedFileType.Template) ?? new SavedFile();
					var path = templateEditor.LabelTemplatePath.Text;

					if (path.IsFilled() && !Regex.IsMatch(path, @"<.*?>"))
					{
						templateFile.Folder = Path.GetDirectoryName(path);
						templateFile.File = Path.GetFileName(path);
						savedFileGroup.SavedFiles[SavedFileType.Template] = templateFile;
					}

					var codeFile = savedFileGroup.SavedFiles.FirstNotNullOrDefault(SavedFileType.Code) ?? new SavedFile();
					path = templateEditor.LabelCodePath.Text;

					if (path.IsFilled() && !Regex.IsMatch(path, @"<.*?>"))
					{
						codeFile.Folder = Path.GetDirectoryName(path);
						codeFile.File = Path.GetFileName(path);
						savedFileGroup.SavedFiles[SavedFileType.Code] = templateFile;
					}

					UpdateFilePathsUi();
					uiHelper.ShowToast($"{filetype} saved.");
				});
			templateViewModel = new TemplateViewModel();
			uiHelper = new UiHelper(panelToast, labelToast, InvokeSafe);
		}

		private void InvokeSafe(Action action)
		{
			if (IsHandleCreated)
			{
				Invoke(action);
			}
			else
			{
				action();
			}
		}

		private void LoadPluginSettings()
		{
			try
			{
				SettingsManager.Instance.TryLoad(typeof(TemplateCodeGeneratorPlugin), out pluginSettings);
			}
			catch
			{
				// ignored
			}

			pluginSettings ??= new PluginSettings();

			var nonExistentFiles = pluginSettings.SavedFiles
				.Where(e => !File.Exists(e.Key)).Select(e => e.Key).ToArray();

			// clean
			foreach (var group in nonExistentFiles)
			{
				pluginSettings.SavedFiles.Remove(group);
			}

			if (!pluginSettings.SavedFiles.ContainsKey(pluginSettings.LatestPath ?? ""))
			{
				pluginSettings.LatestPath = pluginSettings.SavedFiles.FirstOrDefault().Value?.SavedFiles
					.FirstNotNullOrDefault(SavedFileType.Settings)?.Path;
			}
		}

		private void ShowReleaseNotes()
		{
			if (pluginSettings.ReleaseNotesShownVersion != Constants.AppVersion)
			{
				MessageBox.Show(Constants.ReleaseNotes, "Release Notes",
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				pluginSettings.ReleaseNotesShownVersion = Constants.AppVersion;
				SettingsManager.Instance.Save(typeof(TemplateCodeGeneratorPlugin), pluginSettings);
			}
		}

		public override void ClosingPlugin(PluginCloseInfo info)
		{
			var memorySettings = JsonConvert.SerializeObject(settings, Formatting.Indented,
				new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
			string savedSettings = null;

			if (File.Exists(pluginSettings.LatestPath))
			{
				savedSettings = File.ReadAllText(pluginSettings.LatestPath);
			}

			if (!info.Cancel && settings != null && memorySettings != savedSettings)
			{
				if (!PromptSave("Settings"))
				{
					info.Cancel = true;
				}
			}

			if (!info.Cancel && !templateViewModel.T4Saved && templateViewModel.CodeEditorT4.Text.IsFilled())
			{
				if (!PromptSave("Template"))
				{
					info.Cancel = true;
				}
			}

			if (!info.Cancel && !templateViewModel.CsSaved && templateViewModel.CodeEditorCs.Text.IsFilled())
			{
				if (!PromptSave("Code"))
				{
					info.Cancel = true;
				}
			}

			base.ClosingPlugin(info);
		}

		private void BtnCloseClick(object sender, EventArgs e)
		{
			CloseTool(); // PluginBaseControl method that notifies the XrmToolBox that the user wants to close the plugin
		}

		private static bool PromptSave(string name)
		{
			var result = MessageBox.Show($"{name} not saved. Are you sure you want to exit before saving?", $"{name} Not Saved",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			return result == DialogResult.Yes;
		}

		//private void btnCancel_Click(object sender, EventArgs e)
		//{
		//	CancelWorker(); // PluginBaseControl method that calls the Background Workers CancelAsync method.

		//	MessageBox.Show("Cancelled");
		//}

		#endregion Base tool implementation

		#region UI Generated

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginControl));
			this.toolBar = new System.Windows.Forms.ToolStrip();
			this.buttonCloseTool = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonEntitySelector = new System.Windows.Forms.ToolStripButton();
			this.buttonFetchData = new System.Windows.Forms.ToolStripButton();
			this.buttonOptions = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonTemplateEditor = new System.Windows.Forms.ToolStripButton();
			this.buttonDefaultT4 = new System.Windows.Forms.ToolStripButton();
			this.buttonGenerate = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonHistory = new System.Windows.Forms.ToolStripButton();
			this.buttonLoadSettings = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveAsSettings = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveSettings = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonClearCache = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.labelYagasoft = new System.Windows.Forms.ToolStripLabel();
			this.linkDownVs = new System.Windows.Forms.LinkLabel();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.tableLayoutMainPanel = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSettingsPath = new System.Windows.Forms.Label();
			this.panelNamespace = new System.Windows.Forms.Panel();
			this.labelNamespace = new System.Windows.Forms.Label();
			this.textBoxNamespace = new System.Windows.Forms.TextBox();
			this.panelLinks = new System.Windows.Forms.Panel();
			this.linkQuickGuide = new System.Windows.Forms.LinkLabel();
			this.panelHost = new System.Windows.Forms.Panel();
			this.panelToast = new System.Windows.Forms.Panel();
			this.labelToast = new System.Windows.Forms.Label();
			this.labelSeparator1 = new System.Windows.Forms.Label();
			this.toolBar.SuspendLayout();
			this.tableLayoutMainPanel.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panelNamespace.SuspendLayout();
			this.panelLinks.SuspendLayout();
			this.panelToast.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolBar
			// 
			this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonCloseTool,
            this.toolStripSeparator4,
            this.buttonEntitySelector,
            this.buttonFetchData,
            this.buttonOptions,
            this.toolStripSeparator1,
            this.buttonTemplateEditor,
            this.buttonDefaultT4,
            this.buttonGenerate,
            this.toolStripSeparator2,
            this.buttonHistory,
            this.buttonLoadSettings,
            this.buttonSaveAsSettings,
            this.buttonSaveSettings,
            this.toolStripSeparator3,
            this.buttonClearCache,
            this.toolStripSeparator5,
            this.labelYagasoft});
			this.toolBar.Location = new System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.Size = new System.Drawing.Size(1000, 25);
			this.toolBar.TabIndex = 0;
			this.toolBar.Text = "toolBar";
			// 
			// buttonCloseTool
			// 
			this.buttonCloseTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonCloseTool.Image")));
			this.buttonCloseTool.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonCloseTool.Name = "buttonCloseTool";
			this.buttonCloseTool.Size = new System.Drawing.Size(56, 22);
			this.buttonCloseTool.Text = "Close";
			this.buttonCloseTool.Click += new System.EventHandler(this.BtnCloseClick);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonEntitySelector
			// 
			this.buttonEntitySelector.Image = ((System.Drawing.Image)(resources.GetObject("buttonEntitySelector.Image")));
			this.buttonEntitySelector.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonEntitySelector.Name = "buttonEntitySelector";
			this.buttonEntitySelector.Size = new System.Drawing.Size(99, 22);
			this.buttonEntitySelector.Text = "Select Entities";
			this.buttonEntitySelector.Click += new System.EventHandler(this.buttonEntitySelector_Click);
			// 
			// buttonFetchData
			// 
			this.buttonFetchData.Enabled = false;
			this.buttonFetchData.Image = ((System.Drawing.Image)(resources.GetObject("buttonFetchData.Image")));
			this.buttonFetchData.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonFetchData.Name = "buttonFetchData";
			this.buttonFetchData.Size = new System.Drawing.Size(83, 22);
			this.buttonFetchData.Text = "Fetch Data";
			this.buttonFetchData.Click += new System.EventHandler(this.buttonFetchData_Click);
			// 
			// buttonOptions
			// 
			this.buttonOptions.Image = ((System.Drawing.Image)(resources.GetObject("buttonOptions.Image")));
			this.buttonOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonOptions.Name = "buttonOptions";
			this.buttonOptions.Size = new System.Drawing.Size(69, 22);
			this.buttonOptions.Text = "Options";
			this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonTemplateEditor
			// 
			this.buttonTemplateEditor.Image = ((System.Drawing.Image)(resources.GetObject("buttonTemplateEditor.Image")));
			this.buttonTemplateEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonTemplateEditor.Name = "buttonTemplateEditor";
			this.buttonTemplateEditor.Size = new System.Drawing.Size(75, 22);
			this.buttonTemplateEditor.Text = "Template";
			this.buttonTemplateEditor.Click += new System.EventHandler(this.buttonTemplateEditor_Click);
			// 
			// buttonDefaultT4
			// 
			this.buttonDefaultT4.Image = ((System.Drawing.Image)(resources.GetObject("buttonDefaultT4.Image")));
			this.buttonDefaultT4.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonDefaultT4.Name = "buttonDefaultT4";
			this.buttonDefaultT4.Size = new System.Drawing.Size(55, 22);
			this.buttonDefaultT4.Text = "Reset";
			this.buttonDefaultT4.Click += new System.EventHandler(this.buttonDefaultT4_Click);
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Enabled = false;
			this.buttonGenerate.Image = ((System.Drawing.Image)(resources.GetObject("buttonGenerate.Image")));
			this.buttonGenerate.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(74, 22);
			this.buttonGenerate.Text = "Generate";
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonHistory
			// 
			this.buttonHistory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonHistory.Image = ((System.Drawing.Image)(resources.GetObject("buttonHistory.Image")));
			this.buttonHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonHistory.Name = "buttonHistory";
			this.buttonHistory.Size = new System.Drawing.Size(23, 22);
			this.buttonHistory.Text = "History";
			this.buttonHistory.Click += new System.EventHandler(this.buttonHistory_Click);
			// 
			// buttonLoadSettings
			// 
			this.buttonLoadSettings.Image = ((System.Drawing.Image)(resources.GetObject("buttonLoadSettings.Image")));
			this.buttonLoadSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonLoadSettings.Name = "buttonLoadSettings";
			this.buttonLoadSettings.Size = new System.Drawing.Size(98, 22);
			this.buttonLoadSettings.Text = "Load Settings";
			this.buttonLoadSettings.Click += new System.EventHandler(this.buttonLoadSettings_Click);
			// 
			// buttonSaveAsSettings
			// 
			this.buttonSaveAsSettings.Image = ((System.Drawing.Image)(resources.GetObject("buttonSaveAsSettings.Image")));
			this.buttonSaveAsSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveAsSettings.Name = "buttonSaveAsSettings";
			this.buttonSaveAsSettings.Size = new System.Drawing.Size(67, 22);
			this.buttonSaveAsSettings.Text = "Save As";
			this.buttonSaveAsSettings.Click += new System.EventHandler(this.buttonSaveAsSettings_Click);
			// 
			// buttonSaveSettings
			// 
			this.buttonSaveSettings.Image = ((System.Drawing.Image)(resources.GetObject("buttonSaveSettings.Image")));
			this.buttonSaveSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveSettings.Name = "buttonSaveSettings";
			this.buttonSaveSettings.Size = new System.Drawing.Size(51, 22);
			this.buttonSaveSettings.Text = "Save";
			this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonClearCache
			// 
			this.buttonClearCache.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearCache.Image")));
			this.buttonClearCache.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClearCache.Name = "buttonClearCache";
			this.buttonClearCache.Size = new System.Drawing.Size(90, 22);
			this.buttonClearCache.Text = "Clear Cache";
			this.buttonClearCache.Click += new System.EventHandler(this.buttonClearCache_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// labelYagasoft
			// 
			this.labelYagasoft.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
			this.labelYagasoft.ForeColor = System.Drawing.Color.DarkViolet;
			this.labelYagasoft.IsLink = true;
			this.labelYagasoft.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.labelYagasoft.LinkColor = System.Drawing.Color.DarkViolet;
			this.labelYagasoft.Name = "labelYagasoft";
			this.labelYagasoft.Size = new System.Drawing.Size(95, 22);
			this.labelYagasoft.Text = "Yagasoft.com";
			this.labelYagasoft.VisitedLinkColor = System.Drawing.Color.DarkBlue;
			this.labelYagasoft.Click += new System.EventHandler(this.labelYagasoft_Click);
			// 
			// linkDownVs
			// 
			this.linkDownVs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkDownVs.AutoSize = true;
			this.linkDownVs.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkDownVs.Location = new System.Drawing.Point(90, 3);
			this.linkDownVs.Name = "linkDownVs";
			this.linkDownVs.Size = new System.Drawing.Size(121, 13);
			this.linkDownVs.TabIndex = 20;
			this.linkDownVs.TabStop = true;
			this.linkDownVs.Text = "Download VS Extension";
			this.linkDownVs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkDownVs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDownVs_LinkClicked);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(948, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(49, 20);
			this.buttonCancel.TabIndex = 21;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// tableLayoutMainPanel
			// 
			this.tableLayoutMainPanel.ColumnCount = 1;
			this.tableLayoutMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutMainPanel.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutMainPanel.Controls.Add(this.panelHost, 0, 1);
			this.tableLayoutMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutMainPanel.Location = new System.Drawing.Point(0, 25);
			this.tableLayoutMainPanel.Name = "tableLayoutMainPanel";
			this.tableLayoutMainPanel.RowCount = 2;
			this.tableLayoutMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutMainPanel.Size = new System.Drawing.Size(1000, 416);
			this.tableLayoutMainPanel.TabIndex = 22;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
			this.tableLayoutPanel2.Controls.Add(this.labelSettingsPath, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.panelNamespace, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.panelLinks, 2, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(994, 26);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// labelSettingsPath
			// 
			this.labelSettingsPath.AutoEllipsis = true;
			this.labelSettingsPath.AutoSize = true;
			this.labelSettingsPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelSettingsPath.Location = new System.Drawing.Point(253, 0);
			this.labelSettingsPath.Name = "labelSettingsPath";
			this.labelSettingsPath.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.labelSettingsPath.Size = new System.Drawing.Size(518, 26);
			this.labelSettingsPath.TabIndex = 1;
			this.labelSettingsPath.Text = "<settings-path>";
			this.labelSettingsPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelNamespace
			// 
			this.panelNamespace.Controls.Add(this.labelNamespace);
			this.panelNamespace.Controls.Add(this.textBoxNamespace);
			this.panelNamespace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelNamespace.Location = new System.Drawing.Point(3, 3);
			this.panelNamespace.Name = "panelNamespace";
			this.panelNamespace.Size = new System.Drawing.Size(244, 20);
			this.panelNamespace.TabIndex = 0;
			// 
			// labelNamespace
			// 
			this.labelNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.labelNamespace.AutoSize = true;
			this.labelNamespace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNamespace.Location = new System.Drawing.Point(3, 3);
			this.labelNamespace.Name = "labelNamespace";
			this.labelNamespace.Size = new System.Drawing.Size(73, 13);
			this.labelNamespace.TabIndex = 18;
			this.labelNamespace.Text = "Namespace";
			// 
			// textBoxNamespace
			// 
			this.textBoxNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxNamespace.Location = new System.Drawing.Point(82, 0);
			this.textBoxNamespace.Name = "textBoxNamespace";
			this.textBoxNamespace.Size = new System.Drawing.Size(162, 20);
			this.textBoxNamespace.TabIndex = 17;
			this.textBoxNamespace.TextChanged += new System.EventHandler(this.textBoxNamespace_TextChanged);
			// 
			// panelLinks
			// 
			this.panelLinks.Controls.Add(this.labelSeparator1);
			this.panelLinks.Controls.Add(this.linkQuickGuide);
			this.panelLinks.Controls.Add(this.linkDownVs);
			this.panelLinks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLinks.Location = new System.Drawing.Point(777, 3);
			this.panelLinks.Name = "panelLinks";
			this.panelLinks.Size = new System.Drawing.Size(214, 20);
			this.panelLinks.TabIndex = 2;
			// 
			// linkQuickGuide
			// 
			this.linkQuickGuide.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkQuickGuide.AutoSize = true;
			this.linkQuickGuide.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkQuickGuide.Location = new System.Drawing.Point(3, 3);
			this.linkQuickGuide.Name = "linkQuickGuide";
			this.linkQuickGuide.Size = new System.Drawing.Size(66, 13);
			this.linkQuickGuide.TabIndex = 21;
			this.linkQuickGuide.TabStop = true;
			this.linkQuickGuide.Text = "Quick Guide";
			this.linkQuickGuide.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkQuickGuide.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkQuickGuide_LinkClicked);
			// 
			// panelHost
			// 
			this.panelHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelHost.Location = new System.Drawing.Point(3, 35);
			this.panelHost.Name = "panelHost";
			this.panelHost.Size = new System.Drawing.Size(994, 378);
			this.panelHost.TabIndex = 1;
			// 
			// panelToast
			// 
			this.panelToast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panelToast.BackColor = System.Drawing.Color.Black;
			this.panelToast.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelToast.Controls.Add(this.labelToast);
			this.panelToast.ForeColor = System.Drawing.Color.Black;
			this.panelToast.Location = new System.Drawing.Point(741, 341);
			this.panelToast.Name = "panelToast";
			this.panelToast.Size = new System.Drawing.Size(250, 65);
			this.panelToast.TabIndex = 0;
			this.panelToast.Visible = false;
			// 
			// labelToast
			// 
			this.labelToast.AutoEllipsis = true;
			this.labelToast.BackColor = System.Drawing.Color.DarkViolet;
			this.labelToast.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelToast.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelToast.ForeColor = System.Drawing.Color.White;
			this.labelToast.Location = new System.Drawing.Point(0, 0);
			this.labelToast.Name = "labelToast";
			this.labelToast.Size = new System.Drawing.Size(248, 63);
			this.labelToast.TabIndex = 0;
			this.labelToast.Text = "<toast>";
			this.labelToast.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelSeparator1
			// 
			this.labelSeparator1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSeparator1.AutoSize = true;
			this.labelSeparator1.Location = new System.Drawing.Point(75, 3);
			this.labelSeparator1.Name = "labelSeparator1";
			this.labelSeparator1.Size = new System.Drawing.Size(9, 13);
			this.labelSeparator1.TabIndex = 0;
			this.labelSeparator1.Text = "|";
			// 
			// PluginControl
			// 
			this.Controls.Add(this.panelToast);
			this.Controls.Add(this.tableLayoutMainPanel);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.toolBar);
			this.Name = "PluginControl";
			this.Size = new System.Drawing.Size(1000, 441);
			this.Load += new System.EventHandler(this.PluginControl_Load);
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.tableLayoutMainPanel.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.panelNamespace.ResumeLayout(false);
			this.panelNamespace.PerformLayout();
			this.panelLinks.ResumeLayout(false);
			this.panelLinks.PerformLayout();
			this.panelToast.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

		#region Event handlers

		private void PluginControl_Load(object sender, EventArgs e)
		{
			buttonCancel.Hide();

			templateEditor = new TemplateEditor(templateViewModel, fileHelper, ParentForm, workerHelper);
			entitySelector = new EntitySelectorForms(connectionManager, metadataCache, workerHelper);

			LoadSavedSettings();
			ShowTemplateEditor();
		}

		private void buttonEntitySelector_Click(object sender, EventArgs e)
		{
			ShowEntitySelector();
		}

		private void buttonTemplateEditor_Click(object sender, EventArgs e)
		{
			ShowTemplateEditor();
		}

		private void buttonOptions_Click(object sender, EventArgs e)
		{
			ShowOptions();
		}

		private void buttonFetchData_Click(object sender, EventArgs e)
		{
			ExecuteMethod(FetchData);
		}

		private void buttonGenerate_Click(object sender, EventArgs eArgs)
		{
			ExecuteMethod(GenerateCode);
		}

		private void buttonSaveSettings_Click(object sender, EventArgs e)
		{
			entitySelector.SaveSelection();
			var text = JsonConvert.SerializeObject(settings, Formatting.Indented,
				new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });

			fileHelper.SaveFile("Save settings ...", "JSON", "json", "CrmSchema-Config.json",
				SavedFileType.Settings, text);

			////SettingsSaved = true;
		}

		private void buttonSaveAsSettings_Click(object sender, EventArgs e)
		{
			entitySelector.SaveSelection();
			var text = JsonConvert.SerializeObject(settings, Formatting.Indented,
				new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });

			var savedFileGroup = fileHelper.SaveFile("Save settings ...", "JSON", "json", "CrmSchema-Config.json",
				SavedFileType.Settings, text, true);

			savedFileGroup.SavedFiles.Remove(SavedFileType.Code);
			UpdateFilePathsUi();

			////SettingsSaved = true;
		}

		private void buttonLoadSettings_Click(object sender, EventArgs e)
		{
			var text = fileHelper.LoadFile("Load settings ...", "JSON", "json", SavedFileType.Settings);

			if (text.IsFilled())
			{
				ProcessSavedSettings(text);
			}
		}

		private void buttonHistory_Click(object sender, EventArgs e)
		{
			using var form = new Form();
			var control = new MenuPanel(pluginSettings.SavedFiles
				.OrderBy(s => s.Key)
				.ToDictionary(s => (object)s.Value, s => s.Key),
				pluginSettings.SavedFiles.FirstNotNullOrDefault(pluginSettings.LatestPath ?? ""));

			control.ItemClicked +=
				(s, a) =>
				{
					if (a?.Value is SavedFileGroup savedGroup)
					{
						pluginSettings.LatestPath = savedGroup.SavedFiles[SavedFileType.Settings].Path;
						LoadSavedSettings();
						form.Close();
					}
				};

			control.ItemDeleted +=
				(s, a) =>
				{
					pluginSettings.SavedFiles.Remove(a.Label);

					if (pluginSettings.LatestPath == a.Label)
					{
						pluginSettings.LatestPath = pluginSettings.SavedFiles.FirstOrDefault().Value?.SavedFiles
							.FirstNotNullOrDefault(SavedFileType.Settings)?.Path;
					}
				};

			form.Controls.Add(control);

			form.AutoSize = true;
			form.AutoSizeMode = AutoSizeMode.GrowAndShrink;

			form.FormBorderStyle = FormBorderStyle.None;
			form.ShowInTaskbar = false;

			form.StartPosition = FormStartPosition.Manual;
			form.Location = new Point(MousePosition.X, MousePosition.Y);

			form.ShowDialog();
		}

		private void buttonClearCache_Click(object sender, EventArgs e)
		{
			metadataCache.Clear();
			uiHelper.ShowToast("Cache cleared.");
		}

		private void textBoxNamespace_TextChanged(object sender, EventArgs e)
		{
			settings.Namespace = textBoxNamespace.Text;
			////SettingsSaved = false;
		}

		private void labelYagasoft_Click(object sender, EventArgs e)
		{
			Process.Start(new ProcessStartInfo("http://yagasoft.com"));
		}

		private void linkDownVs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://marketplace.visualstudio.com/items?itemName=Yagasoft.CrmCodeGenerator"));
		}

		private void linkQuickGuide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("http://blog.yagasoft.com/2020/09/dynamics-template-based-code-generator-supercharged"));
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			if (mapper != null)
			{
				uiHelper.ShowToast("Cancelling generation ...");
				mapper.CancelMapping = true;
			}
		}

		private void buttonDefaultT4_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show($"Resetting the template will overwrite the one in the editor. Are you sure you want to proceed?",
				$"Template Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				templateViewModel.CodeEditorT4.Text = DefaultTemplate.Text;
				uiHelper.ShowToast("Template text has been reset to the default.");
			}
		}

		#endregion

		private void LoadSavedSettings()
		{
			var savedFile = fileHelper.GetSavedFileInfo(SavedFileType.Settings);
			var isSettingsDefined = savedFile?.Path?.IsFilled() != true;

			if (isSettingsDefined)
			{
				if (!templateViewModel.CodeEditorT4.Text.IsFilled())
				{
					templateViewModel.CodeEditorT4.Text = DefaultTemplate.Text;
				}
			}

			try
			{
				ProcessSavedSettings(isSettingsDefined ? null : File.ReadAllText(savedFile.Path));
			}
			catch (Exception ex)
			{
				Status.PopException(Dispatcher.CurrentDispatcher, $"Failed to process saved settings => {ex.Message}");
			}
		}

		private void ProcessSavedSettings(string text = null)
		{
			if (text?.IsFilled() == true)
			{
				settings = JsonConvert.DeserializeObject<Settings>(text);
			}

			var isNew = settings == null;

			if (settings == null)
			{
				CreateNewSettings();
				uiHelper.ShowToast("Created new settings.");
			}

			if (settings.SettingsVersion.IsFilled() && new Version(settings.SettingsVersion) > new Version(Constants.SettingsVersion))
			{
				throw new NotSupportedException(
					"Settings verion is not supported by this plugin. To protect the file, loading has been aborted.");
			}

			settings.Threads = 1;
			settings.EntitiesPerThread = 9999;
			textBoxNamespace.Text = settings.Namespace;

			entitySelector.Settings = settings;

			if (!templateViewModel.CodeEditorT4.Text.IsFilled())
			{
				templateViewModel.CodeEditorT4.Text = DefaultTemplate.Text;
			}

			if (isNew)
			{
				uiHelper.ShowToast("Settings loaded.");
				return;
			}

			var savedFileGroup = fileHelper.GetSavedFileGroup();

			if (savedFileGroup == null)
			{
				throw new ArgumentNullException(nameof(savedFileGroup), "Saved file group not found.");
			}

			var savedFile = fileHelper.GetSavedFileInfo(SavedFileType.Code);

			if (savedFile == null)
			{
				savedFile = new SavedFile();
				savedFileGroup.SavedFiles[SavedFileType.Code] = savedFile;
			}

			savedFile = fileHelper.GetSavedFileInfo(SavedFileType.Template);

			if (savedFile == null)
			{
				savedFile = new SavedFile();
				savedFileGroup.SavedFiles[SavedFileType.Template] = savedFile;
			}

			if (savedFile.Path != null && File.Exists(savedFile.Path))
			{
				templateViewModel.CodeEditorT4.Text = File.ReadAllText(savedFile.Path);
				templateViewModel.T4Saved = true;
			}
			else if (!templateViewModel.CodeEditorT4.Text.IsFilled())
			{
				templateViewModel.CodeEditorT4.Text = DefaultTemplate.Text;
			}

			UpdateFilePathsUi();

			uiHelper.ShowToast("Settings loaded.");
		}

		private void CreateNewSettings()
		{
			settings =
				new Settings
				{
					SettingsVersion = Constants.SettingsVersion,
					IsAddEntityAnnotations = true,
					IsGenerateAlternateKeys = true
				};
		}

		private void FetchData()
		{
			RunAsync("Refreshing metadata ...", progressReporter => entitySelector.ResetList());
		}

		private void RunAsync(string message, Action<Action<int, string>> work, Action callback = null)
		{
			RunAsync<object>(message,
				progressReporter =>
				{
					work(progressReporter);
					return null;
				},
				result => callback?.Invoke());
		}

		private void RunAsync<TOut>(string message, Func<Action<int, string>, TOut> work, Action<TOut> callback = null)
		{
			DisableTool();

			WorkAsync(
				new WorkAsyncInfo
				{
					Message = message,
					Work =
						(w, e) =>
						{
							try
							{
								work(w.ReportProgress);
							}
							finally
							{
								EnableTool();
							}
						},
					ProgressChanged =
						e =>
						{
							// If progress has to be notified to user, use the following method:
							SetWorkingMessage(e.UserState.ToString());

							// If progress has to be notified to user, through the
							// status bar, use the following method
							SendMessageToStatusBar?.Invoke(this,
								new StatusBarMessageEventArgs(e.ProgressPercentage, e.UserState.ToString()));
						},
					PostWorkCallBack =
						e =>
						{
							SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(""));
							callback?.Invoke((TOut)e.Result);
						},
					AsyncArgument = null,
					IsCancelable = false,
					MessageWidth = 340,
					MessageHeight = 150
				});
		}

		private void GenerateCode()
		{
			uiHelper.ShowToast("Generating code ...");

			var template = templateViewModel.CodeEditorT4.Text;

			DisableTool();

			buttonCancel.Show();

			WorkAsync(
				new WorkAsyncInfo
				{
					Message = "Generating code ...",
					Work =
						(w, e) =>
						{
							Context context = null;
							var isCancelled = false;
							var isError = false;

							BusyMessage<Style> MapperOnMessage(object o, MapperEventArgs args)
							{
								try
								{
									if (args.Exception == null)
									{
										var message = $"[Generator] {Regex.Replace(args.Message, "^>> ", "[DONE] ")}";
										w.ReportProgress(args.Progress ?? -1, message);
									}
								}
								catch
								{
									// ignored
								}

								return null;
							}

							void MapperOnStatusUpdate(object o, MapperEventArgs args)
							{
								try
								{
									switch (args.Status)
									{
										case MapperStatus.Cancelled:
											isCancelled = true;
											UnregisterMapperEvents();
											break;

										case MapperStatus.Error:
											isError = true;
											UnregisterMapperEvents();
											var message = args.Exception.Message;
											var inner = args.Exception?.InnerException;

											if (inner?.Message.IsFilled() == true)
											{
												message += $" | {inner.Message}";
											}

											if (args.Exception is NullReferenceException)
											{
												message = $"Generator failed. Clear the cache and try again.";
											}

											MessageBox.Show(message, "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
											break;

										case MapperStatus.Finished:
											UnregisterMapperEvents();
											context = mapper.Context;
											context.Namespace = settings.Namespace;
											context.SplitFiles = settings.SplitFiles;
											context.SplitContractFiles = settings.SplitContractFiles;
											context.UseDisplayNames = settings.UseDisplayNames;
											context.IsUseCustomDictionary = settings.IsUseCustomDictionary;
											context.IsUseCustomEntityReference = settings.IsUseCustomEntityReference;
											context.IsAddEntityAnnotations = settings.IsAddEntityAnnotations;
											context.IsAddContractAnnotations = settings.IsAddContractAnnotations;
											context.IsGenerateLoadPerRelation = settings.IsGenerateLoadPerRelation;
											context.IsGenerateEnumNames = settings.IsGenerateEnumNames;
											context.IsGenerateEnumLabels = settings.IsGenerateEnumLabels;
											context.IsGenerateFieldSchemaNames = settings.IsGenerateFieldSchemaNames;
											context.IsGenerateFieldLabels = settings.IsGenerateFieldLabels;
											context.IsGenerateRelationNames = settings.IsGenerateRelationNames;
											context.IsImplementINotifyProperty = settings.IsImplementINotifyProperty;
											context.GenerateGlobalActions = settings.GenerateGlobalActions;
											context.PluginMetadataEntities = settings.PluginMetadataEntitiesSelected.ToList();
											context.OptionsetLabelsEntities = settings.OptionsetLabelsEntitiesSelected.ToList();
											context.LookupLabelsEntities = settings.LookupLabelsEntitiesSelected.ToList();
											context.JsEarlyBoundEntities = settings.JsEarlyBoundEntitiesSelected.ToList();
											context.EarlyBoundFilteredSelected = settings.EarlyBoundFilteredSelected.ToList();
											context.SelectedActions = settings.SelectedActions;
											context.ClearMode = settings.ClearMode;
											context.SelectedEntities = settings.EntitiesSelected.ToArray();
											context.IsGenerateAlternateKeys = settings.IsGenerateAlternateKeys;
											context.IsUseCustomTypeForAltKeys = settings.IsUseCustomTypeForAltKeys;
											context.IsMakeCrmEntitiesJsonFriendly = settings.IsMakeCrmEntitiesJsonFriendly;
											context.CrmEntityProfiles = settings.CrmEntityProfiles;
											break;
									}
								}
								catch
								{
									// ignored
								}
							}

							void RegisterMapperEvents()
							{
								mapper.Message += MapperOnMessage;
								mapper.StatusUpdate += MapperOnStatusUpdate;
							}

							void UnregisterMapperEvents()
							{
								mapper.Message -= MapperOnMessage;
								mapper.StatusUpdate -= MapperOnStatusUpdate;
							}

							try
							{
								w.ReportProgress(0, $"Gathering metadata from server ...");

								settings.ConnectionString = ConnectionDetail.GetConnectionString();

								mapper = new Mapper(settings, connectionManager, metadataCache);

								try
								{
									RegisterMapperEvents();
									mapper.MapContext();
								}
								catch (OperationCanceledException)
								{
									isCancelled = true;
								}
								catch (Exception ex)
								{
									MessageBox.Show(ex.ToString(), "Generation Error", MessageBoxButtons.OK,
										MessageBoxIcon.Error);
									return;
								}

								if (isError)
								{
									return;
								}

								if (isCancelled)
								{
									uiHelper.ShowToast("Generation cancelled.");
									return;
								}

								w.ReportProgress(99, $"Generating code ...");

								var host = new DefaultTemplateEngineHost { TemplateFileValue = "" };
								host.CreateSession();
								host.AddToSession("Context", context);

								var engine = new Engine();
								var output = engine.ProcessTemplate(template, host);

								if (host.Errors.HasErrors)
								{
									var errors = host.Errors.Cast<object>()
										.Select(s => s is CompilerError error ? $"{error.ErrorText} @{error.Line}|{error.Column}" : s.ToString())
										.StringAggregate("\r\n");

									MessageBox.Show(errors, "Generation Errors", MessageBoxButtons.OK,
										MessageBoxIcon.Error);
								}
								else
								{
									uiHelper.ShowToast("Code generated.");
								}

								e.Result = output;
							}
							finally
							{
								UnregisterMapperEvents();
								InvokeSafe(() => buttonCancel.Hide());
								EnableTool();
							}
						},
					ProgressChanged =
						e =>
						{
							// If progress has to be notified to user, use the following method:
							SetWorkingMessage(e.UserState.ToString());

							if (e.ProgressPercentage < 0)
							{
								return;
							}

							// If progress has to be notified to user, through the
							// status bar, use the following method
							SendMessageToStatusBar?.Invoke(this,
								new StatusBarMessageEventArgs(e.ProgressPercentage, e.UserState.ToString()));
						},
					PostWorkCallBack =
						e =>
						{
							SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(""));

							if (e.Result != null)
							{
								templateViewModel.CodeEditorCs.Text = (string)e.Result;
							}
						},
					AsyncArgument = null,
					IsCancelable = false,
					MessageWidth = 340,
					MessageHeight = 150
				});
		}

		private void EnableTool()
		{
			InvokeSafe(
				() =>
				{
					tableLayoutMainPanel.Enabled = true;
					toolBar.Enabled = true;
				});
		}

		private void DisableTool()
		{
			InvokeSafe(
				() =>
				{
					toolBar.Enabled = false;
					tableLayoutMainPanel.Enabled = false;
				});
		}

		private void UpdateFilePathsUi()
		{
			var savedFile = fileHelper.GetSavedFileInfo(SavedFileType.Settings);
			labelSettingsPath.Text = savedFile?.Path ?? "";

			savedFile = fileHelper.GetSavedFileInfo(SavedFileType.Template);
			templateEditor.LabelTemplatePath.Text = savedFile?.Path ?? "";

			savedFile = fileHelper.GetSavedFileInfo(SavedFileType.Code);
			templateEditor.LabelCodePath.Text = savedFile?.Path ?? "";
		}

		private void ShowTemplateEditor()
		{
			entitySelector.SaveSelection();

			buttonTemplateEditor.Enabled = false;
			buttonEntitySelector.Enabled = true;
			buttonFetchData.Enabled = false;
			buttonGenerate.Enabled = true;

			panelHost.Controls.Clear();
			panelHost.Controls.Add(templateEditor);
			templateEditor.Dock = DockStyle.Fill;
		}

		private void ShowEntitySelector()
		{
			buttonTemplateEditor.Enabled = true;
			buttonEntitySelector.Enabled = false;
			buttonFetchData.Enabled = true;
			buttonGenerate.Enabled = false;

			panelHost.Controls.Clear();
			entitySelector.ResetView();
			panelHost.Controls.Add(entitySelector);
			entitySelector.Dock = DockStyle.Fill;
		}

		private void ShowOptions()
		{
			buttonTemplateEditor.Enabled = true;
			buttonEntitySelector.Enabled = true;
			buttonFetchData.Enabled = false;
			buttonGenerate.Enabled = false;

			panelHost.Controls.Clear();
			entitySelector.ShowView(new Options(settings, connectionManager, metadataCache, workerHelper));
			panelHost.Controls.Add(entitySelector);
			entitySelector.Dock = DockStyle.Fill;
		}
	}

	public class ConnectionManager : IConnectionManager<IDisposableOrgSvc>
	{
		public Func<IOrganizationService> ServiceGetter { get; set; }

		public ConnectionManager(Func<IOrganizationService> serviceGetter)
		{
			ServiceGetter = serviceGetter;
		}

		public IDisposableOrgSvc Get(string connectionString = null)
		{
			return new DisposableOrgSvc(ServiceGetter());
		}
	}

	public class DisposableOrgSvc : DisposableOrgSvcBase
	{
		public DisposableOrgSvc(IOrganizationService innerService) : base(innerService)
		{ }

		public override void Dispose()
		{ }
	}
}
