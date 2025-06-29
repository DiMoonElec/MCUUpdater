using System;

namespace MCUUpdater.Connectors
{
  internal interface IDeviceConnector
  {
    bool IsConnected();
    void Write(byte[] data);
    event EventHandler<byte[]> DataReceived;
  }
}
