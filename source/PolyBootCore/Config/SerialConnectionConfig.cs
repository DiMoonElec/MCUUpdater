using PolyBootCore.Connectors;
using PolyBootCore.MISC;

namespace PolyBootCore
{
  public class SerialConnectionConfig : ConnectionConfig
  {
    static public readonly int DefaultBaudRate = 115200;

    readonly SetOnce<string> _ComPort = new SetOnce<string>("ComPort");
    readonly SetOnce<int> _BaudRate = new SetOnce<int>("BaudRate");

    public string ComPort { get => _ComPort.Value; set => _ComPort.Value = value; }
    public int BaudRate { get => _BaudRate.Value; set => _BaudRate.Value = value; }


    private protected override IDeviceConnector CreateConnector()
    {
      var connector = new SerialPortConnector();
      connector.SetConnectionParams(ComPort, BaudRate);
      return connector;
    }
  }
}
