#region Imports

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Newtonsoft.Json;
using ScintillaNET;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;
using Yagasoft.CrmCodeGenerator.Cache.Metadata;
using Yagasoft.CrmCodeGenerator.Connection;
using Yagasoft.CrmCodeGenerator.Connection.OrgSvcs;
using Yagasoft.CrmCodeGenerator.Mapper;
using Yagasoft.CrmCodeGenerator.Models.Cache;
using Yagasoft.CrmCodeGenerator.Models.Mapper;
using Yagasoft.CrmCodeGenerator.Models.Settings;
using Yagasoft.Libraries.Common;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings.File;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModel;
using Yagasoft.TemplateCodeGeneratorPlugin.Templates;
using Label = System.Windows.Forms.Label;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public partial class PluginControl : PluginControlBase, IStatusBarMessenger
	{
		private bool SettingsSaved
		{
			get => settingsSaved;
			set
			{
				settingsSaved = value;
				buttonSaveSettings.Enabled = !value;
			}
		}

		private bool T4Saved
		{
			get => t4Saved;
			set
			{
				t4Saved = value;
				buttonSaveT4.Enabled = !value;
			}
		}

		private bool CsSaved
		{
			get => csSaved;
			set
			{
				csSaved = value;
				buttonSaveCs.Enabled = !value;
			}
		}

		private int EntitiesCount
		{
			set
			{
				labelCount.ForeColor = value > 0 ? Color.Red : Color.Blue;
				labelCount.Text = $"({value})";
				buttonGenerate.Enabled = value > 0;
			}
		}

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
		private ToolStripLabel labelSettingsPath;
		private LinkLabel linkDownVs;

		private Label labelNamespace;
		private TextBox textBoxNamespace;

		private GroupBox groupBoxEntities;
		private Label labelCount;

		private Panel panelEntities;
		private Label labelFilter;
		private TextBox textBoxFilterEntities;
		private Button buttonClearFilter;
		private Button buttonSortEntities;
		private CheckedListBox checkedListBoxEntities;

		private TableLayoutPanel tableLayoutTextPanels;

		private Panel panelTemplate;
		private Panel panelCodeEditorT4;
		private Label labelTemplate;
		private Button buttonLoadT4;
		private Button buttonSaveAsT4;
		private Button buttonSaveT4;
		private Scintilla codeEditorT4;

		private Panel panelCode;
		private Label labelCode;
		private Button buttonSaveAsCs;
		private Button buttonSaveCs;
		private Label labelTemplatePath;
		private Label labelCodePath;
		private Panel panelCodeEditorCs;
		private Scintilla codeEditorCs;

		private readonly PluginSettings pluginSettings;
		
		private Settings settings;
		
		private bool settingsSaved;
		private bool t4Saved;
		private bool csSaved;
		private Button buttonCancel;

		private Mapper mapper;

		// load T4 Generator required assemblies
		private RefreshMode dummy1 = RefreshMode.KeepChanges;

		#region Base tool implementation

		public PluginControl()
		{
			InitializeComponent();

			SettingsManager.Instance.TryLoad(typeof(TemplateCodeGeneratorPlugin), out pluginSettings);

			if (pluginSettings == null)
			{
				pluginSettings = new PluginSettings();
			}
		}

		private void BtnCloseClick(object sender, EventArgs e)
		{
			if (!SettingsSaved && settings != null)
			{
				if (!PromptSave("Settings"))
				{
					return;
				}
			}

			if (!T4Saved && codeEditorT4.Text.IsFilled())
			{
				if (!PromptSave("Template"))
				{
					return;
				}
			}

			if (!CsSaved && codeEditorCs.Text.IsFilled())
			{
				if (!PromptSave("Code"))
				{
					return;
				}
			}

			////var isSavedFileExist = pluginSettings.SavedFiles.TryGetValue(SavedFileType.Template, out var savedFile);

			////if (!isSavedFileExist)
			////{
			////	savedFile = new SavedFile();
			////	pluginSettings.SavedFiles[SavedFileType.Template] = savedFile;
			////}

			////var isExists = savedFile.Path.IsFilled() && File.Exists(savedFile.Path);
			////var isModified = !isExists || File.ReadAllText(savedFile.Path) != codeEditorT4.Text;

			////if (codeEditorT4.Text.IsFilled() && !T4Saved && isModified)
			////{
			////	if (!PromptSave("Template"))
			////	{
			////		return;
			////	}
			////}

			////isSavedFileExist = pluginSettings.SavedFiles.TryGetValue(SavedFileType.Code, out savedFile);

			////if (!isSavedFileExist)
			////{
			////	savedFile = new SavedFile();
			////	pluginSettings.SavedFiles[SavedFileType.Code] = savedFile;
			////}

			////isExists = savedFile.Path.IsFilled() && File.Exists(savedFile.Path);
			////isModified = !isExists || File.ReadAllText(savedFile.Path) != codeEditorCs.Text;

			////if (codeEditorCs.Text.IsFilled() && !CsSaved && isModified)
			////{
			////	if (!PromptSave("Code"))
			////	{
			////		return;
			////	}
			////}

			CloseTool(); // PluginBaseControl method that notifies the XrmToolBox that the user wants to close the plugin
			// Override the ClosingPlugin method to allow for any plugin specific closing logic to be performed (saving configs, canceling close, etc...)
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
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonFetchData = new System.Windows.Forms.ToolStripButton();
			this.buttonGenerate = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonLoadSettings = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveAsSettings = new System.Windows.Forms.ToolStripButton();
			this.buttonSaveSettings = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonClearCache = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.labelSettingsPath = new System.Windows.Forms.ToolStripLabel();
			this.labelCode = new System.Windows.Forms.Label();
			this.textBoxFilterEntities = new System.Windows.Forms.TextBox();
			this.buttonClearFilter = new System.Windows.Forms.Button();
			this.checkedListBoxEntities = new System.Windows.Forms.CheckedListBox();
			this.panelCodeEditorT4 = new System.Windows.Forms.Panel();
			this.panelCodeEditorCs = new System.Windows.Forms.Panel();
			this.buttonLoadT4 = new System.Windows.Forms.Button();
			this.buttonSaveT4 = new System.Windows.Forms.Button();
			this.buttonSaveCs = new System.Windows.Forms.Button();
			this.tableLayoutTextPanels = new System.Windows.Forms.TableLayoutPanel();
			this.panelTemplate = new System.Windows.Forms.Panel();
			this.labelTemplatePath = new System.Windows.Forms.Label();
			this.buttonSaveAsT4 = new System.Windows.Forms.Button();
			this.labelTemplate = new System.Windows.Forms.Label();
			this.panelCode = new System.Windows.Forms.Panel();
			this.labelCodePath = new System.Windows.Forms.Label();
			this.buttonSaveAsCs = new System.Windows.Forms.Button();
			this.labelCount = new System.Windows.Forms.Label();
			this.buttonSortEntities = new System.Windows.Forms.Button();
			this.textBoxNamespace = new System.Windows.Forms.TextBox();
			this.labelNamespace = new System.Windows.Forms.Label();
			this.groupBoxEntities = new System.Windows.Forms.GroupBox();
			this.panelEntities = new System.Windows.Forms.Panel();
			this.labelFilter = new System.Windows.Forms.Label();
			this.linkDownVs = new System.Windows.Forms.LinkLabel();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.toolBar.SuspendLayout();
			this.tableLayoutTextPanels.SuspendLayout();
			this.panelTemplate.SuspendLayout();
			this.panelCode.SuspendLayout();
			this.groupBoxEntities.SuspendLayout();
			this.panelEntities.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolBar
			// 
			this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonCloseTool,
            this.toolStripSeparator1,
            this.buttonFetchData,
            this.buttonGenerate,
            this.toolStripSeparator2,
            this.buttonLoadSettings,
            this.buttonSaveAsSettings,
            this.buttonSaveSettings,
            this.toolStripSeparator3,
            this.buttonClearCache,
            this.toolStripSeparator5,
            this.labelSettingsPath});
			this.toolBar.Location = new System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.Size = new System.Drawing.Size(1184, 25);
			this.toolBar.TabIndex = 0;
			this.toolBar.Text = "toolBar";
			// 
			// buttonCloseTool
			// 
			this.buttonCloseTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonCloseTool.Image")));
			this.buttonCloseTool.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonCloseTool.Name = "buttonCloseTool";
			this.buttonCloseTool.Size = new System.Drawing.Size(81, 22);
			this.buttonCloseTool.Text = "Close Tool";
			this.buttonCloseTool.Click += new System.EventHandler(this.BtnCloseClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonFetchData
			// 
			this.buttonFetchData.Image = ((System.Drawing.Image)(resources.GetObject("buttonFetchData.Image")));
			this.buttonFetchData.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonFetchData.Name = "buttonFetchData";
			this.buttonFetchData.Size = new System.Drawing.Size(83, 22);
			this.buttonFetchData.Text = "Fetch Data";
			this.buttonFetchData.Click += new System.EventHandler(this.buttonFetchData_Click);
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
			// buttonLoadSettings
			// 
			this.buttonLoadSettings.Enabled = false;
			this.buttonLoadSettings.Image = ((System.Drawing.Image)(resources.GetObject("buttonLoadSettings.Image")));
			this.buttonLoadSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonLoadSettings.Name = "buttonLoadSettings";
			this.buttonLoadSettings.Size = new System.Drawing.Size(98, 22);
			this.buttonLoadSettings.Text = "Load Settings";
			this.buttonLoadSettings.Click += new System.EventHandler(this.buttonLoadSettings_Click);
			// 
			// buttonSaveAsSettings
			// 
			this.buttonSaveAsSettings.Enabled = false;
			this.buttonSaveAsSettings.Image = ((System.Drawing.Image)(resources.GetObject("buttonSaveAsSettings.Image")));
			this.buttonSaveAsSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveAsSettings.Name = "buttonSaveAsSettings";
			this.buttonSaveAsSettings.Size = new System.Drawing.Size(112, 22);
			this.buttonSaveAsSettings.Text = "Save Settings As";
			this.buttonSaveAsSettings.Click += new System.EventHandler(this.buttonSaveAsSettings_Click);
			// 
			// buttonSaveSettings
			// 
			this.buttonSaveSettings.Enabled = false;
			this.buttonSaveSettings.Image = ((System.Drawing.Image)(resources.GetObject("buttonSaveSettings.Image")));
			this.buttonSaveSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSaveSettings.Name = "buttonSaveSettings";
			this.buttonSaveSettings.Size = new System.Drawing.Size(96, 22);
			this.buttonSaveSettings.Text = "Save Settings";
			this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonClearCache
			// 
			this.buttonClearCache.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.buttonClearCache.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearCache.Image")));
			this.buttonClearCache.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClearCache.Name = "buttonClearCache";
			this.buttonClearCache.Size = new System.Drawing.Size(74, 22);
			this.buttonClearCache.Text = "Clear Cache";
			this.buttonClearCache.Click += new System.EventHandler(this.buttonClearCache_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// labelSettingsPath
			// 
			this.labelSettingsPath.Name = "labelSettingsPath";
			this.labelSettingsPath.Size = new System.Drawing.Size(93, 22);
			this.labelSettingsPath.Text = "<settings-path>";
			// 
			// labelCode
			// 
			this.labelCode.AutoSize = true;
			this.labelCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCode.Location = new System.Drawing.Point(3, 4);
			this.labelCode.Name = "labelCode";
			this.labelCode.Size = new System.Drawing.Size(99, 13);
			this.labelCode.TabIndex = 11;
			this.labelCode.Text = "Generated Code";
			// 
			// textBoxFilterEntities
			// 
			this.textBoxFilterEntities.Location = new System.Drawing.Point(42, 2);
			this.textBoxFilterEntities.Name = "textBoxFilterEntities";
			this.textBoxFilterEntities.Size = new System.Drawing.Size(145, 20);
			this.textBoxFilterEntities.TabIndex = 12;
			this.textBoxFilterEntities.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxFilterEntities_KeyUp);
			// 
			// buttonClearFilter
			// 
			this.buttonClearFilter.Location = new System.Drawing.Point(193, 0);
			this.buttonClearFilter.Name = "buttonClearFilter";
			this.buttonClearFilter.Size = new System.Drawing.Size(40, 23);
			this.buttonClearFilter.TabIndex = 13;
			this.buttonClearFilter.Text = "Clear";
			this.buttonClearFilter.UseVisualStyleBackColor = true;
			this.buttonClearFilter.Click += new System.EventHandler(this.buttonClearFilter_Click);
			// 
			// checkedListBoxEntities
			// 
			this.checkedListBoxEntities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.checkedListBoxEntities.CheckOnClick = true;
			this.checkedListBoxEntities.FormattingEnabled = true;
			this.checkedListBoxEntities.Location = new System.Drawing.Point(4, 48);
			this.checkedListBoxEntities.Name = "checkedListBoxEntities";
			this.checkedListBoxEntities.Size = new System.Drawing.Size(229, 409);
			this.checkedListBoxEntities.TabIndex = 14;
			this.checkedListBoxEntities.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxEntities_ItemCheck);
			// 
			// panelCodeEditorT4
			// 
			this.panelCodeEditorT4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCodeEditorT4.Location = new System.Drawing.Point(3, 49);
			this.panelCodeEditorT4.Name = "panelCodeEditorT4";
			this.panelCodeEditorT4.Size = new System.Drawing.Size(452, 449);
			this.panelCodeEditorT4.TabIndex = 15;
			// 
			// panelCodeEditorCs
			// 
			this.panelCodeEditorCs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCodeEditorCs.Location = new System.Drawing.Point(461, 49);
			this.panelCodeEditorCs.Name = "panelCodeEditorCs";
			this.panelCodeEditorCs.Size = new System.Drawing.Size(452, 449);
			this.panelCodeEditorCs.TabIndex = 15;
			// 
			// buttonLoadT4
			// 
			this.buttonLoadT4.Location = new System.Drawing.Point(131, 0);
			this.buttonLoadT4.Name = "buttonLoadT4";
			this.buttonLoadT4.Size = new System.Drawing.Size(75, 20);
			this.buttonLoadT4.TabIndex = 16;
			this.buttonLoadT4.Text = "Load";
			this.buttonLoadT4.UseVisualStyleBackColor = true;
			this.buttonLoadT4.Click += new System.EventHandler(this.buttonLoadT4_Click);
			// 
			// buttonSaveT4
			// 
			this.buttonSaveT4.Enabled = false;
			this.buttonSaveT4.Location = new System.Drawing.Point(293, 0);
			this.buttonSaveT4.Name = "buttonSaveT4";
			this.buttonSaveT4.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveT4.TabIndex = 17;
			this.buttonSaveT4.Text = "Save";
			this.buttonSaveT4.UseVisualStyleBackColor = true;
			this.buttonSaveT4.Click += new System.EventHandler(this.buttonSaveT4_Click);
			// 
			// buttonSaveCs
			// 
			this.buttonSaveCs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveCs.Enabled = false;
			this.buttonSaveCs.Location = new System.Drawing.Point(213, 0);
			this.buttonSaveCs.Name = "buttonSaveCs";
			this.buttonSaveCs.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveCs.TabIndex = 18;
			this.buttonSaveCs.Text = "Save";
			this.buttonSaveCs.UseVisualStyleBackColor = true;
			this.buttonSaveCs.Click += new System.EventHandler(this.buttonSaveCs_Click);
			// 
			// tableLayoutTextPanels
			// 
			this.tableLayoutTextPanels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutTextPanels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutTextPanels.ColumnCount = 2;
			this.tableLayoutTextPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutTextPanels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutTextPanels.Controls.Add(this.panelTemplate, 0, 0);
			this.tableLayoutTextPanels.Controls.Add(this.panelCodeEditorT4, 0, 1);
			this.tableLayoutTextPanels.Controls.Add(this.panelCodeEditorCs, 1, 1);
			this.tableLayoutTextPanels.Controls.Add(this.panelCode, 1, 0);
			this.tableLayoutTextPanels.Location = new System.Drawing.Point(260, 25);
			this.tableLayoutTextPanels.Name = "tableLayoutTextPanels";
			this.tableLayoutTextPanels.RowCount = 2;
			this.tableLayoutTextPanels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutTextPanels.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutTextPanels.Size = new System.Drawing.Size(916, 501);
			this.tableLayoutTextPanels.TabIndex = 0;
			// 
			// panelTemplate
			// 
			this.panelTemplate.Controls.Add(this.labelTemplatePath);
			this.panelTemplate.Controls.Add(this.buttonSaveAsT4);
			this.panelTemplate.Controls.Add(this.labelTemplate);
			this.panelTemplate.Controls.Add(this.buttonSaveT4);
			this.panelTemplate.Controls.Add(this.buttonLoadT4);
			this.panelTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTemplate.Location = new System.Drawing.Point(3, 3);
			this.panelTemplate.Name = "panelTemplate";
			this.panelTemplate.Size = new System.Drawing.Size(452, 40);
			this.panelTemplate.TabIndex = 0;
			// 
			// labelTemplatePath
			// 
			this.labelTemplatePath.AutoSize = true;
			this.labelTemplatePath.Location = new System.Drawing.Point(3, 22);
			this.labelTemplatePath.Name = "labelTemplatePath";
			this.labelTemplatePath.Size = new System.Drawing.Size(83, 13);
			this.labelTemplatePath.TabIndex = 19;
			this.labelTemplatePath.Text = "<template-path>";
			// 
			// buttonSaveAsT4
			// 
			this.buttonSaveAsT4.Enabled = false;
			this.buttonSaveAsT4.Location = new System.Drawing.Point(212, 0);
			this.buttonSaveAsT4.Name = "buttonSaveAsT4";
			this.buttonSaveAsT4.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveAsT4.TabIndex = 18;
			this.buttonSaveAsT4.Text = "Save As";
			this.buttonSaveAsT4.UseVisualStyleBackColor = true;
			this.buttonSaveAsT4.Click += new System.EventHandler(this.buttonSaveAsT4_Click);
			// 
			// labelTemplate
			// 
			this.labelTemplate.AutoSize = true;
			this.labelTemplate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelTemplate.Location = new System.Drawing.Point(3, 4);
			this.labelTemplate.Name = "labelTemplate";
			this.labelTemplate.Size = new System.Drawing.Size(78, 13);
			this.labelTemplate.TabIndex = 7;
			this.labelTemplate.Text = "T4 Template";
			// 
			// panelCode
			// 
			this.panelCode.Controls.Add(this.labelCodePath);
			this.panelCode.Controls.Add(this.buttonSaveAsCs);
			this.panelCode.Controls.Add(this.labelCode);
			this.panelCode.Controls.Add(this.buttonSaveCs);
			this.panelCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCode.Location = new System.Drawing.Point(461, 3);
			this.panelCode.Name = "panelCode";
			this.panelCode.Size = new System.Drawing.Size(452, 40);
			this.panelCode.TabIndex = 16;
			// 
			// labelCodePath
			// 
			this.labelCodePath.AutoSize = true;
			this.labelCodePath.Location = new System.Drawing.Point(3, 22);
			this.labelCodePath.Name = "labelCodePath";
			this.labelCodePath.Size = new System.Drawing.Size(67, 13);
			this.labelCodePath.TabIndex = 20;
			this.labelCodePath.Text = "<code-path>";
			// 
			// buttonSaveAsCs
			// 
			this.buttonSaveAsCs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveAsCs.Enabled = false;
			this.buttonSaveAsCs.Location = new System.Drawing.Point(132, 0);
			this.buttonSaveAsCs.Name = "buttonSaveAsCs";
			this.buttonSaveAsCs.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveAsCs.TabIndex = 19;
			this.buttonSaveAsCs.Text = "Save As";
			this.buttonSaveAsCs.UseVisualStyleBackColor = true;
			this.buttonSaveAsCs.Click += new System.EventHandler(this.buttonSaveAsCs_Click);
			// 
			// labelCount
			// 
			this.labelCount.AutoSize = true;
			this.labelCount.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.labelCount.Location = new System.Drawing.Point(61, 0);
			this.labelCount.Name = "labelCount";
			this.labelCount.Size = new System.Drawing.Size(22, 13);
			this.labelCount.TabIndex = 15;
			this.labelCount.Text = "(0)";
			// 
			// buttonSortEntities
			// 
			this.buttonSortEntities.Location = new System.Drawing.Point(4, 25);
			this.buttonSortEntities.Name = "buttonSortEntities";
			this.buttonSortEntities.Size = new System.Drawing.Size(229, 20);
			this.buttonSortEntities.TabIndex = 16;
			this.buttonSortEntities.Text = "Sort";
			this.buttonSortEntities.UseVisualStyleBackColor = true;
			this.buttonSortEntities.Click += new System.EventHandler(this.buttonSortEntities_Click);
			// 
			// textBoxNamespace
			// 
			this.textBoxNamespace.Enabled = false;
			this.textBoxNamespace.Location = new System.Drawing.Point(86, 29);
			this.textBoxNamespace.Name = "textBoxNamespace";
			this.textBoxNamespace.Size = new System.Drawing.Size(168, 20);
			this.textBoxNamespace.TabIndex = 17;
			this.textBoxNamespace.TextChanged += new System.EventHandler(this.textBoxNamespace_TextChanged);
			// 
			// labelNamespace
			// 
			this.labelNamespace.AutoSize = true;
			this.labelNamespace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNamespace.Location = new System.Drawing.Point(8, 32);
			this.labelNamespace.Name = "labelNamespace";
			this.labelNamespace.Size = new System.Drawing.Size(73, 13);
			this.labelNamespace.TabIndex = 18;
			this.labelNamespace.Text = "Namespace";
			// 
			// groupBoxEntities
			// 
			this.groupBoxEntities.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBoxEntities.Controls.Add(this.labelCount);
			this.groupBoxEntities.Controls.Add(this.panelEntities);
			this.groupBoxEntities.Enabled = false;
			this.groupBoxEntities.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxEntities.Location = new System.Drawing.Point(10, 55);
			this.groupBoxEntities.Name = "groupBoxEntities";
			this.groupBoxEntities.Size = new System.Drawing.Size(244, 471);
			this.groupBoxEntities.TabIndex = 19;
			this.groupBoxEntities.TabStop = false;
			this.groupBoxEntities.Text = "Entities";
			// 
			// panelEntities
			// 
			this.panelEntities.Controls.Add(this.labelFilter);
			this.panelEntities.Controls.Add(this.buttonSortEntities);
			this.panelEntities.Controls.Add(this.checkedListBoxEntities);
			this.panelEntities.Controls.Add(this.textBoxFilterEntities);
			this.panelEntities.Controls.Add(this.buttonClearFilter);
			this.panelEntities.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEntities.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panelEntities.Location = new System.Drawing.Point(3, 16);
			this.panelEntities.Name = "panelEntities";
			this.panelEntities.Size = new System.Drawing.Size(238, 452);
			this.panelEntities.TabIndex = 0;
			// 
			// labelFilter
			// 
			this.labelFilter.AutoSize = true;
			this.labelFilter.Location = new System.Drawing.Point(7, 5);
			this.labelFilter.Name = "labelFilter";
			this.labelFilter.Size = new System.Drawing.Size(29, 13);
			this.labelFilter.TabIndex = 19;
			this.labelFilter.Text = "Filter";
			// 
			// linkDownVs
			// 
			this.linkDownVs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkDownVs.AutoSize = true;
			this.linkDownVs.Location = new System.Drawing.Point(1057, 7);
			this.linkDownVs.Name = "linkDownVs";
			this.linkDownVs.Size = new System.Drawing.Size(121, 13);
			this.linkDownVs.TabIndex = 20;
			this.linkDownVs.TabStop = true;
			this.linkDownVs.Text = "Download VS Extension";
			this.linkDownVs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDownVs_LinkClicked);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(1002, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(49, 20);
			this.buttonCancel.TabIndex = 21;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Visible = false;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// PluginControl
			// 
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.linkDownVs);
			this.Controls.Add(this.groupBoxEntities);
			this.Controls.Add(this.labelNamespace);
			this.Controls.Add(this.textBoxNamespace);
			this.Controls.Add(this.tableLayoutTextPanels);
			this.Controls.Add(this.toolBar);
			this.Name = "PluginControl";
			this.Size = new System.Drawing.Size(1184, 529);
			this.Load += new System.EventHandler(this.PluginControl_Load);
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.tableLayoutTextPanels.ResumeLayout(false);
			this.panelTemplate.ResumeLayout(false);
			this.panelTemplate.PerformLayout();
			this.panelCode.ResumeLayout(false);
			this.panelCode.PerformLayout();
			this.groupBoxEntities.ResumeLayout(false);
			this.groupBoxEntities.PerformLayout();
			this.panelEntities.ResumeLayout(false);
			this.panelEntities.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

		#region Event handlers

		private void PluginControl_Load(object sender, EventArgs e)
		{
			codeEditorT4 = new CodeEditor(ParentForm);
			codeEditorT4.TextChanged += (o, args) => T4Saved = false;
			panelCodeEditorT4.Controls.Add(codeEditorT4);

			codeEditorCs = new CodeEditor(ParentForm);
			codeEditorCs.TextChanged += (o, args) => CsSaved = false;
			panelCodeEditorCs.Controls.Add(codeEditorCs);

			UpdateFilePathsUi();
		}

		private void buttonFetchData_Click(object sender, EventArgs e)
		{
			ExecuteMethod(FetchData);
		}

		private void textBoxFilterEntities_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				FilterEntities(textBoxFilterEntities.Text);
			}
		}

		private void buttonClearFilter_Click(object sender, EventArgs e)
		{
			ClearEntityFilter();
		}

		private void buttonGenerate_Click(object sender, EventArgs eArgs)
		{
			ExecuteMethod(GenerateCode);
		}

		private void buttonSaveSettings_Click(object sender, EventArgs e)
		{
			SetSettingsSelectedEntities();
			var text = JsonConvert.SerializeObject(settings, Formatting.Indented,
				new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });

			SaveFile("Save settings ...", "JSON", "json", "CrmSchema-Config.json",
				SavedFileType.Settings, text);

			SettingsSaved = true;
		}

		private void buttonSaveAsSettings_Click(object sender, EventArgs e)
		{
			SetSettingsSelectedEntities();
			var text = JsonConvert.SerializeObject(settings, Formatting.Indented,
				new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });

			SaveFile("Save settings ...", "JSON", "json", "CrmSchema-Config.json",
				SavedFileType.Settings, text, true);

			SettingsSaved = true;
		}

		private void buttonLoadSettings_Click(object sender, EventArgs e)
		{
			var text = LoadFile("Load settings ...", "JSON", "json", SavedFileType.Settings);
			ProcessSavedSettings(text);
		}

		private void checkedListBoxEntities_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			var selectedItem = (checkedListBoxEntities.Items[e.Index] as EntityNameViewModel)?.LogicalName;

			if (e.NewValue == CheckState.Checked)
			{
				if (!ControlData.SelectedEntityNames.Contains(selectedItem))
				{
					ControlData.SelectedEntityNames.Add(selectedItem);
				}
			}
			else
			{
				ControlData.SelectedEntityNames.Remove(selectedItem);
			}

			if (settings.EntitiesSelected.Intersect(ControlData.SelectedEntityNames).Count() != settings.EntitiesSelected.Count
				|| ControlData.SelectedEntityNames.Intersect(settings.EntitiesSelected).Count() != ControlData.SelectedEntityNames.Count)
			{
				SetSettingsSelectedEntities();
				SettingsSaved = false;
			}

			EntitiesCount = settings?.EntitiesSelected?.Count ?? 0;
		}

		private void buttonLoadT4_Click(object sender, EventArgs e)
		{
			var text = LoadFile("Load T4 Template ...", "T4 Template", "tt", SavedFileType.Template);

			if (text.IsFilled())
			{
				codeEditorT4.Text = text;
			}
		}

		private void buttonSaveT4_Click(object sender, EventArgs e)
		{
			SaveFile("Save T4 Template ...", "T4 Template", "tt", "CrmSchema.tt",
				SavedFileType.Template, codeEditorT4.Text);
			T4Saved = true;
		}

		private void buttonSaveAsT4_Click(object sender, EventArgs e)
		{
			SaveFile("Save T4 Template ...", "T4 Template", "tt", "CrmSchema.tt",
				SavedFileType.Template, codeEditorT4.Text, true);
			T4Saved = true;
		}

		private void buttonSaveCs_Click(object sender, EventArgs e)
		{
			SaveFile("Save Generated Code ...", "CS", "cs", "CrmSchema.cs",
				SavedFileType.Code, codeEditorCs.Text);
			CsSaved = true;
		}

		private void buttonSaveAsCs_Click(object sender, EventArgs e)
		{
			SaveFile("Save Generated Code ...", "CS", "cs", "CrmSchema.cs",
				SavedFileType.Code, codeEditorCs.Text, true);
			CsSaved = true;
		}

		private void buttonClearCache_Click(object sender, EventArgs e)
		{
			MetadataCacheManager.Clear();
		}

		private void buttonSortEntities_Click(object sender, EventArgs e)
		{
			SortEntities();
		}

		private void textBoxNamespace_TextChanged(object sender, EventArgs e)
		{
			settings.Namespace = textBoxNamespace.Text;
			SettingsSaved = false;
		}

		private void linkDownVs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://marketplace.visualstudio.com/items?itemName=Yagasoft.CrmCodeGenerator"));
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			if (mapper != null)
			{
				mapper.CancelMapping = true;
			}
		}

		#endregion

		private void ProcessSavedSettings(string text)
		{
			if (text.IsFilled())
			{
				settings = JsonConvert.DeserializeObject<Settings>(text);
			}

			if (settings == null)
			{
				CreateNewSettings();
			}
			else
			{
				ControlData.SelectedEntityNames = settings.EntitiesSelected.ToList();
			}

			FilterEntities(string.Empty);
			SetSettingsSelectedEntities();

			if (codeEditorT4.Text.IsFilled())
			{
				return;
			}

			var isSavedFileExist = pluginSettings.SavedFiles.TryGetValue(SavedFileType.Template, out var savedFile);

			if (!isSavedFileExist)
			{
				savedFile = new SavedFile();
				pluginSettings.SavedFiles[SavedFileType.Template] = savedFile;
				UpdateFilePathsUi();
			}

			var templatePath = savedFile.Path.IsFilled()
				? savedFile.Path
				: pluginSettings.SavedFiles[SavedFileType.Settings].Path?
					.Replace("-Config.json", ".tt").Replace(".json", ".tt");

			if (templatePath.IsFilled() && File.Exists(templatePath))
			{
				codeEditorT4.Text = File.ReadAllText(templatePath);
				T4Saved = true;
			}
			else
			{
				codeEditorT4.Text = DefaultTemplate.Text;
			}
		}

		private void CreateNewSettings()
		{
			settings =
				new Settings
				{
					IsAddEntityAnnotations = true,
					IsGenerateAlternateKeys = true
				};
		}

		private void SaveFile(string title, string fileTypeName, string fileExtension, string defaultFileName,
			SavedFileType savedFileType, string textToSave, bool isForceDialogue = false)
		{
			var saveDialogue =
				new SaveFileDialog
				{
					Title = title,
					OverwritePrompt = true,
					Filter = $"{fileTypeName} files (*.{fileExtension})|*.{fileExtension}",
				};

			var isSavedFileExist = pluginSettings.SavedFiles.TryGetValue(savedFileType, out var savedFile);

			if (!isSavedFileExist)
			{
				savedFile = new SavedFile();
				pluginSettings.SavedFiles[savedFileType] = savedFile;
			}

			if (savedFile.Path.IsFilled())
			{
				saveDialogue.FileName = savedFile.Path;
				saveDialogue.InitialDirectory = savedFile.Folder;
			}
			else
			{
				saveDialogue.FileName = defaultFileName;
			}

			if (isForceDialogue || savedFile.Path.IsEmpty())
			{
				var result = saveDialogue.ShowDialog();

				if (result != DialogResult.OK || saveDialogue.FileName.IsEmpty())
				{
					return;
				}

				if (!Path.HasExtension(saveDialogue.FileName))
				{
					saveDialogue.FileName += $".{fileExtension}";
				}
			}

			using (var stream = (FileStream)saveDialogue.OpenFile())
			{
				var array = Encoding.UTF8.GetBytes(textToSave);
				stream.Write(array, 0, array.Length);
			}
			
			savedFile.Folder = Path.GetDirectoryName(saveDialogue.FileName);
			savedFile.File = Path.GetFileName(saveDialogue.FileName);
			SettingsManager.Instance.Save(typeof(TemplateCodeGeneratorPlugin), pluginSettings);
			UpdateFilePathsUi();
		}

		private string LoadFile(string title, string fileTypeName, string fileExtension, SavedFileType savedFileType)
		{
			var openDialogue =
				new OpenFileDialog
				{
					Title = title,
					Filter = $"{fileTypeName} files (*.{fileExtension})|*.{fileExtension}",
				};

			var isSavedFileExist = pluginSettings.SavedFiles.TryGetValue(savedFileType, out var savedFile);

			if (!isSavedFileExist)
			{
				savedFile = new SavedFile();
				pluginSettings.SavedFiles[savedFileType] = savedFile;
			}

			if (savedFile.Path.IsFilled())
			{
				openDialogue.FileName = savedFile.Path;
				openDialogue.InitialDirectory = savedFile.Folder;
			}

			var result = openDialogue.ShowDialog();

			if (result != DialogResult.OK || openDialogue.FileName.IsEmpty())
			{
				return null;
			}

			string text;

			using (var reader = new StreamReader(openDialogue.FileName))
			{
				text = reader.ReadToEnd();
			}

			savedFile.Folder = Path.GetDirectoryName(openDialogue.FileName);
			savedFile.File = Path.GetFileName(openDialogue.FileName);
			SettingsManager.Instance.Save(typeof(TemplateCodeGeneratorPlugin), pluginSettings);

			return text;
		}

		private void FetchData()
		{
			checkedListBoxEntities.SelectedIndex = -1;
			checkedListBoxEntities.Items.Clear();

			DisableButtons();

			WorkAsync(
				new WorkAsyncInfo
				{
					Message = "Retrieving data ...",
					Work =
						(w, e) =>
						{
							try
							{
								w.ReportProgress(90, $"Retrieving entity names ...");
								var entityNames = DataHelpers.RetrieveEntityNames(Service)
									.Where(s => s.DisplayName.IsFilled())
									.OrderBy(s => s.ToString());

								e.Result =
									new RetrieveResult
									{
										EntityNames = entityNames
									};
							}
							finally
							{
								EnableButtons();
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
							var result = (RetrieveResult)e.Result;

							ControlData.EntityNames = result.EntityNames.ToList();

							Invoke(new Action(
								() =>
								{
									if (pluginSettings.SavedFiles.TryGetValue(SavedFileType.Settings, out var savedFile)
										&& File.Exists(savedFile.Path))
									{
										var text = File.ReadAllText(savedFile.Path);
										ProcessSavedSettings(text);
									}
									else
									{
										codeEditorT4.Text = DefaultTemplate.Text;
									}

									if (settings == null)
									{
										CreateNewSettings();
									}

									ClearEntityFilter();
								}));
						},
					AsyncArgument = null,
					IsCancelable = false,
					MessageWidth = 340,
					MessageHeight = 150
				});
		}

		private void FilterEntities(string keyword = "")
		{
			checkedListBoxEntities.SelectedIndex = -1;

			var filteredEntityName = ControlData.EntityNames
				.Where(e => e.LogicalName.Contains(keyword) || e.DisplayName.Contains(keyword));
			
			FillEntities(filteredEntityName);
			SortEntities();

			buttonClearFilter.Enabled = keyword.IsFilled();
		}

		private void ClearEntityFilter()
		{
			textBoxFilterEntities.Text = string.Empty;
			FilterEntities();
		}

		private void GenerateCode()
		{
			var template = codeEditorT4.Text;

			DisableButtons();

			buttonCancel.Visible = true;

			WorkAsync(
				new WorkAsyncInfo
				{
					Message = "Generating code ...",
					Work =
						(w, e) =>
						{
							try
							{
								w.ReportProgress(0, $"Gathering metadata from server ...");

								settings.ConnectionString = ConnectionDetail.GetConnectionString();
								settings.Threads = 1;
								settings.EntitiesPerThread = 999;

								mapper = new Mapper(settings, new ConnectionManager(Service), new MetadataCacheManager());

								Context context = null;
								var isCancelled = false;

								mapper.Message
									+= (o, args) =>
									   {
										   try
										   {
											   w.ReportProgress(args.Progress ?? -1, args.Message);
										   }
										   catch
										   {
										   // ignored
									   }
									   };

								mapper.Status
									+= (o, args) =>
									   {
										   try
										   {
											   switch (args.Status)
											   {
												   case MapperStatus.Cancelled:
													   isCancelled = true;
													   break;

												   case MapperStatus.Finished:
													   context = mapper.Context;
													   context.Namespace = settings.Namespace;
													   context.SplitFiles = settings.SplitFiles;
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
													   context.GenerateGlobalActions = settings.GenerateGlobalActions;
													   context.PluginMetadataEntities = settings.PluginMetadataEntitiesSelected.ToList();
													   context.OptionsetLabelsEntities = settings.OptionsetLabelsEntitiesSelected.ToList();
													   context.LookupLabelsEntities = settings.LookupLabelsEntitiesSelected.ToList();
													   context.JsEarlyBoundEntities = settings.JsEarlyBoundEntitiesSelected.ToList();
													   context.SelectedActions = settings.SelectedActions;
													   context.ClearMode = settings.ClearMode;
													   context.SelectedEntities = settings.EntitiesSelected.ToArray();
													   context.IsGenerateAlternateKeys = settings.IsGenerateAlternateKeys;
													   context.IsUseCustomTypeForAltKeys = settings.IsUseCustomTypeForAltKeys;
													   context.IsMakeCrmEntitiesJsonFriendly = settings.IsMakeCrmEntitiesJsonFriendly;
													   break;
											   }
										   }
										   catch
										   {
											   // ignored
										   }
									   };

								try
								{
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

								if (isCancelled)
								{
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

								e.Result = output;
							}
							finally
							{
								Invoke(new Action(() => buttonCancel.Visible = false));
								EnableButtons();
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
								codeEditorCs.Text = (string)e.Result;
							}
						},
					AsyncArgument = null,
					IsCancelable = false,
					MessageWidth = 340,
					MessageHeight = 150
				});
		}

		private void SetSettingsSelectedEntities()
		{
			settings.EntitiesSelected = new ObservableCollection<string>(ControlData.SelectedEntityNames.OrderBy(s => s));
			settings.JsEarlyBoundEntitiesSelected = new ObservableCollection<string>(settings.EntitiesSelected);
			settings.PluginMetadataEntitiesSelected = new ObservableCollection<string>(settings.EntitiesSelected);
		}

		private void EnableButtons()
		{
			Invoke(new Action(
				() =>
				{
					buttonSortEntities.Enabled = true;
					buttonLoadSettings.Enabled = true;
					buttonSaveAsSettings.Enabled = true;
					buttonSaveAsT4.Enabled = true;
					buttonLoadT4.Enabled = true;
					buttonSaveAsCs.Enabled = true;

					textBoxNamespace.Enabled = true;

					groupBoxEntities.Enabled = true;
					tableLayoutTextPanels.Enabled = true;
					toolBar.Enabled = true;
				}));
		}

		private void DisableButtons()
		{
			Invoke(new Action(
				() =>
				{
					toolBar.Enabled = false;
					tableLayoutTextPanels.Enabled = false;
					groupBoxEntities.Enabled = false;

					textBoxNamespace.Enabled = false;
				}));
		}

		private void UpdateFilePathsUi()
		{
			if (pluginSettings.SavedFiles.TryGetValue(SavedFileType.Settings, out var savedFile))
			{
				labelSettingsPath.Text = $"({savedFile.Path})";
			}

			if (pluginSettings.SavedFiles.TryGetValue(SavedFileType.Template, out savedFile))
			{
				labelTemplatePath.Text = savedFile.Path;
			}

			if (pluginSettings.SavedFiles.TryGetValue(SavedFileType.Code, out savedFile))
			{
				labelCodePath.Text = savedFile.Path;
			}
		}

		private void FillEntities(IEnumerable<EntityNameViewModel> entityList)
		{
			checkedListBoxEntities.Items.Clear();

			checkedListBoxEntities.Items.AddRange(entityList.Cast<object>().ToArray());

			for (var i = 0; i < checkedListBoxEntities.Items.Count; i++)
			{
				var item = checkedListBoxEntities.Items[i];

				if (ControlData.SelectedEntityNames.Contains((item as EntityNameViewModel)?.LogicalName))
				{
					checkedListBoxEntities.SetItemChecked(i, true);
				}
			}
		}

		private void SortEntities()
		{
			FillEntities(checkedListBoxEntities.Items.Cast<EntityNameViewModel>()
				.OrderByDescending(e => ControlData.SelectedEntityNames.Contains(e.LogicalName))
				.ThenBy(e => e).ToArray());
		}
	}

	public class ConnectionManager : IConnectionManager<IDisposableOrgSvc>
	{
		private readonly IOrganizationService service;

		public ConnectionManager(IOrganizationService service)
		{
			this.service = service;
		}

		public IDisposableOrgSvc Get(string connectionString = null)
		{
			return new DisposableOrgSvc(service);
		}
	}

	public class DisposableOrgSvc : DisposableOrgSvcBase
	{
		public DisposableOrgSvc(IOrganizationService innerService) : base(innerService)
		{ }

		public override void Dispose()
		{ }
	}

	public class MetadataCacheManager : MetadataCacheManagerBase
	{
		private static MetadataCacheArray cacheArray = new MetadataCacheArray();

		protected override MetadataCacheArray GetCacheArray()
		{
			return cacheArray;
		}

		public static void Clear()
		{
			cacheArray = new MetadataCacheArray();
		}
	}
}
