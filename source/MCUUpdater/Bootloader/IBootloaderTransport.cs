namespace MCUUpdater.Bootloader
{
  internal interface IBootloaderTransport
  {
    int ResponseTimeout { get; set; }
    bool Send(byte[] data);
    byte[] Receive();
  }
}
