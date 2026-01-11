using System;

namespace MCUUpdater.Connectors
{
  internal interface IDeviceConnector
  {
    bool Connect();
    void Disconnect();
    bool IsConnected();
    void Write(byte[] data);
    event EventHandler<byte[]> DataReceived;

    event EventHandler<string> ConnectionError;
  }
}
