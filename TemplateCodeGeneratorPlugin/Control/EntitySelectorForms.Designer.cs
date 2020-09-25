namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	partial class EntitySelectorForms
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
			this.entitySelectorHost = new System.Windows.Forms.Integration.ElementHost();
			this.SuspendLayout();
			// 
			// entitySelectorHost
			// 
			this.entitySelectorHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.entitySelectorHost.Location = new System.Drawing.Point(0, 0);
			this.entitySelectorHost.Name = "entitySelectorHost";
			this.entitySelectorHost.Size = new System.Drawing.Size(757, 488);
			this.entitySelectorHost.TabIndex = 23;
			this.entitySelectorHost.Text = "elementHost1";
			this.entitySelectorHost.Child = null;
			// 
			// EntitySelectorForms
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.entitySelectorHost);
			this.Name = "EntitySelectorForms";
			this.Size = new System.Drawing.Size(757, 488);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Integration.ElementHost entitySelectorHost;
	}
}
