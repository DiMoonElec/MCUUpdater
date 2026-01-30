using MCUUpdaterGUI.UserControls;
using PolyBootCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MCUUpdaterGUI
{
  [Serializable]
  public class TransportUISettings
  {
    public int SettingsVersion { get; set; } = 1;
    public int SelectedTransportId { get; set; } = 0;
    public TransportUISerialSettings SerialSettings { get; set; } = new TransportUISerialSettings();
    public TransportUIRawTCPSettings RawTCPSettings { get; set; } = new TransportUIRawTCPSettings();
  }

  [Serializable]
  public class TransportUISerialSettings
  {
    public int DeviceWaitTimeout { get; set; } = ConnectionConfig.DefaultDeviceWaitTimeoutSec;
    public int ResponseTimeout { get; set; } = ConnectionConfig.DefaultResponseTimeoutMs;
    public string ComPort { get; set; } = "COM1";
    public int BaudRate { get; set; } = SerialConnectionConfig.DefaultBaudRate;
  }

  [Serializable]
  public class TransportUIRawTCPSettings
  {
    public int DeviceWaitTimeout { get; set; } = ConnectionConfig.DefaultDeviceWaitTimeoutSec;
    public int ResponseTimeout { get; set; } = ConnectionConfig.DefaultResponseTimeoutMs;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = RawTcpConnectionConfig.DefaultPort;
    public int ConnectTimeout { get; set; } = RawTcpConnectionConfig.DefaultConnectTimeoutMs;
  }

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

    private TransportUISettings UISettings;

    public TransportUI()
    {
      InitializeComponent();

      comboBoxPort_Host.TextChanged += Control_TextChanged;
      textBoxDeviceWait.TextChanged += Control_TextChanged;
      textBoxBaud_NetPort.TextChanged += Control_TextChanged;
      textBoxRespTimeout.TextChanged += Control_TextChanged;
      textBoxConnectTimeout.TextChanged += Control_TextChanged;
    }

    public void Init(TransportUISettings settings = null)
    {
      if (settings == null)
        UISettings = new TransportUISettings();
      else
        UISettings = settings;

      var transports = new List<Transport>()
      {
        new SerialTransport(0)
        {
          DeviceWait = UISettings.SerialSettings.DeviceWaitTimeout.ToString(),
          RespTimeout = UISettings.SerialSettings.ResponseTimeout.ToString(),
          PortName = UISettings.SerialSettings.ComPort,
          Baud = UISettings.SerialSettings.BaudRate.ToString(),
        },

        new RawTCPTransport(1)
        {
          DeviceWait = UISettings.RawTCPSettings.DeviceWaitTimeout.ToString(),
          RespTimeout = UISettings.RawTCPSettings.ResponseTimeout.ToString(),
          Host = UISettings.RawTCPSettings.Host,
          Port = UISettings.RawTCPSettings.Port.ToString(),
          ConnectionTimeout = UISettings.RawTCPSettings.ConnectTimeout.ToString(),
        }
      };

      comboBoxTransport.DataSource = transports;
      comboBoxTransport.DisplayMember = "Name";
      comboBoxTransport.ValueMember = "Id";
      comboBoxTransport.SelectedValue = UISettings.SelectedTransportId;
    }

    public ConnectionConfig GetEnteredConfig()
    {
      if (comboBoxTransport.SelectedItem is Transport transport)
        return GetConfig(transport);

      return null;
    }

    public TransportUISettings GetSettings()
    {
      var config = GetEnteredConfig();

      if (config != null)
      {
        if (config is SerialConnectionConfig serial)
        {
          UISettings.SerialSettings.DeviceWaitTimeout = serial.DeviceWaitTimeout;
          UISettings.SerialSettings.ResponseTimeout = serial.ResponseTimeout;
          UISettings.SerialSettings.ComPort = serial.ComPort;
          UISettings.SerialSettings.BaudRate = serial.BaudRate;
        }
        else if (config is RawTcpConnectionConfig rawTcp)
        {
          UISettings.RawTCPSettings.DeviceWaitTimeout = rawTcp.DeviceWaitTimeout;
          UISettings.RawTCPSettings.ResponseTimeout = rawTcp.ResponseTimeout;
          UISettings.RawTCPSettings.Host = rawTcp.Host;
          UISettings.RawTCPSettings.Port = rawTcp.Port;
          UISettings.RawTCPSettings.ConnectTimeout = rawTcp.ConnectTimeout;
        }
      }

      UISettings.SelectedTransportId = ((Transport)comboBoxTransport.SelectedItem).Id;
      return UISettings;
    }

    private ConnectionConfig GetConfig(Transport transport)
    {
      ConnectionConfig config = null;

      if (transport is SerialTransport serialTransport)
      {
        config = ParseShowSerialTransport(serialTransport);
      }
      else if (transport is RawTCPTransport rawTcpTransport)
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

    private RawTcpConnectionConfig ParseShowRawTcpTransport(RawTCPTransport rawTcpTransport)
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

    private SerialConnectionConfig ParseShowSerialTransport(SerialTransport serialTransport)
    {
      bool isOK = true;

      string ComPort;
      int BaudRate;

      ComPort = comboBoxPort_Host.Text;

      if (comboBoxPort_Host.Text == null || comboBoxPort_Host.Text == "")
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
      comboBoxPort_Host.Items.Clear();

      if (comboBoxTransport.SelectedItem is SerialTransport)
      {
        var ports = MISC.GetComPorts();
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
