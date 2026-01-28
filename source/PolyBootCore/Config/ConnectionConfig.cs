using PolyBootCore.Transport;
using PolyBootCore.Connectors;
using PolyBootCore.MISC;

namespace PolyBootCore
{
  public abstract class ConnectionConfig
  {
    static public readonly int DefaultResponseTimeoutMs = 1000;
    static public readonly int DefaultDeviceWaitTimeoutSec = 60;

    readonly SetOnce<int> _ResponseTimeout = new SetOnce<int>("ResponseTimeout");
    readonly SetOnce<int> _DeviceWaitTimeout = new SetOnce<int>("DeviceWaitTimeout");

    public int ResponseTimeout { get => _ResponseTimeout.Value; set => _ResponseTimeout.Value = value; }
    public int DeviceWaitTimeout { get => _DeviceWaitTimeout.Value; set => _DeviceWaitTimeout.Value = value; }

    private protected abstract IDeviceConnector CreateConnector();

    internal IBootloaderTransport CreateTransport()
    {
      var transport = new BootloaderTransport(CreateConnector())
      {
        ResponseTimeout_ms = ResponseTimeout
      };

      return transport;
    }
  }
}
