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
		private readonly Action<SavedFileType, SavedFileGroup> saveCallback;

		public FileHelper(PluginSettings pluginSettings, Action<SavedFileType, SavedFileGroup> saveCallback)
		{
			this.pluginSettings = pluginSettings;
			this.saveCallback = saveCallback;
		}

		public SavedFileGroup SaveFile(string title, string fileTypeName, string fileExtension, string defaultFileName,
			SavedFileType savedFileType, string textToSave, bool isForceDialogue = false)
		{
			var saveDialogue =
				new SaveFileDialog
				{
					Title = title,
					OverwritePrompt = true,
					Filter = $"{fileTypeName} files (*.{fileExtension})|*.{fileExtension}",
				};

			SavedFile savedFile = null;

			SavedFileGroup savedFileGroup = null;

			if (pluginSettings.LatestPath.IsFilled()
				&& pluginSettings.SavedFiles.TryGetValue(pluginSettings.LatestPath, out savedFileGroup)
				&& savedFileGroup.SavedFiles.TryGetValue(savedFileType, out savedFile)
				&& savedFile.Path.IsFilled())
			{
				saveDialogue.FileName = savedFile.Path;
				saveDialogue.InitialDirectory = savedFile.Folder;
			}
			else
			{
				saveDialogue.FileName = defaultFileName;
				saveDialogue.InitialDirectory = pluginSettings.LatestPath.IsEmpty() ? null : Path.GetDirectoryName(pluginSettings.LatestPath);
			}

			if (isForceDialogue || savedFile?.Path.IsEmpty() != false)
			{
				var result = saveDialogue.ShowDialog();

				if (result != DialogResult.OK || saveDialogue.FileName.IsEmpty())
				{
					return null;
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

			savedFile =
				new SavedFile
				{
					Folder = Path.GetDirectoryName(saveDialogue.FileName),
					File = Path.GetFileName(saveDialogue.FileName)
				};

			if (savedFileType == SavedFileType.Settings)
			{
				pluginSettings.LatestPath = savedFile.Path;
			}

			if (pluginSettings.LatestPath.IsFilled() && !pluginSettings.SavedFiles.TryGetValue(pluginSettings.LatestPath, out savedFileGroup))
			{
				savedFileGroup = new SavedFileGroup();
				pluginSettings.SavedFiles[pluginSettings.LatestPath] = savedFileGroup;
			}

			if (savedFileGroup != null)
			{
				savedFileGroup.SavedFiles[savedFileType] = savedFile;
			}

			SettingsManager.Instance.Save(typeof(TemplateCodeGeneratorPlugin), pluginSettings);
			saveCallback(savedFileType, savedFileGroup);

			return savedFileGroup;
		}

		public string LoadFile(string title, string fileTypeName, string fileExtension, SavedFileType savedFileType)
		{
			var openDialogue =
				new OpenFileDialog
				{
					Title = title,
					Filter = $"{fileTypeName} files (*.{fileExtension})|*.{fileExtension}",
				};

			SavedFileGroup savedFileGroup = null;

			if (pluginSettings.LatestPath.IsFilled()
				&& pluginSettings.SavedFiles.TryGetValue(pluginSettings.LatestPath, out savedFileGroup)
				&& savedFileGroup.SavedFiles.TryGetValue(savedFileType, out var savedFile)
				&& savedFile.Path.IsFilled())
			{
				openDialogue.FileName = savedFile.Path;
				openDialogue.InitialDirectory = savedFile.Folder;
			}
			else
			{
				openDialogue.InitialDirectory = pluginSettings.LatestPath.IsEmpty() ? null : Path.GetDirectoryName(pluginSettings.LatestPath);
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

			var folder = Path.GetDirectoryName(openDialogue.FileName);
			var file = Path.GetFileName(openDialogue.FileName);
			savedFile =
				new SavedFile
				{
					Folder = folder,
					File = file
				};

			if (savedFileType == SavedFileType.Settings)
			{
				pluginSettings.LatestPath = savedFile.Path;
			}

			if (pluginSettings.LatestPath.IsFilled() && !pluginSettings.SavedFiles.TryGetValue(pluginSettings.LatestPath, out savedFileGroup))
			{
				savedFileGroup = new SavedFileGroup();
				pluginSettings.SavedFiles[pluginSettings.LatestPath] = savedFileGroup;
			}

			if (savedFileGroup != null)
			{
				savedFileGroup.SavedFiles[savedFileType] = savedFile;
			}

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
