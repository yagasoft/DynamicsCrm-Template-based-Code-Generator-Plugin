using System.Collections.Generic;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings.File;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings
{
	public enum SavedFileType
	{
		Settings,
		Template,
		Code
	}

    public class PluginSettings
    {
	    public SerializableDictionary<string, SavedFileGroup> SavedFiles = new SerializableDictionary<string, SavedFileGroup>();
	    public string LatestPath;
	    public string ReleaseNotesShownVersion;
    }
}
