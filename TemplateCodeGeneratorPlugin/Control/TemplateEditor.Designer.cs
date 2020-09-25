namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	partial class TemplateEditor
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelTemplatePath = new System.Windows.Forms.Label();
			this.labelTemplate = new System.Windows.Forms.Label();
			this.buttonSaveT4 = new System.Windows.Forms.Button();
			this.buttonSaveAsT4 = new System.Windows.Forms.Button();
			this.buttonLoadT4 = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.labelCodePath = new System.Windows.Forms.Label();
			this.labelCode = new System.Windows.Forms.Label();
			this.buttonSaveCs = new System.Windows.Forms.Button();
			this.buttonSaveAsCs = new System.Windows.Forms.Button();
			this.panelCodeEditorT4 = new System.Windows.Forms.Panel();
			this.panelCodeEditorCs = new System.Windows.Forms.Panel();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.panel2, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.panelCodeEditorT4, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.panelCodeEditorCs, 1, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(879, 587);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.labelTemplatePath);
			this.panel1.Controls.Add(this.labelTemplate);
			this.panel1.Controls.Add(this.buttonSaveT4);
			this.panel1.Controls.Add(this.buttonSaveAsT4);
			this.panel1.Controls.Add(this.buttonLoadT4);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(433, 41);
			this.panel1.TabIndex = 0;
			// 
			// labelTemplatePath
			// 
			this.labelTemplatePath.AutoEllipsis = true;
			this.labelTemplatePath.AutoSize = true;
			this.labelTemplatePath.Location = new System.Drawing.Point(3, 23);
			this.labelTemplatePath.Name = "labelTemplatePath";
			this.labelTemplatePath.Size = new System.Drawing.Size(83, 13);
			this.labelTemplatePath.TabIndex = 19;
			this.labelTemplatePath.Text = "<template-path>";
			this.labelTemplatePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			// buttonSaveT4
			// 
			this.buttonSaveT4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveT4.Enabled = false;
			this.buttonSaveT4.Location = new System.Drawing.Point(355, 0);
			this.buttonSaveT4.Name = "buttonSaveT4";
			this.buttonSaveT4.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveT4.TabIndex = 17;
			this.buttonSaveT4.Text = "Save";
			this.buttonSaveT4.UseVisualStyleBackColor = true;
			this.buttonSaveT4.Click += new System.EventHandler(this.buttonSaveT4_Click);
			// 
			// buttonSaveAsT4
			// 
			this.buttonSaveAsT4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveAsT4.Location = new System.Drawing.Point(274, 0);
			this.buttonSaveAsT4.Name = "buttonSaveAsT4";
			this.buttonSaveAsT4.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveAsT4.TabIndex = 18;
			this.buttonSaveAsT4.Text = "Save As";
			this.buttonSaveAsT4.UseVisualStyleBackColor = true;
			this.buttonSaveAsT4.Click += new System.EventHandler(this.buttonSaveAsT4_Click);
			// 
			// buttonLoadT4
			// 
			this.buttonLoadT4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoadT4.Location = new System.Drawing.Point(193, 0);
			this.buttonLoadT4.Name = "buttonLoadT4";
			this.buttonLoadT4.Size = new System.Drawing.Size(75, 20);
			this.buttonLoadT4.TabIndex = 16;
			this.buttonLoadT4.Text = "Load";
			this.buttonLoadT4.UseVisualStyleBackColor = true;
			this.buttonLoadT4.Click += new System.EventHandler(this.buttonLoadT4_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.labelCodePath);
			this.panel2.Controls.Add(this.labelCode);
			this.panel2.Controls.Add(this.buttonSaveCs);
			this.panel2.Controls.Add(this.buttonSaveAsCs);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(442, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(434, 41);
			this.panel2.TabIndex = 2;
			// 
			// labelCodePath
			// 
			this.labelCodePath.AutoEllipsis = true;
			this.labelCodePath.AutoSize = true;
			this.labelCodePath.Location = new System.Drawing.Point(3, 23);
			this.labelCodePath.Name = "labelCodePath";
			this.labelCodePath.Size = new System.Drawing.Size(67, 13);
			this.labelCodePath.TabIndex = 20;
			this.labelCodePath.Text = "<code-path>";
			this.labelCodePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			// buttonSaveCs
			// 
			this.buttonSaveCs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveCs.Enabled = false;
			this.buttonSaveCs.Location = new System.Drawing.Point(356, 0);
			this.buttonSaveCs.Name = "buttonSaveCs";
			this.buttonSaveCs.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveCs.TabIndex = 18;
			this.buttonSaveCs.Text = "Save";
			this.buttonSaveCs.UseVisualStyleBackColor = true;
			this.buttonSaveCs.Click += new System.EventHandler(this.buttonSaveCs_Click);
			// 
			// buttonSaveAsCs
			// 
			this.buttonSaveAsCs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveAsCs.Location = new System.Drawing.Point(275, 0);
			this.buttonSaveAsCs.Name = "buttonSaveAsCs";
			this.buttonSaveAsCs.Size = new System.Drawing.Size(75, 20);
			this.buttonSaveAsCs.TabIndex = 19;
			this.buttonSaveAsCs.Text = "Save As";
			this.buttonSaveAsCs.UseVisualStyleBackColor = true;
			this.buttonSaveAsCs.Click += new System.EventHandler(this.buttonSaveAsCs_Click);
			// 
			// panelCodeEditorT4
			// 
			this.panelCodeEditorT4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCodeEditorT4.Location = new System.Drawing.Point(3, 50);
			this.panelCodeEditorT4.Name = "panelCodeEditorT4";
			this.panelCodeEditorT4.Size = new System.Drawing.Size(433, 534);
			this.panelCodeEditorT4.TabIndex = 20;
			// 
			// panelCodeEditorCs
			// 
			this.panelCodeEditorCs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCodeEditorCs.Location = new System.Drawing.Point(442, 50);
			this.panelCodeEditorCs.Name = "panelCodeEditorCs";
			this.panelCodeEditorCs.Size = new System.Drawing.Size(434, 534);
			this.panelCodeEditorCs.TabIndex = 21;
			// 
			// TemplateEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel2);
			this.Name = "TemplateEditor";
			this.Size = new System.Drawing.Size(879, 587);
			this.Load += new System.EventHandler(this.TemplateEditor_Load);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Panel panelCodeEditorCs;
		private System.Windows.Forms.Panel panelCodeEditorT4;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelTemplatePath;
		private System.Windows.Forms.Label labelTemplate;
		private System.Windows.Forms.Button buttonSaveT4;
		private System.Windows.Forms.Button buttonSaveAsT4;
		private System.Windows.Forms.Button buttonLoadT4;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label labelCodePath;
		private System.Windows.Forms.Label labelCode;
		private System.Windows.Forms.Button buttonSaveCs;
		private System.Windows.Forms.Button buttonSaveAsCs;
	}
}
