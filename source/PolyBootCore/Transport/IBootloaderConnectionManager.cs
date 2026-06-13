namespace PolyBootCore.Transport
{
  internal interface IBootloaderConnectionManager
  {
    bool Connect();
    void Disconnect();
  }
}
