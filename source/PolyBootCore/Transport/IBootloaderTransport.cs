using System.Threading;

namespace PolyBootCore.Bootloader.Transport
{
  public interface IBootloaderTransport
  {
    int ResponseTimeout_ms { get; set; }
    void SetCancellationToken(CancellationToken token);
    bool Connect();
    void Disconnect();
    bool Send(byte[] data);
    byte[] Receive();
  }
}
