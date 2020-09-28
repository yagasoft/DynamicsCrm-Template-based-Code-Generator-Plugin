namespace Yagasoft.TemplateCodeGeneratorPlugin.Model
{
	public static class Constants
	{
		public const string AppName = "Dynamics Template-based Code Generator";
		public const string AppId = "xrmtoolbox-code-gen-plugin";
		public const string AppVersion = "2.1.0.1";

		public const string SettingsVersion = "2.0.0.1";

		public const string MinTemplateVersion = "2.0.0.1";
		public const string LatestTemplateVersion = "2.0.0.1";

		public const string MetaCacheMemKey = "ys_CrmGen_Meta_639156";
		public const string ConnCacheMemKey = "ys_CrmGen_Conn_185599";

		public const string ReleaseNotes = AppName + "\r\n" +
			"    v" + AppVersion + "\r\n" +
			"~~~~~~~~~~\r\n" +
			@"
+ Added: all missing features from VS extension (click on 'Quick Guide' for more info), except Contracts.

+ Added: keep track of paths (settings, template, and code) used in previous sessions and the links between them.

+ Added: recent settings list (load history).

+ Added: reset option for the template text.

+ Added: toast notification for clearer status.

+ Improved: load and save logic.

+ Fixed: layout issues.

+ Fixed: fixed cancel button.

+ Fixed: issues.

" +
			"~~~~~~~~~~";
	}
}
