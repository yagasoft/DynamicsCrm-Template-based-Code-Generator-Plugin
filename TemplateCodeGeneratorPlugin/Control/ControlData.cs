#region Imports

using System.Collections.Generic;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModel;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public static class ControlData
	{
		public static List<EntityNameViewModel> EntityNames = new List<EntityNameViewModel>();
		public static List<string> SelectedEntityNames = new List<string>();
	}

	internal class RetrieveResult
	{
		internal IEnumerable<EntityNameViewModel> EntityNames { get; set; }
	}
}
