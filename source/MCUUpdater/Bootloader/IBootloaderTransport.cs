namespace MCUUpdater.Bootloader
{
  public interface IBootloaderTransport
  {
    int ResponseTimeout_ms { get; set; }
    bool Connect();
    void Disconnect();
    bool Send(byte[] data);
    byte[] Receive();
  }
}
