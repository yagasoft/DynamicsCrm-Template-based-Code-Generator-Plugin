namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	partial class DeletableMenuItem
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
			this.buttonDelete = new System.Windows.Forms.Button();
			this.labelItem = new System.Windows.Forms.Label();
			this.tableLayoutPanelItem = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelItem.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonDelete
			// 
			this.buttonDelete.AutoSize = true;
			this.buttonDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.buttonDelete.BackColor = System.Drawing.Color.Transparent;
			this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonDelete.FlatAppearance.BorderSize = 0;
			this.buttonDelete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
			this.buttonDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonDelete.Location = new System.Drawing.Point(35, 0);
			this.buttonDelete.Margin = new System.Windows.Forms.Padding(0);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(20, 23);
			this.buttonDelete.TabIndex = 1;
			this.buttonDelete.Text = "X";
			this.buttonDelete.UseVisualStyleBackColor = false;
			this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
			// 
			// labelItem
			// 
			this.labelItem.AutoSize = true;
			this.labelItem.BackColor = System.Drawing.Color.Transparent;
			this.labelItem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelItem.Location = new System.Drawing.Point(0, 0);
			this.labelItem.Margin = new System.Windows.Forms.Padding(0);
			this.labelItem.Name = "labelItem";
			this.labelItem.Size = new System.Drawing.Size(35, 23);
			this.labelItem.TabIndex = 0;
			this.labelItem.Text = "label1";
			this.labelItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelItem.Click += new System.EventHandler(this.labelItem_Click);
			this.labelItem.Paint += new System.Windows.Forms.PaintEventHandler(this.labelItem_Paint);
			this.labelItem.MouseEnter += new System.EventHandler(this.labelItem_MouseEnter);
			this.labelItem.MouseLeave += new System.EventHandler(this.labelItem_MouseLeave);
			// 
			// tableLayoutPanelItem
			// 
			this.tableLayoutPanelItem.AutoSize = true;
			this.tableLayoutPanelItem.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelItem.ColumnCount = 2;
			this.tableLayoutPanelItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelItem.Controls.Add(this.buttonDelete, 1, 0);
			this.tableLayoutPanelItem.Controls.Add(this.labelItem, 0, 0);
			this.tableLayoutPanelItem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelItem.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelItem.Name = "tableLayoutPanelItem";
			this.tableLayoutPanelItem.RowCount = 1;
			this.tableLayoutPanelItem.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelItem.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelItem.Size = new System.Drawing.Size(55, 23);
			this.tableLayoutPanelItem.TabIndex = 2;
			// 
			// DeletableMenuItem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanelItem);
			this.Name = "DeletableMenuItem";
			this.Size = new System.Drawing.Size(55, 23);
			this.Load += new System.EventHandler(this.DeletableMenuItem_Load);
			this.tableLayoutPanelItem.ResumeLayout(false);
			this.tableLayoutPanelItem.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button buttonDelete;
		private System.Windows.Forms.Label labelItem;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelItem;
	}
}
