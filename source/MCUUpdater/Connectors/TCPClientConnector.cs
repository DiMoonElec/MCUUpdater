using System;
using System.Net.Sockets;

namespace MCUUpdater.Connectors
{
  internal class TCPClientConnector : IDeviceConnector
  {
    private TcpClient tcpClient;
    private NetworkStream netStream;

    public string Host { get; private set; }
    public int Port { get; private set; }

    /// <summary>
    /// Тайм-аут установки соединения в миллисекундах.
    /// </summary>
    public int ConnectTimeoutMs { get; private set; } = 5000;

    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }

    public TCPClientConnector()
    {
    }

    /// <summary>
    /// Установить параметры подключения.
    /// </summary>
    public void SetConnectionParams(string host, int port, int connectTimeoutMs = 5000)
    {
      Host = host;
      Port = port;
      ConnectTimeoutMs = connectTimeoutMs;
    }

    /// <summary>
    /// Попытка установить TCP-соединение в пределах ConnectTimeoutMs.
    /// </summary>
    public bool Connect()
    {
      if (string.IsNullOrEmpty(Host) || Port <= 0)
      {
        RaiseConnectionError("Некорректные параметры подключения (host/port).");
        return false;
      }

      try
      {
        // Закрыть старое при повторном подключении
        Disconnect();

        tcpClient = new TcpClient();

        var connectTask = tcpClient.ConnectAsync(Host, Port);
        var completed = connectTask.Wait(ConnectTimeoutMs);

        if (!completed)
        {
          // Таймаут
          RaiseConnectionError($"Тайм-аут при подключении к {Host}:{Port} ({ConnectTimeoutMs} ms).");
          tcpClient.Close();
          tcpClient = null;
          return false;
        }

        // возможная ошибка при подключении (исключение внутри задачи)
        if (connectTask.IsFaulted)
        {
          var ex = connectTask.Exception?.GetBaseException();
          RaiseConnectionError($"Ошибка подключения: {ex?.Message ?? "неизвестная ошибка"}");
          tcpClient.Close();
          tcpClient = null;
          return false;
        }

        if (!tcpClient.Connected)
        {
          RaiseConnectionError($"Не удалось подключиться к {Host}:{Port}.");
          tcpClient.Close();
          tcpClient = null;
          return false;
        }

        // Открываем поток и запускаем приём
        netStream = tcpClient.GetStream();
        netStream.ReadTimeout = ReadTimeout;
        netStream.WriteTimeout = WriteTimeout;
        return true;
      }
      catch (SocketException sex)
      {
        RaiseConnectionError($"SocketException: {sex.Message}");
      }
      catch (Exception ex)
      {
        RaiseConnectionError($"Ошибка при подключении: {ex.Message}");
      }

      return false;
    }

    public void Disconnect()
    {
      try
      {
        // Закрываем поток и клиент
        if (netStream != null)
        {
          try { netStream.Close(); } catch { }
          netStream = null;
        }

        if (tcpClient != null)
        {
          try { tcpClient.Close(); } catch { }
          tcpClient = null;
        }
      }
      catch { }
    }

    public bool IsConnected()
    {
      try
      {
        return tcpClient != null && tcpClient.Connected && netStream != null && netStream.CanRead;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Закрытие (синоним Disconnect для совместимости с SerialPortConnector).
    /// </summary>
    public void Close()
    {
      Disconnect();
    }

    public void Write(byte[] data)
    {
      if (data == null) return;

      /*
        Исключения записи в поток будут отлавливаться
        вышестоящим кодом
      */
      if (netStream != null)
        netStream.Write(data, 0, data.Length);
    }

    public int Read()
    {
      if (netStream == null)
        return -1;

      try
      {
        return netStream.ReadByte();
      }
      catch
      {
        return -1;
      }
    }

    public int Read(byte[] buffer, int offset, int count)
    {
      if (netStream == null)
        return 0;

      try
      {
        return netStream.Read(buffer, offset, count);
      }
      catch (Exception ex) when (ex.InnerException is SocketException socket_ex)
      {
        if (socket_ex.SocketErrorCode == SocketError.TimedOut)
          return 0; // Если тайм-аут, то возвращаем 0
        else
          throw socket_ex; // Если что-то другое, то пробрасываем исключение далее
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
