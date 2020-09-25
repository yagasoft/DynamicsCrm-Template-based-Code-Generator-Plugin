using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yagasoft.Libraries.Common;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings.File
{
    public class SavedFileGroup
    {
	    public SerializableDictionary<SavedFileType, SavedFile> SavedFiles = new SerializableDictionary<SavedFileType, SavedFile>();
    }
}
