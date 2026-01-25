using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PolyBootCore;
using PolyBootCore.Bootloader.Transport;
using PolyBootCore.Connectors;
using PolyBootCore.UpdateFile;

namespace MCUUpdaterGUI
{
  public partial class MainForm : Form
  {
    static readonly Color buttonStartUpdateActive = Color.FromArgb(0, 192, 0);
    static readonly Color buttonAbortActive = Color.FromArgb(255, 128, 128);

    static CancellationTokenSource cts = null;

    public MainForm()
    {
      InitializeComponent();
    }

    private void buttonStartUpdate_Click(object sender, EventArgs e)
    {
      cts = new CancellationTokenSource();

      var transport = CreateTransport();
      var bootloaderWorkflow = new BootloaderWorkflow(transport, cts.Token);
      var updateFile = FirmwareUpdateParser.Parse("testfile.xbin");

      bootloaderWorkflow.EraseProgress += Bootloader_EraseProgress;
      bootloaderWorkflow.UploadProgress += Bootloader_UploadProgress;
      bootloaderWorkflow.UploadEnd += Bootloader_UploadEnd;

      Task.Run(() =>
      {
        try
        {
          var result = bootloaderWorkflow.Update(updateFile, 600);
          AppendToLog($"Result: {result.ToString()}");

          this.InvokeIfRequired(() => UIStateReady());
        }
        catch(Exception ex) 
        {
          AppendToLog($"Error: {ex.Message}");
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


    private IBootloaderTransport CreateTransport()
    {
      var connector = new SerialPortConnector();
      connector.ConnectionError += Connector_ConnectionError;
      connector.SetConnectionParams("COM1", 119200);
      var transport = new BootloaderTransport(connector)
      {
        ResponseTimeout_ms = 2000
      };

      return transport;
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

    private void AppendToLog(string message)
    {
      this.InvokeIfRequired(() => textBoxLog.Text += message + "\r\n");
    }
  }
}
