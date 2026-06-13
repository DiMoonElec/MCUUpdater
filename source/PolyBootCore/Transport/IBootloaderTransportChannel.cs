namespace PolyBootCore.Transport
{
  internal interface IBootloaderTransportChannel
  {
    bool Send(byte[] data);
    byte[] Receive();
  }
}
