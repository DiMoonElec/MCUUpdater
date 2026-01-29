using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MCUUpdaterGUI.UserControls
{
  public class LogRichTextBox : RichTextBox
  {
    public LogRichTextBox()
    {
      ReadOnly = true;
      HideSelection = true;
      ScrollBars = RichTextBoxScrollBars.Vertical;
      BorderStyle = BorderStyle.FixedSingle;
      BackColor = SystemColors.Window;
      Font = new Font("Segoe UI", 9F);

      // Значения по умолчанию
      TimestampColor = Color.DarkGray;
      InfoColor = Color.SteelBlue;
      ErrorColor = Color.DarkRed;
      WarningColor = Color.DarkOrange;
      DebugColor = Color.Gray;
      TimestampFormat = "yyyy-MM-dd HH:mm:ss";
      MaxLines = 1000;
    }

    // ===== Appearance properties =====

    [Browsable(true)]
    [Category("Log Appearance")]
    [Description("Color of timestamp part")]
    public Color TimestampColor { get; set; }

    [Browsable(true)]
    [Category("Log Appearance")]
    [Description("Color of INFO messages")]
    public Color InfoColor { get; set; }

    [Browsable(true)]
    [Category("Log Appearance")]
    [Description("Color of ERROR messages")]
    public Color ErrorColor { get; set; }

    [Browsable(true)]
    [Category("Log Appearance")]
    [Description("Color of WARNING messages")]
    public Color WarningColor { get; set; }

    [Browsable(true)]
    [Category("Log Appearance")]
    [Description("Color of DEBUG messages")]
    public Color DebugColor { get; set; }

    [Browsable(true)]
    [Category("Log Appearance")]
    [Description("Timestamp format (DateTime.ToString format)")]
    public string TimestampFormat { get; set; }

    [Browsable(true)]
    [Category("Log Behavior")]
    [Description("Maximum number of log lines. Set to 0 to disable auto cleanup.")]
    public int MaxLines { get; set; }

    // ===== Public logging API =====

    public void LogInfo(string message)
    {
      AppendLog("INFO", message, InfoColor);
    }

    public void LogError(string message)
    {
      AppendLog("ERROR", message, ErrorColor);
    }

    public void LogWarning(string message)
    {
      AppendLog("WARN", message, WarningColor);
    }

    public void LogDebug(string message)
    {
      AppendLog("DEBUG", message, DebugColor);
    }

    // ===== Internal logic =====

    private void AppendLog(string level, string message, Color levelColor)
    {
      if (IsDisposed)
        return;

      if (InvokeRequired)
      {
        BeginInvoke(new Action(() => AppendLog(level, message, levelColor)));
        return;
      }

      string timestamp = DateTime.Now.ToString(TimestampFormat);

      AppendColoredText($"[{timestamp}] ", TimestampColor);
      AppendColoredText($"[{level}] ", levelColor);
      AppendColoredText(message, ForeColor);
      AppendText(Environment.NewLine);

      TrimLinesIfNeeded();
      ScrollToBottom();
    }

    private void AppendColoredText(string text, Color color)
    {
      SelectionStart = TextLength;
      SelectionLength = 0;
      SelectionColor = color;

      AppendText(text);

      SelectionColor = ForeColor;
    }

    private void TrimLinesIfNeeded()
    {
      if (MaxLines <= 0)
        return;
      int lineCount = GetLineCount();
      if (lineCount <= MaxLines)
        return;

      int linesToRemove = lineCount - MaxLines;

      int lastCharIndex = GetCharCountToRemove(linesToRemove) - 1;
      
      if (lastCharIndex <= 0)
        return;

      var l = Lines[0].Length;
      SelectionStart = 0;
      SelectionLength = lastCharIndex;
      ReadOnly = false;
      SelectedText = string.Empty;
      ReadOnly = true;
    }

    private int GetCharCountToRemove(int lineCount)
    {
      if (lineCount < 0)
        return 0;

      int charIndex = 0;

      for (int i = 0; i < lineCount && i < Lines.Length; i++)
      {
        charIndex += Lines[i].Length;
        if (i < Lines.Length - 1)
          charIndex += Environment.NewLine.Length;
      }

      return charIndex;
    }

    private int GetLineCount()
    {
      return Lines.Length - 1;
    }

    private void ScrollToBottom()
    {
      SelectionStart = TextLength;
      SelectionLength = 0;
      ScrollToCaret();
    }
  }
}
