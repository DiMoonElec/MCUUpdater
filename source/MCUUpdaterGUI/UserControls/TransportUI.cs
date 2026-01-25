using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MCUUpdaterGUI
{
  public partial class TransportUI : UserControl
  {
    public TransportUI()
    {
      InitializeComponent();
      Init();
    }


    private void Init()
    {
      var transports = new List<Transport>
      {
        new SerialTransport(0) {DeviceWait = "60", PortName = "COM1", Baud = "115200", RespTimeout = "1000"},
        new RawTCPTransport(1) {DeviceWait = "60", Host = "localhost", Port = "5555", RespTimeout = "1000", ConnectionTimeout = "1000"}
      };

      comboBoxTransport.DataSource = transports;
      comboBoxTransport.DisplayMember = "Name";
      comboBoxTransport.ValueMember = "Id";
    }

    private void comboBoxTransport_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (comboBoxTransport.SelectedItem is SerialTransport serialTransport)
      {
        ShowSerialTransport(serialTransport);
      }
      else if (comboBoxTransport.SelectedItem is RawTCPTransport rawTcpTransport)
      {
        ShowRawTcpTransport(rawTcpTransport);
      }
    }
    private void ShowSerialTransport(SerialTransport transport)
    {
      // Set labels
      labelConnectTimeout.Visible = false;

      labelPort_Host.Text = "Port";
      labelBaud_NetPort.Text = "Baud";
      labelRespTimeout.Text = "Response Timeout, ms";

      // Set Fields
      textBoxConnectTimeout.DataBindings.Clear();
      textBoxConnectTimeout.Visible = false;

      textBoxDeviceWait.DataBindings.Clear();
      textBoxDeviceWait.DataBindings.Add("Text", transport, nameof(transport.DeviceWait), false, DataSourceUpdateMode.OnPropertyChanged);

      comboBoxPort_Host.DataBindings.Clear();
      comboBoxPort_Host.DataBindings.Add("Text", transport, nameof(transport.PortName), false, DataSourceUpdateMode.OnPropertyChanged);

      textBoxBaud_NetPort.DataBindings.Clear();
      textBoxBaud_NetPort.DataBindings.Add("Text", transport, nameof(transport.Baud), false, DataSourceUpdateMode.OnPropertyChanged);

      textBoxRespTimeout.DataBindings.Clear();
      textBoxRespTimeout.DataBindings.Add("Text", transport, nameof(transport.RespTimeout), false, DataSourceUpdateMode.OnPropertyChanged);

    }

    private void ShowRawTcpTransport(RawTCPTransport transport)
    {
      // Set labels
      labelConnectTimeout.Visible = true;

      labelPort_Host.Text = "Host";
      labelBaud_NetPort.Text = "TCP Port";
      labelRespTimeout.Text = "Response Timeout, ms";
      labelConnectTimeout.Text = "Connection Timeout, ms";

      // Set Fields
      textBoxConnectTimeout.DataBindings.Clear();
      textBoxConnectTimeout.Visible = true;
      textBoxConnectTimeout.DataBindings.Add("Text", transport, nameof(transport.ConnectionTimeout), false, DataSourceUpdateMode.OnPropertyChanged);

      textBoxDeviceWait.DataBindings.Clear();
      textBoxDeviceWait.DataBindings.Add("Text", transport, nameof(transport.DeviceWait), false, DataSourceUpdateMode.OnPropertyChanged);

      comboBoxPort_Host.DataBindings.Clear();
      comboBoxPort_Host.DataBindings.Add("Text", transport, nameof(transport.Host), false, DataSourceUpdateMode.OnPropertyChanged);

      textBoxBaud_NetPort.DataBindings.Clear();
      textBoxBaud_NetPort.DataBindings.Add("Text", transport, nameof(transport.Port), false, DataSourceUpdateMode.OnPropertyChanged);

      textBoxRespTimeout.DataBindings.Clear();
      textBoxRespTimeout.DataBindings.Add("Text", transport, nameof(transport.RespTimeout), false, DataSourceUpdateMode.OnPropertyChanged);
    }

    abstract class Transport
    {
      public string Name { get; internal set; }
      public int Id { get; internal set; }
      public string DeviceWait { get; set; }
      public string RespTimeout { get; set; }
    }

    class SerialTransport : Transport
    {
      public string PortName { get; set; }
      public string Baud { get; set; }

      public SerialTransport(int Id)
      {
        this.Name = "SERIAL";
        this.Id = Id;
      }
    }

    class RawTCPTransport : Transport
    {
      public string Host { get; set; }
      public string Port { get; set; }
      public string ConnectionTimeout { get; set; }

      public RawTCPTransport(int Id)
      {
        this.Name = "RAW-TCP";
        this.Id = Id;
      }
    }
  }


}
