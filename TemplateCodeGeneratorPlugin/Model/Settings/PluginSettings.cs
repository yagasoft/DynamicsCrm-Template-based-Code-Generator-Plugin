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
	    public SerializableDictionary<SavedFileType, SavedFile> SavedFiles = new SerializableDictionary<SavedFileType, SavedFile>();
    }
}
