#region Imports

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public partial class DeletableMenuItem : UserControl
	{
		public delegate void DeletableMenuItemClickedEventHandler(object sender, MenuItemClickedEventArgs e);

		public event DeletableMenuItemClickedEventHandler ItemClicked;
		public event DeletableMenuItemClickedEventHandler ItemDeleted;

		protected virtual void OnClicked()
		{
			ItemClicked?.Invoke(this, new MenuItemClickedEventArgs(label, value));
		}

		protected virtual void OnDeleted()
		{
			ItemDeleted?.Invoke(this, new MenuItemClickedEventArgs(label, value));
		}

		public int LabelWidth => labelItem.Width;

		private readonly object value;
		private readonly string label;
		private readonly bool isHighlight;
		private bool isChangeBorder;

		public DeletableMenuItem(object value, string label, bool isHighlight = false)
		{
			this.value = value;
			this.label = label;
			this.isHighlight = isHighlight;

			InitializeComponent();
		}

		private void DeletableMenuItem_Load(object sender, EventArgs e)
		{
			labelItem.Text = label;

			if (isHighlight)
			{
				labelItem.ForeColor = Color.Blue;
			}
		}

		private void labelItem_Click(object sender, EventArgs e)
		{
			OnClicked();
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			OnDeleted();
		}

		private void labelItem_MouseEnter(object sender, EventArgs e)
		{
			labelItem.BackColor = Color.WhiteSmoke;
			isChangeBorder = true;
		}

		private void labelItem_MouseLeave(object sender, EventArgs e)
		{
			labelItem.BackColor = Color.Transparent;
			isChangeBorder = false;
		}

		private void labelItem_Paint(object sender, PaintEventArgs e)
		{
			const int width = 1;
			var colour = Color.DarkGray;

			if (isChangeBorder)
			{
				ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle,
					colour, width, ButtonBorderStyle.Solid,
					colour, width, ButtonBorderStyle.Solid,
					colour, width, ButtonBorderStyle.Solid,
					colour, width, ButtonBorderStyle.Solid);
			}
			else
			{
				ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, BackColor, ButtonBorderStyle.Solid);
			}
		}
	}

	public class MenuItemClickedEventArgs
	{
		public string Label { get; set; }
		public object Value { get; set; }

		public MenuItemClickedEventArgs(string label, object value)
		{
			Label = label;
			Value = value;
		}
	}
}
