using PolyBootCore.Connectors;
using PolyBootCore.MISC;

namespace PolyBootCore
{
  public class RawTcpConnectionConfig : ConnectionConfig
  {
    static public readonly int DefaultPort = 5000;
    static public readonly int DefaultConnectTimeoutMs = 2000;

    readonly SetOnce<string> _Host = new SetOnce<string>("Host");
    readonly SetOnce<int> _Port = new SetOnce<int>("Port");
    readonly SetOnce<int> _ConnectTimeout = new SetOnce<int>("ConnectTimeout");

    public string Host { get => _Host.Value; set => _Host.Value = value; }
    public int Port { get => _Port.Value; set => _Port.Value = value; }
    public int ConnectTimeout { get => _ConnectTimeout.Value; set => _ConnectTimeout.Value = value; }

    private protected override IDeviceConnector CreateConnector()
    {
      var connector = new TCPClientConnector();
      connector.SetConnectionParams(Host, Port, ConnectTimeout);
      return connector;
    }
  }
}
