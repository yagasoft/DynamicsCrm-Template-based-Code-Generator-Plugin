﻿namespace Yagasoft.TemplateCodeGeneratorPlugin.Model
{
	public static class Constants
	{
		public const string AppName = "Dynamics Template-based Code Generator";
		public const string AppId = "xrmtoolbox-code-gen-plugin";
		public const string AppVersion = "2.3.1.1";

		public const string SettingsVersion = "2.0.0.1";

		public const string MinTemplateVersion = "2.0.0.1";
		public const string LatestTemplateVersion = "2.3.1.1";

		public const string MetaCacheMemKey = "ys_CrmGen_Meta_639156";
		public const string ConnCacheMemKey = "ys_CrmGen_Conn_185599";

		public const string ReleaseNotes = AppName + "\r\n" +
			"    v" + AppVersion + "\r\n" +
			"~~~~~~~~~~\r\n" +
			@"
  >> IMPORTANT VERSION UPDATE <<
  - FIX: If you are using an unmodified template, please reset (button) the template to get the latest version; otherwise, backup yours, reset, and then using CodeCompare or a similar tool, check the updates.
  - REQUEST: Please report issues and improvement suggestions on the generator's GitHub repository. Use the 'Help' menu above to access the page.
  
  * 2.3.1.1
  Improved: filtering feature now works over logical and display names, and renames
  Updated: latest custom libraries
  Fixed: template issues
  * 2.2.2.1
  Fixed: mishandling errors
  * 2.2.1.1
  Added: Filter Details window row filtering
  Fixed: generated code 'labels' syntax error
  * 2.1.0.1
  Added: recent settings list (load history)
  Added: reset option for the template text
  Added: toast notification for clearer status
  Improved: load and save logic
  Fixed: fixed cancel button
  Fixed: issues
  * 2.0.0.1
  Added: all missing features from VS extension (click on 'Quick Guide' for more info), except Contracts
  Added: keep track of paths (settings, template, and code) used in previous sessions and the links between them
  Fixed: layout issues
" +
			"\r\n~~~~~~~~~~";
	}
}
