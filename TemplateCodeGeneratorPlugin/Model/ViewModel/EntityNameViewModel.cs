using System;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModel
{
	public class EntityNameViewModel : IComparable<EntityNameViewModel>
	{
		internal string LogicalName { get; set; }
		internal string DisplayName { get; set; }

		public override string ToString()
		{
			return $"{DisplayName} ({LogicalName})";
		}

		public int CompareTo(EntityNameViewModel obj)
		{
			return string.Compare(ToString(), obj.ToString(), StringComparison.Ordinal);
		}
	}
}
