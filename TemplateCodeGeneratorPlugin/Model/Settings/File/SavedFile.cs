using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yagasoft.Libraries.Common;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings.File
{
    public class SavedFile
    {
	    public string Folder;
	    public string File;

	    public string Path => File.IsFilled() ? System.IO.Path.Combine(Folder ?? ".", File) : null;
    }
}
