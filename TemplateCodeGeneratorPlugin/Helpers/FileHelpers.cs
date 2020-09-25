#region Imports

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Yagasoft.Libraries.Common;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings;
using Yagasoft.TemplateCodeGeneratorPlugin.Model.Settings.File;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Helpers
{
	public class FileHelper
	{
		private readonly PluginSettings pluginSettings;
		private readonly Action saveCallback;

		public FileHelper(PluginSettings pluginSettings, Action saveCallback)
		{
			this.pluginSettings = pluginSettings;
			this.saveCallback = saveCallback;
		}

		public void SaveFile(string title, string fileTypeName, string fileExtension, string defaultFileName,
			SavedFileType savedFileType, string textToSave, bool isForceDialogue = false)
		{
			var saveDialogue =
				new SaveFileDialog
				{
					Title = title,
					OverwritePrompt = true,
					Filter = $"{fileTypeName} files (*.{fileExtension})|*.{fileExtension}",
				};

			var anchorPath = pluginSettings.LatestPath ?? "";

			if (!pluginSettings.SavedFiles.TryGetValue(anchorPath, out var savedFileGroup))
			{
				savedFileGroup = new SavedFileGroup();
				pluginSettings.SavedFiles[anchorPath] = savedFileGroup;
			}

			if (!savedFileGroup.SavedFiles.TryGetValue(savedFileType, out var savedFile))
			{
				savedFile = savedFileGroup.SavedFiles[savedFileType] = new SavedFile();
			}

			if (savedFile.Path.IsFilled())
			{
				saveDialogue.FileName = savedFile.Path;
				saveDialogue.InitialDirectory = savedFile.Folder;
			}
			else
			{
				saveDialogue.FileName = defaultFileName;
				saveDialogue.InitialDirectory = anchorPath.IsEmpty() ? null : Path.GetDirectoryName(anchorPath);
			}

			if (isForceDialogue || savedFile.Path.IsEmpty())
			{
				var result = saveDialogue.ShowDialog();

				if (result != DialogResult.OK || saveDialogue.FileName.IsEmpty())
				{
					return;
				}

				if (!Path.HasExtension(saveDialogue.FileName))
				{
					saveDialogue.FileName += $".{fileExtension}";
				}
			}

			using (var stream = (FileStream)saveDialogue.OpenFile())
			{
				var array = Encoding.UTF8.GetBytes(textToSave);
				stream.Write(array, 0, array.Length);
			}

			savedFile.Folder = Path.GetDirectoryName(saveDialogue.FileName);
			savedFile.File = Path.GetFileName(saveDialogue.FileName);

			if (savedFileType == SavedFileType.Settings && savedFile.Path != anchorPath)
			{
				anchorPath = savedFile.Path;
				pluginSettings.LatestPath = anchorPath;
				pluginSettings.SavedFiles[anchorPath] = savedFileGroup;
			}

			SettingsManager.Instance.Save(typeof(TemplateCodeGeneratorPlugin), pluginSettings);
			saveCallback();
		}

		public string LoadFile(string title, string fileTypeName, string fileExtension, SavedFileType savedFileType)
		{
			var openDialogue =
				new OpenFileDialog
				{
					Title = title,
					Filter = $"{fileTypeName} files (*.{fileExtension})|*.{fileExtension}",
				};

			var anchorPath = pluginSettings.LatestPath ?? "";

			if (!pluginSettings.SavedFiles.TryGetValue(anchorPath, out var savedFileGroup))
			{
				savedFileGroup = new SavedFileGroup();
				pluginSettings.SavedFiles[anchorPath] = savedFileGroup;
			}

			if (!savedFileGroup.SavedFiles.TryGetValue(savedFileType, out var savedFile))
			{
				savedFile = savedFileGroup.SavedFiles[savedFileType] = new SavedFile();
			}

			if (savedFile.Path.IsFilled())
			{
				openDialogue.FileName = savedFile.Path;
				openDialogue.InitialDirectory = savedFile.Folder;
			}
			else
			{
				openDialogue.InitialDirectory = anchorPath.IsEmpty() ? null : Path.GetDirectoryName(anchorPath);
			}

			var result = openDialogue.ShowDialog();

			if (result != DialogResult.OK || openDialogue.FileName.IsEmpty())
			{
				return null;
			}

			string text;

			using (var reader = new StreamReader(openDialogue.FileName))
			{
				text = reader.ReadToEnd();
			}

			savedFile.Folder = Path.GetDirectoryName(openDialogue.FileName);
			savedFile.File = Path.GetFileName(openDialogue.FileName);
			SettingsManager.Instance.Save(typeof(TemplateCodeGeneratorPlugin), pluginSettings);

			return text;
		}

		public SavedFileGroup GetSavedFileGroup()
		{
			var isExists = pluginSettings.SavedFiles.TryGetValue(pluginSettings.LatestPath ?? "", out var savedFileGroup)
				&& File.Exists(pluginSettings.LatestPath);
			return isExists ? savedFileGroup : null;
		}

		public SavedFile GetSavedFileInfo(SavedFileType fileType)
		{
			SavedFile savedFile = null;
			var isExists = pluginSettings.SavedFiles.TryGetValue(pluginSettings.LatestPath ?? "", out var savedFileGroup)
				&& savedFileGroup.SavedFiles.TryGetValue(fileType, out savedFile)
				&& File.Exists(savedFile.Path);
			return isExists ? savedFile : null;
		}
	}
}
