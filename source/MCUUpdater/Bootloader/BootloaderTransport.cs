using System.Diagnostics;
using DiMoon.Protocols;
using MCUUpdater.Connectors;
using MCUUpdater.MISC;

namespace MCUUpdater.Bootloader
{
  internal class BootloaderTransport : IBootloaderTransport
  {
    public int ResponseTimeout_ms { get; set; } = 500;

    private IDeviceConnector DeviceConnector;

    private readonly BinexLibReceiver binexLibReceiver = new BinexLibReceiver(512);
    private readonly BinexLibTransmitter binexLibTransmitter = new BinexLibTransmitter();

    private readonly CircularQueue<byte[]> queue = new CircularQueue<byte[]>(128);

    private byte[] receiveBuffer = new byte[256];

    public bool Connect() => DeviceConnector.Connect();

    public void Disconnect() => DeviceConnector.Disconnect();

    public BootloaderTransport(IDeviceConnector deviceConnector)
    {
      DeviceConnector = deviceConnector;
      deviceConnector.ReadTimeout = 100;
      deviceConnector.WriteTimeout = ResponseTimeout_ms;
    }

    public bool Send(byte[] data)
    {
      if (DeviceConnector.IsConnected() == false)
        return false;

      queue.Clear();

      var pack = binexLibTransmitter.BuildPackage(data);

      try
      {
        DeviceConnector.Write(pack);
      }
      catch
      {
        // Во время отправки пакета возникли в потоке передачи,
        // интерпретируем это как обрыв связи
        return false;
      }

      return true;
    }

    public byte[] Receive()
    {
      long start = Stopwatch.GetTimestamp();

      // Конвертируем миллисекунды в системные тики
      long timeoutTicks = (long)(ResponseTimeout_ms * Stopwatch.Frequency / 1000);

      while ((Stopwatch.GetTimestamp() - start) < timeoutTicks)
      {
        if (queue.Count > 0)
          return queue.Dequeue();

        try
        {
          int count = DeviceConnector.Read(receiveBuffer, 0, receiveBuffer.Length);

          if (count > 0)
          {
            for (int i = 0; i < count; i++)
            {
              if (binexLibReceiver.Input(receiveBuffer[i]))
                queue.Enqueue(binexLibReceiver.GetReceiveData());
            }
          }
        }
        catch
        {
          // Возникли ошибки потока, интерпретируем это как обрыв связи
          return null;
        }
      }

      //Тайм-аут приема пакета
      return null;
    }
  }
}
