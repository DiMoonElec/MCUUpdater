namespace PolyBootCore.Transport
{
  internal interface IBootloaderTransportConnection
  {
    bool Connect();
    void Disconnect();
  }
}
