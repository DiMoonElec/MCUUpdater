using System;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace MCUUpdater.Connectors
{
  internal class SerialPortConnector : IDeviceConnector
  {
    private readonly SerialPort serialPort = new SerialPort();
    private Stream stream;

    public string PortName { get; private set; }
    public int BaudRate { get; private set; }

    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }

    public SerialPortConnector()
    {
      serialPort.StopBits = StopBits.One;
      serialPort.Parity = Parity.None;
      serialPort.DataBits = 8;
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

        serialPort.Open();

        if (!serialPort.IsOpen)
        {
          RaiseConnectionError($"Не удалось открыть порт {PortName}");
          return false;
        }
        stream = serialPort.BaseStream;
        stream.ReadTimeout = ReadTimeout;
        stream.WriteTimeout = WriteTimeout;
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
      stream = null;

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
        serialPort.Close();
      }
    }

    public void Write(byte[] data)
    {
      /*
        Исключения записи в поток будут отлавливаться
        вышестоящим кодом
      */
      if (stream != null)
        stream.Write(data, 0, data.Length);
    }

    public int Read()
    {
      if (stream == null)
        return -1;

      try
      {
        return stream.ReadByte();
      }
      catch
      {
        return -1;
      }
    }

    public int Read(byte[] buffer, int offset, int count)
    {
      if (stream == null)
        return 0;

      try
      {
        return stream.Read(buffer, offset, count);
      }
      catch (TimeoutException ex)
      {
        return 0;
      }
    }

    public event EventHandler<string> ConnectionError;

    private void RaiseConnectionError(string message)
    {
      try
      {
        ConnectionError?.Invoke(this, message);
      }
      catch
      {
        // Ничего не делаем, чтобы ошибка обработчика не ломала логику
      }
    }
  }
}
