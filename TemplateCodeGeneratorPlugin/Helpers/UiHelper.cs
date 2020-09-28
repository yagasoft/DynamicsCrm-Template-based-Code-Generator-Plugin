#region Imports

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Helpers
{
	public class UiHelper
	{
		private readonly Panel panel;
		private readonly Label label;
		private readonly Action<Action> invoker;
		private Point originalLocation = Point.Empty;
		private int workingIndex;

		public UiHelper(Panel panel, Label label, Action<Action> invoker)
		{
			this.panel = panel;
			this.label = label;
			this.invoker = invoker;
		}

		public void ShowToast(string message, bool isShow = true, int? workerIndex = null)
		{
			lock (this)
			{
				if (workerIndex == null)
				{
					workerIndex = new Random().Next();
				}

				if (isShow && workingIndex != workerIndex)
				{
					workingIndex = workerIndex.GetValueOrDefault();
					invoker(() => label.Text = message);
				}

				if (!isShow && workingIndex != workerIndex)
				{
					return;
				}
			}

			const int tick = 10;
			Timer locationTimer = null;
			invoker(() => locationTimer = new Timer());

			if (locationTimer == null)
			{
				return;
			}

			var distance = panel.Size.Width + 10;
			var step = Math.Max(1, distance / (500 / 3 / tick));

			async void ExecuteDelayed(Action action)
			{
				await Task.Delay(3000);
				action();
			}

			var targetLocation = Point.Empty;

			void ChangeLocation(object sender, EventArgs e)
			{
				if (originalLocation == Point.Empty)
				{
					originalLocation = new Point(panel.Location.X + distance, panel.Location.Y);
				}

				if (targetLocation == Point.Empty)
				{
					if (isShow)
					{
						invoker(
							() =>
							{
								if (workerIndex == workingIndex)
								{
									panel.Location = originalLocation;
									panel.Show();
								}
							});
					}

					targetLocation = isShow ? new Point(originalLocation.X - distance, originalLocation.Y) : originalLocation;
				}

				if (workerIndex == workingIndex)
				{
					invoker(() => panel.Location = new Point(panel.Location.X - (isShow ? step : -step), originalLocation.Y));
				}
				else
				{
					locationTimer.Stop();
					return;
				}

				if ((isShow && panel.Location.X <= targetLocation.X)
					|| (!isShow && (panel.Location.X >= targetLocation.X)))
				{
					locationTimer.Stop();

					if (isShow)
					{
						ExecuteDelayed(() => ShowToast(message, false, workerIndex));
					}
				}
			}

			invoker(
				() =>
				{
					locationTimer.Interval = tick;
					locationTimer.Tick += ChangeLocation;
					locationTimer.Start();
				});
		}
	}
}
