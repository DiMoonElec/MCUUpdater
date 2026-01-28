using PolyBootCore;
using PolyBootCore.UpdateFile;
using System;
using System.Drawing;
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

    public MainForm()
    {
      InitializeComponent();
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

      bootloaderWorkflow.EraseProgress += Bootloader_EraseProgress;
      bootloaderWorkflow.UploadProgress += Bootloader_UploadProgress;
      bootloaderWorkflow.UploadEnd += Bootloader_UploadEnd;

      Task.Run(() =>
      {
        try
        {
          var result = bootloaderWorkflow.Update(updateFile);

          string description = BootloaderWorkflow.GetDescription(result);
          switch (result)
          {
            case BootloaderWorkflowResult.ConnectionError:
            case BootloaderWorkflowResult.ConnectionLost:
            case BootloaderWorkflowResult.ErasingError:
            case BootloaderWorkflowResult.IncompatibleDeviceError:
            case BootloaderWorkflowResult.UpdateError:
              AppendErrorToLog(description);
              break;

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
      this.Text = "MCUUpdater GUI v0.3.0";
      UIStateReady();
    }

    private void Connector_ConnectionError(object sender, string e)
    {
      AppendToLog(e);
    }

    private void Bootloader_EraseProgress(int percent)
    {
      this.InvokeIfRequired(() => progressBarProgress.Value = percent);
    }

    private void Bootloader_UploadProgress(int percent)
    {
    }

    private void Bootloader_UploadEnd()
    {
    }

    private void AppendInfoToLog(string message)
    {
      AppendToLog(message);
    }

    private void AppendErrorToLog(string message)
    {
      AppendToLog("[ERROR] " + message);
    }

    private void AppendToLog(string message)
    {
      this.InvokeIfRequired(() =>
      {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        textBoxLog.Text += $">:[{timestamp}] {message}\r\n";

        // Прокрутка к последней строке без установки фокуса
        textBoxLog.SelectionStart = textBoxLog.TextLength;
        textBoxLog.SelectionLength = 0;
        textBoxLog.ScrollToCaret();
      });
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
  }
}
