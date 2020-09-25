using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using Yagasoft.Libraries.Common;
using Yagasoft.TemplateCodeGeneratorPlugin.Helpers;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModels;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public partial class TemplateEditor : UserControl
	{
		public Label LabelTemplatePath => labelTemplatePath;
		public Label LabelCodePath => labelCodePath;

		private readonly TemplateViewModel templateViewModel;
		private readonly FileHelper fileHelper;
		private readonly WorkerHelper workerHelper;

		public TemplateEditor(TemplateViewModel templateViewModel, FileHelper fileHelper, Form parentForm, WorkerHelper workerHelper)
		{
			this.templateViewModel = templateViewModel;
			this.fileHelper = fileHelper;
			this.workerHelper = workerHelper;

			InitializeComponent();

			templateViewModel.ButtonSaveT4 = buttonSaveT4;
			templateViewModel.ButtonSaveCs = buttonSaveCs;

			templateViewModel.CodeEditorT4 = new CodeEditor(parentForm);
			templateViewModel.CodeEditorT4.TextChanged += (o, args) => templateViewModel.T4Saved = false;
			templateViewModel.CodeEditorCs = new CodeEditor(parentForm);
			templateViewModel.CodeEditorCs.TextChanged += (o, args) => templateViewModel.CsSaved = false;
		}

		private void TemplateEditor_Load(object sender, EventArgs e)
		{
			panelCodeEditorT4.Controls.Add(templateViewModel.CodeEditorT4);
			templateViewModel.CodeEditorT4.Dock = DockStyle.Fill;
			panelCodeEditorCs.Controls.Add(templateViewModel.CodeEditorCs);
			templateViewModel.CodeEditorCs.Dock = DockStyle.Fill;
		}

		private void buttonLoadT4_Click(object sender, EventArgs e)
		{
			var text = fileHelper.LoadFile("Load T4 Template ...", "T4 Template", "tt", SavedFileType.Template);

			if (text.IsFilled())
			{
				templateViewModel.CodeEditorT4.Text = text;
			}
		}

		private void buttonSaveT4_Click(object sender, EventArgs e)
		{
			fileHelper.SaveFile("Save T4 Template ...", "T4 Template", "tt", "CrmSchema.tt",
				SavedFileType.Template, templateViewModel.CodeEditorT4.Text);
			templateViewModel.T4Saved = true;
		}

		private void buttonSaveAsT4_Click(object sender, EventArgs e)
		{
			fileHelper.SaveFile("Save T4 Template ...", "T4 Template", "tt", "CrmSchema.tt",
				SavedFileType.Template, templateViewModel.CodeEditorT4.Text, true);
			templateViewModel.T4Saved = true;
		}

		private void buttonSaveCs_Click(object sender, EventArgs e)
		{
			fileHelper.SaveFile("Save Generated Code ...", "CS", "cs", "CrmSchema.cs",
				SavedFileType.Code, templateViewModel.CodeEditorCs.Text);
			templateViewModel.CsSaved = true;
		}

		private void buttonSaveAsCs_Click(object sender, EventArgs e)
		{
			fileHelper.SaveFile("Save Generated Code ...", "CS", "cs", "CrmSchema.cs",
				SavedFileType.Code, templateViewModel.CodeEditorCs.Text, true);
			templateViewModel.CsSaved = true;
		}
	}
}
