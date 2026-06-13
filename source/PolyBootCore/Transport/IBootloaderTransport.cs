using System.Threading;

namespace PolyBootCore.Transport
{
  internal interface IBootloaderTransport : IBootloaderTransportConnection, IBootloaderTransportChannel
  {
    int ResponseTimeout_ms { get; set; }
    void SetCancellationToken(CancellationToken token);
  }
}
