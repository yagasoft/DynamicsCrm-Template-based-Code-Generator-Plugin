using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace Yagasoft.TemplateCodeGeneratorPlugin.Model.ViewModels
{
    public class TemplateViewModel
    {
	    public bool T4Saved
	    {
		    get => t4Saved;
		    set
		    {
			    t4Saved = value;
			    ButtonSaveT4.Enabled = !value;
		    }
	    }

	    public Button ButtonSaveT4 { get; set; }

	    public bool CsSaved
	    {
		    get => csSaved;
		    set
		    {
			    csSaved = value;
			    ButtonSaveCs.Enabled = !value;
		    }
	    }

	    public Button ButtonSaveCs { get; set; }

	    public Scintilla CodeEditorT4 { get; set; }
	    public Scintilla CodeEditorCs { get; set; }

	    private bool t4Saved;
	    private bool csSaved;
    }
}
