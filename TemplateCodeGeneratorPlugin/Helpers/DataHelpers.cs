#region Imports

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModel;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Helpers
{
	public static class DataHelpers
	{
		public static IEnumerable<EntityNameViewModel> RetrieveEntityNames(IOrganizationService service)
		{
			var entityProperties =
				new MetadataPropertiesExpression
				{
					AllProperties = false
				};
			entityProperties.PropertyNames.AddRange("LogicalName", "DisplayName");

			var entityQueryExpression =
				new EntityQueryExpression
				{
					Properties = entityProperties
				};

			var retrieveMetadataChangesRequest =
				new RetrieveMetadataChangesRequest
				{
					Query = entityQueryExpression,
					ClientVersionStamp = null
				};

			return ((RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest))
				.EntityMetadata.Select(e =>
					new EntityNameViewModel
					{
						LogicalName = e.LogicalName,
						DisplayName = e.DisplayName?.UserLocalizedLabel?.Label
					});
		}
	}
}
