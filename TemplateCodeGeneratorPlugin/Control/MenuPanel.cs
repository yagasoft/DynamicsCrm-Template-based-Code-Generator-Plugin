using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public partial class MenuPanel : UserControl
	{
		public delegate void DeletableMenuItemClickedEventHandler(object sender, MenuItemClickedEventArgs e);

		public event DeletableMenuItemClickedEventHandler ItemClicked;
		public event DeletableMenuItemClickedEventHandler ItemDeleted;

		protected virtual void OnClicked(string label, object value)
		{
			ItemClicked?.Invoke(this, new MenuItemClickedEventArgs(label, value));
		}


		protected virtual void OnDeleted(string label, object value)
		{
			ItemDeleted?.Invoke(this, new MenuItemClickedEventArgs(label, value));
		}

		private readonly IDictionary<object, string> items;
		private readonly object highlight;

		public MenuPanel(IDictionary<object, string> items, object highlight = null)
		{
			this.items = items;
			this.highlight = highlight;
			InitializeComponent();
		}

		private void MenuPanel_Load(object sender, EventArgs e)
		{
			var controls =
				items
					.Select(
						i =>
						{
							var control = new DeletableMenuItem(i.Key, i.Value, highlight == i.Key);
							control.ItemClicked += (s, a) => OnClicked(a?.Label, a?.Value);
							control.ItemDeleted +=
								(s, a) =>
								{
									OnDeleted(a?.Label, a?.Value);
									flowLayoutPanelItems.Controls.Remove(control);
									items.Remove(i.Key);
								};
							control.Dock = DockStyle.Fill;
							return control;
						}).Cast<System.Windows.Forms.Control>().ToArray();

			flowLayoutPanelItems.Controls.AddRange(controls);
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
			ParentForm?.Close();
		}
	}
}
