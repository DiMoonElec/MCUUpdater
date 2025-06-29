using System;
using System.IO.Ports;
using DiMoon.Protocols;

namespace MCUUpdater.Connectors
{
  internal class SerialPortConnector : IDeviceConnector
  {
    private readonly SerialPort serialPort = new SerialPort();
    private readonly BinexLibReceiver binexLibReceiver = new BinexLibReceiver(512);
    private readonly BinexLibTransmitter binexLibTransmitter = new BinexLibTransmitter();

    public SerialPortConnector()
    {
      serialPort.StopBits = StopBits.One;
      serialPort.Parity = Parity.None;
      serialPort.DataBits = 8;

      serialPort.ReadBufferSize = 1024;
      serialPort.WriteBufferSize = 1024;
    }

    public static string[] GetPortNames()
    {
      return SerialPort.GetPortNames();
    }

    public void Open(string portname, int baud)
    {
      serialPort.PortName = portname;
      serialPort.BaudRate = baud;

      binexLibReceiver.Reset();

      serialPort.Open();
      serialPort.DataReceived += SerialPort_DataReceived;
    }

    public bool IsConnected()
    {
      return serialPort.IsOpen;
    }

    public void Close()
    {
      if (serialPort.IsOpen)
      {
        serialPort.DataReceived -= SerialPort_DataReceived;
        serialPort.Close();
      }
    }

    public void Write(byte[] data)
    {
      var pack = binexLibTransmitter.BuildPackage(data);
      serialPort.Write(pack, 0, pack.Length);
    }

    // Событие для передачи данных
    public event EventHandler<byte[]> DataReceived;

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      //List<byte> dbgBuffer = new List<byte>();

      try
      {
        while (serialPort.BytesToRead > 0)
        {
          int byteRead = serialPort.ReadByte();

          //dbgBuffer.Add((byte)byteRead);

          if (binexLibReceiver.Input((byte)byteRead))
          {
            var receivedData = binexLibReceiver.GetReceiveData();

            // Вызов события с переданными данными
            DataReceived?.Invoke(this, receivedData);
          }
        }
      }
      catch (Exception ex)
      {
        // Логирование или обработка ошибок при чтении
        Console.WriteLine($"[Error]: {ex.Message}");
      }

      //var str = BitConverter.ToString(dbgBuffer.ToArray(), 0, dbgBuffer.Count);
      //Log.Information($"(SerialConnector) Data Received: {str}");
    }
  }

}
