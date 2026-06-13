using System.Threading;

namespace PolyBootCore.Transport
{
  internal interface IBootloaderTransport : IBootloaderConnectionManager
  {
    int ResponseTimeout_ms { get; set; }
    void SetCancellationToken(CancellationToken token);
    bool Send(byte[] data);
    byte[] Receive();
  }
}
