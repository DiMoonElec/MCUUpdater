using System;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace MCUUpdater.Connectors
{
  internal class SerialPortConnector : IDeviceConnector
  {
    private readonly SerialPort serialPort = new SerialPort();

    public string PortName { get; private set; }
    public int BaudRate { get; private set; }

    public SerialPortConnector()
    {
      serialPort.StopBits = StopBits.One;
      serialPort.Parity = Parity.None;
      serialPort.DataBits = 8;

      serialPort.ReadBufferSize = 1024;
      serialPort.WriteBufferSize = 1024;
    }

    public bool Connect()
    {
      var availablePorts = SerialPort.GetPortNames();

      if (!availablePorts.Contains(PortName))
      {
        RaiseConnectionError($"Порт {PortName} не найден");
        return false;
      }

      try
      {
        if (serialPort.IsOpen)
          serialPort.Close();

        serialPort.PortName = PortName;
        serialPort.BaudRate = BaudRate;

        serialPort.DataReceived -= SerialPort_DataReceived;
        serialPort.DataReceived += SerialPort_DataReceived;

        serialPort.Open();

        if (!serialPort.IsOpen)
        {
          RaiseConnectionError($"Не удалось открыть порт {PortName}");
          return false;
        }

        return true;
      }
      catch (UnauthorizedAccessException)
      {
        RaiseConnectionError($"Порт {PortName} занят другим приложением");
      }
      catch (IOException)
      {
        RaiseConnectionError($"Ошибка ввода-вывода на порту {PortName}");
      }
      catch (ArgumentException)
      {
        RaiseConnectionError("Некорректные параметры порта");
      }
      catch (Exception ex)
      {
        RaiseConnectionError($"Ошибка открытия порта: {ex.Message}");
      }

      return false;
    }

    public void Disconnect()
    {
      if (serialPort.IsOpen)
        serialPort.Close();
    }

    public static string[] GetPortNames()
    {
      return SerialPort.GetPortNames();
    }

    public void SetConnectionParams(string portname, int baud)
    {
      PortName = portname;
      BaudRate = baud;
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
      serialPort.Write(data, 0, data.Length);
    }

    // Событие для передачи данных
    public event EventHandler<byte[]> DataReceived;

    public event EventHandler<string> ConnectionError;

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      try
      {
        var bytes = serialPort.BytesToRead;
        var buffer = new byte[bytes];
        serialPort.Read(buffer, 0, bytes);

        DataReceived?.Invoke(this, buffer);
      }
      catch (Exception ex)
      {
        // Логирование или обработка ошибок при чтении
        Console.WriteLine($"[Error]: {ex.Message}");
      }
    }

    private void RaiseConnectionError(string message)
    {
      ConnectionError?.Invoke(this, message);
    }
  }
}
