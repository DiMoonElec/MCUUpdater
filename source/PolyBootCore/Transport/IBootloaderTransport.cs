using System.Threading;

namespace PolyBootCore.Transport
{
  internal interface IBootloaderTransport
  {
    int ResponseTimeout_ms { get; set; }
    void SetCancellationToken(CancellationToken token);
    bool Connect();
    void Disconnect();
    bool Send(byte[] data);
    byte[] Receive();
  }
}
