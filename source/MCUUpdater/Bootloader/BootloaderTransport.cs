using DiMoon.Protocols;
using MCUUpdater.Bootloader.MISC;
using MCUUpdater.Connectors;

namespace MCUUpdater.Bootloader
{
  internal class BootloaderTransport : IBootloaderTransport
  {
    public int ResponseTimeout_ms { get; set; } = 500;

    private IDeviceConnector DeviceConnector;

    private readonly BinexLibReceiver binexLibReceiver = new BinexLibReceiver(512);
    private readonly BinexLibTransmitter binexLibTransmitter = new BinexLibTransmitter();

    private PacketQueue respQueue = new PacketQueue(128);

    public bool Connect() => DeviceConnector.Connect();
    public void Disconnect() => DeviceConnector.Disconnect();

    public BootloaderTransport(IDeviceConnector deviceConnector)
    {
      DeviceConnector = deviceConnector;
      DeviceConnector.DataReceived += DeviceConnector_DataReceived;
    }
    public bool Send(byte[] data)
    {
      if (DeviceConnector.IsConnected() == false)
        return false;

      respQueue.Clear();

      var pack = binexLibTransmitter.BuildPackage(data);
      DeviceConnector.Write(pack);
      return true;
    }

    public byte[] Receive()
    {
      return respQueue.Pop(ResponseTimeout_ms);
    }

    private void DeviceConnector_DataReceived(object sender, byte[] data)
    {
      foreach (var d in data)
      {
        if (binexLibReceiver.Input(d))
        {
          var pack = binexLibReceiver.GetReceiveData();
          respQueue.Push(pack);
        }
      }
    }
  }
}
