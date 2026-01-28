using MCUUpdaterGUI.UserControls;
using PolyBootCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MCUUpdaterGUI
{
  public partial class TransportUI : UserControl
  {
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

    public TransportUI()
    {
      InitializeComponent();
      Init();
    }

    private void Init()
    {
      var transports = new List<Transport>
      {
        new SerialTransport(0) {
          DeviceWait = ConnectionConfig.DefaultDeviceWaitTimeoutSec.ToString(),
          RespTimeout = ConnectionConfig.DefaultResponseTimeoutMs.ToString(),
          PortName = "COM1",
          Baud = SerialConnectionConfig.DefaultBaudRate.ToString(),
        },

        new RawTCPTransport(1)
        {
          DeviceWait = ConnectionConfig.DefaultDeviceWaitTimeoutSec.ToString(),
          RespTimeout = ConnectionConfig.DefaultResponseTimeoutMs.ToString(),
          Host = "localhost",
          Port = RawTcpConnectionConfig.DefaultPort.ToString(),
          ConnectionTimeout = RawTcpConnectionConfig.DefaultConnectTimeoutMs.ToString(),
        }
      };

      comboBoxTransport.DataSource = transports;
      comboBoxTransport.DisplayMember = "Name";
      comboBoxTransport.ValueMember = "Id";

      comboBoxPort_Host.TextChanged += Control_TextChanged;
      textBoxDeviceWait.TextChanged += Control_TextChanged;
      textBoxBaud_NetPort.TextChanged += Control_TextChanged;
      textBoxRespTimeout.TextChanged += Control_TextChanged;
      textBoxConnectTimeout.TextChanged += Control_TextChanged;
    }

    public ConnectionConfig GetEnteredConfig()
    {
      ConnectionConfig config = null;
      if (comboBoxTransport.SelectedItem is SerialTransport serialTransport)
      {
        config = ParseShowSerialTransport(serialTransport);
      }
      else if (comboBoxTransport.SelectedItem is RawTCPTransport rawTcpTransport)
      {
        config = ParseShowRawTcpTransport(rawTcpTransport);
      }

      if (config != null)
      {
        bool isOK = true;

        int ResponseTimeout;
        int DeviceWait;

        if (!int.TryParse(textBoxRespTimeout.Text, out ResponseTimeout))
        {
          FieldMarkError(textBoxRespTimeout);
          isOK = false;
        }

        if (!int.TryParse(textBoxDeviceWait.Text, out DeviceWait))
        {
          FieldMarkError(textBoxDeviceWait);
          isOK = false;
        }

        if (isOK == false)
          return null;

        config.ResponseTimeout = ResponseTimeout;
        config.DeviceWaitTimeout = DeviceWait;
      }

      return config;
    }

    private ConnectionConfig ParseShowRawTcpTransport(RawTCPTransport rawTcpTransport)
    {
      bool isOK = true;

      string Host;
      int Port;
      int ConnectTimeout;

      Host = comboBoxPort_Host.Text;
      if (comboBoxPort_Host.Text == null || comboBoxPort_Host.Text == "")
      {
        FieldMarkError(comboBoxPort_Host);
        isOK = false;
      }

      if (!int.TryParse(textBoxBaud_NetPort.Text, out Port))
      {
        FieldMarkError(textBoxBaud_NetPort);
        isOK = false;
      }

      if (!int.TryParse(textBoxConnectTimeout.Text, out ConnectTimeout))
      {
        FieldMarkError(textBoxConnectTimeout);
        isOK = false;
      }

      if (isOK == false)
        return null;

      return new RawTcpConnectionConfig()
      {
        Host = Host,
        Port = Port,
        ConnectTimeout = ConnectTimeout,
      };
    }

    private ConnectionConfig ParseShowSerialTransport(SerialTransport serialTransport)
    {
      bool isOK = true;

      string ComPort;
      int BaudRate;

      ComPort = comboBoxPort_Host.Text;

      if(comboBoxPort_Host.Text == null || comboBoxPort_Host.Text == "")
      {
        FieldMarkError(comboBoxPort_Host);
        isOK = false;
      }

      if (!int.TryParse(textBoxBaud_NetPort.Text, out BaudRate))
      {
        FieldMarkError(textBoxBaud_NetPort);
        isOK = false;
      }

      if (isOK == false)
        return null;

      return new SerialConnectionConfig()
      {
        ComPort = ComPort,
        BaudRate = BaudRate,
      };
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
      // ToDo: else throw неверный параметр, это заставит
      // обратить внимание на проблему и устранить ее
    }

    private void comboBoxPort_Host_DropDown(object sender, EventArgs e)
    {
      if(comboBoxTransport.SelectedItem is SerialTransport)
      {
        var ports = MISC.GetComPorts();
        comboBoxPort_Host.Items.Clear();
        if (ports != null)
        {
          comboBoxPort_Host.Items.AddRange(ports);
        }
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

    private void FieldMarkError(Control control)
    {
      control.BackColor = Color.Red;
    }

    private void Control_TextChanged(object sender, EventArgs e)
    {
      ((Control)sender).BackColor = SystemColors.Window;
    }
  }
}
