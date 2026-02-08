using MCUUpdaterGUI.Settings;
using PolyBootCore;
using PolyBootCore.UpdateFile;
using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCUUpdaterGUI
{
  public partial class MainForm : Form
  {
    static readonly Color buttonStartUpdateActive = Color.FromArgb(0, 192, 0);
    static readonly Color buttonAbortActive = Color.FromArgb(255, 128, 128);

    private CancellationTokenSource cts = null;

    private string selectedUpdateFilePath = null;

    private AppSettings appSettings = null;
    public MainForm()
    {
      InitializeComponent();
      appSettings = AppSettings.Load();
      transportUI1.Init(appSettings.TransportUISettings);
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      appSettings.TransportUISettings = transportUI1.GetSettings();
      AppSettings.Save(appSettings);
    }

    private void buttonStartUpdate_Click(object sender, EventArgs e)
    {
      if (selectedUpdateFilePath == null)
      {
        MessageBox.Show("Update file is not selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      var enteredConfig = transportUI1.GetEnteredConfig();

      if (enteredConfig == null)
      {
        MessageBox.Show("One or more connection parameters have an invalid data format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      FirmwareUpdateFile updateFile = null;

      try
      {
        updateFile = FirmwareUpdateParser.Parse(selectedUpdateFilePath);
      }
      catch (Exception ex)
      {
        AppendErrorToLog(ex.Message);
        return;
      }

      AppendInfoToLog("Start of update");

      cts = new CancellationTokenSource();
      var bootloaderWorkflow = new BootloaderWorkflow(enteredConfig, cts.Token);

      bootloaderWorkflow.EraseBegin += BootloaderWorkflow_EraseBegin;
      bootloaderWorkflow.EraseProgress += BootloaderWorkflow_EraseProgress;
      bootloaderWorkflow.EraseEnd += BootloaderWorkflow_EraseEnd;

      bootloaderWorkflow.UploadBegin += BootloaderWorkflow_UploadBegin;
      bootloaderWorkflow.UploadProgress += BootloaderWorkflow_UploadProgress;
      bootloaderWorkflow.UploadEnd += BootloaderWorkflow_UploadEnd;

      Task.Run(() =>
      {
        try
        {
          var result = bootloaderWorkflow.Update(updateFile);

          string description = BootloaderWorkflow.GetDescription(result);
          switch (result)
          {
            // Error cases
            case BootloaderWorkflowResult.ConnectionError:
            case BootloaderWorkflowResult.ConnectionLost:
            case BootloaderWorkflowResult.ErasingError:
            case BootloaderWorkflowResult.IncompatibleDeviceError:
            case BootloaderWorkflowResult.UpdateError:
              SetProgressBar(0);
              SetProgressBarLabel(" Error");
              AppendErrorToLog(description);
              break;

            // ОК cases
            default:
              AppendInfoToLog(description);
              break;
          }
        }
        catch (Exception ex)
        {
          AppendErrorToLog(ex.Message);
        }
        finally
        {
          this.InvokeIfRequired(() => UIStateReady());
        }
      });

      UIStateUploading();
    }

    private void BootloaderWorkflow_EraseBegin()
    {
      AppendInfoToLog("Flash erase start");
      SetProgressBarLabel(" Erase");
    }

    private void BootloaderWorkflow_EraseProgress(int percent)
    {
      SetProgressBar(percent);
    }

    private void BootloaderWorkflow_EraseEnd()
    {
      AppendInfoToLog("Flash erase complete");
      SetProgressBar(100);
    }

    private void BootloaderWorkflow_UploadBegin()
    {
      AppendInfoToLog("Upload start");
      SetProgressBarLabel("Upload");
    }

    private void BootloaderWorkflow_UploadProgress(int percent)
    {
      SetProgressBar(percent);
    }

    private void BootloaderWorkflow_UploadEnd()
    {
      AppendInfoToLog("Upload complete");
      SetProgressBarLabel("Ready");
      SetProgressBar(100);
    }

    private void buttonAbort_Click(object sender, EventArgs e)
    {
      cts?.Cancel();
      cts = null;

      buttonAbort.Enabled = false;
      buttonAbort.BackColor = SystemColors.Control;
    }

    private void UIStateUploading()
    {
      buttonStartUpdate.Enabled = false;
      buttonStartUpdate.BackColor = SystemColors.Control;

      buttonAbort.Enabled = true;
      buttonAbort.BackColor = buttonAbortActive;
    }

    private void UIStateReady()
    {
      buttonStartUpdate.Enabled = true;
      buttonStartUpdate.BackColor = buttonStartUpdateActive;

      buttonAbort.Enabled = false;
      buttonAbort.BackColor = SystemColors.Control;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

      this.Text = $"MCUUpdater GUI v{version}";
      UIStateReady();
    }

    private void Connector_ConnectionError(object sender, string e)
    {
      //AppendToLog(e);
    }

    private void SetProgressBar(int percent)
    {
      this.InvokeIfRequired(() => progressBarProgress.Value = percent);
    }

    private void SetProgressBarLabel(string str)
    {
      this.InvokeIfRequired(() => labelProgressBar.Text = str);
    }

    private void AppendInfoToLog(string message)
    {
      this.InvokeIfRequired(() => logRichTextBox.LogInfo(message));
    }

    private void AppendErrorToLog(string message)
    {
      this.InvokeIfRequired(() => logRichTextBox.LogError(message));
    }

    private void buttonSelectFile_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.Title = "Select update file";
        openFileDialog.Filter = "Update files (*.xbin)|*.xbin|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 1;
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
          selectedUpdateFilePath = openFileDialog.FileName;
          textBoxSelectedFile.Text = selectedUpdateFilePath;
        }
      }
    }

    private void MainForm_HelpRequested(object sender, HelpEventArgs hlpevent)
    {
      string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      string message = $"MCU Updater GUI Utility {Environment.NewLine}";
      message += $"Version {version}{Environment.NewLine}";
      message += Environment.NewLine;
      message += $"Author: DiMoon Electronics{Environment.NewLine}";
      message += $"Website: dimoon.ru{Environment.NewLine}";

      MessageBox.Show(message, "About", MessageBoxButtons.OK, MessageBoxIcon.None);
    }
  }
}
