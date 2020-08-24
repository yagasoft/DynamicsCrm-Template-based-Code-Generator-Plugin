#region Imports

using System;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;
using ScintillaNET.Demo.Utils;

#endregion

namespace Yagasoft.TemplateCodeGeneratorPlugin.Control
{
	public class CodeEditor : Scintilla
	{
		public CodeEditor(Form parentForm)
		{
			// BASIC CONFIG
			Dock = DockStyle.Fill;

			// INITIAL VIEW CONFIG
			WrapMode = WrapMode.None;
			IndentationGuides = IndentView.LookBoth;

			// STYLING
			InitColors();
			InitSyntaxColoring();

			// NUMBER MARGIN
			InitNumberMargin();

			// BOOKMARK MARGIN
			InitBookmarkMargin();

			// CODE FOLDING MARGIN
			InitCodeFolding();

			// INIT HOTKEYS
			InitHotkeys(parentForm);
		}

		private void InitColors()
		{
			SetSelectionBackColor(true, IntToColor(0x114D9C));
			CaretForeColor = Color.White;
		}

		private void InitHotkeys(Form parentForm)
		{
			// register the hotkeys with the form
			HotKeyManager.AddHotKey(parentForm, ZoomIn, Keys.Oemplus, true);
			HotKeyManager.AddHotKey(parentForm, ZoomOut, Keys.OemMinus, true);
			HotKeyManager.AddHotKey(parentForm, ZoomDefault, Keys.D0, true);
		}

		private void InitSyntaxColoring()
		{
			// Configure the default style
			StyleResetDefault();
			Styles[Style.Default].Font = "Consolas";
			Styles[Style.Default].Size = 10;
			Styles[Style.Default].BackColor = IntToColor(0x212121);
			Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
			StyleClearAll();

			// Configure the CPP (C#) lexer styles
			Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
			Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
			Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
			Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
			Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
			Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
			Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
			Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
			Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
			Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
			Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
			Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
			Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
			Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
			Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
			Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

			Lexer = Lexer.Cpp;

			SetKeywords(0,
				"class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
			SetKeywords(1,
				"void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");
		}

		#region Numbers, Bookmarks, Code Folding

		/// <summary>
		///     the background color of the text area
		/// </summary>
		private const int BACK_COLOR = 0x2A211C;

		/// <summary>
		///     default text color of the text area
		/// </summary>
		private const int FORE_COLOR = 0xB7B7B7;

		/// <summary>
		///     change this to whatever margin you want the line numbers to show in
		/// </summary>
		private const int NUMBER_MARGIN = 1;

		/// <summary>
		///     change this to whatever margin you want the bookmarks/breakpoints to show in
		/// </summary>
		private const int BOOKMARK_MARGIN = 2;

		private const int BOOKMARK_MARKER = 2;

		/// <summary>
		///     change this to whatever margin you want the code folding tree (+/-) to show in
		/// </summary>
		private const int FOLDING_MARGIN = 3;

		/// <summary>
		///     set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
		/// </summary>
		private const bool CODEFOLDING_CIRCULAR = true;

		private void InitNumberMargin()
		{
			Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
			Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
			Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
			Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

			var nums = Margins[NUMBER_MARGIN];
			nums.Width = 30;
			nums.Type = MarginType.Number;
			nums.Sensitive = true;
			nums.Mask = 0;

			MarginClick += CodeEditor_MarginClick;
		}

		private void InitBookmarkMargin()
		{
			//CodeEditor.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

			var margin = Margins[BOOKMARK_MARGIN];
			margin.Width = 20;
			margin.Sensitive = true;
			margin.Type = MarginType.Symbol;
			margin.Mask = (1 << BOOKMARK_MARKER);
			//margin.Cursor = MarginCursor.Arrow;

			var marker = Markers[BOOKMARK_MARKER];
			marker.Symbol = MarkerSymbol.Circle;
			marker.SetBackColor(IntToColor(0xFF003B));
			marker.SetForeColor(IntToColor(0x000000));
			marker.SetAlpha(100);
		}

		private void InitCodeFolding()
		{
			SetFoldMarginColor(true, IntToColor(BACK_COLOR));
			SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

			// Enable code folding
			SetProperty("fold", "1");
			SetProperty("fold.compact", "1");

			// Configure a margin to display folding symbols
			Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
			Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
			Margins[FOLDING_MARGIN].Sensitive = true;
			Margins[FOLDING_MARGIN].Width = 20;

			// Set colors for all folding markers
			for (int i = 25; i <= 31; i++)
			{
				Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
				Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
			}

			// Configure folding markers with respective symbols
			Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
			Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
			Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR
				? MarkerSymbol.CircleMinusConnected
				: MarkerSymbol.BoxMinusConnected;
			Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// Enable automatic folding
			AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
		}

		private void CodeEditor_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (e.Margin == BOOKMARK_MARGIN)
			{
				// Do we have a marker for this line?
				const uint mask = (1 << BOOKMARK_MARKER);
				var line = Lines[LineFromPosition(e.Position)];
				if ((line.MarkerGet() & mask) > 0)
				{
					// Remove existing bookmark
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// Add bookmark
					line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}

		#endregion

		#region Uppercase / Lowercase

		private void Lowercase()
		{
			// save the selection
			int start = SelectionStart;
			int end = SelectionEnd;

			// modify the selected text
			ReplaceSelection(GetTextRange(start, end - start).ToLower());

			// preserve the original selection
			SetSelection(start, end);
		}

		private void Uppercase()
		{
			// save the selection
			int start = SelectionStart;
			int end = SelectionEnd;

			// modify the selected text
			ReplaceSelection(GetTextRange(start, end - start).ToUpper());

			// preserve the original selection
			SetSelection(start, end);
		}

		#endregion

		#region Indent / Outdent

		private void Indent()
		{
			// we use this hack to send "Shift+Tab" to scintilla, since there is no known API to indent,
			// although the indentation function exists. Pressing TAB with the editor focused confirms this.
			GenerateKeystrokes("{TAB}");
		}

		private void Outdent()
		{
			// we use this hack to send "Shift+Tab" to scintilla, since there is no known API to outdent,
			// although the indentation function exists. Pressing Shift+Tab with the editor focused confirms this.
			GenerateKeystrokes("+{TAB}");
		}

		private void GenerateKeystrokes(string keys)
		{
			HotKeyManager.Enable = false;
			Focus();
			SendKeys.Send(keys);
			HotKeyManager.Enable = true;
		}

		#endregion

		#region Zoom

		private void ZoomDefault()
		{
			Zoom = 0;
		}

		#endregion

		#region Utils

		public static Color IntToColor(int rgb)
		{
			return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
		}

		public void InvokeIfNeeded(Action action)
		{
			if (InvokeRequired)
			{
				BeginInvoke(action);
			}
			else
			{
				action.Invoke();
			}
		}

		#endregion
	}
}
