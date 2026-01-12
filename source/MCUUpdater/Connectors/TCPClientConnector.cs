using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MCUUpdater.Connectors
{
  internal class TCPClientConnector : IDeviceConnector
  {
    private TcpClient tcpClient;
    private NetworkStream netStream;
    private CancellationTokenSource receiveCts;

    public string Host { get; private set; }
    public int Port { get; private set; }

    /// <summary>
    /// Тайм-аут установки соединения в миллисекундах.
    /// </summary>
    public int ConnectTimeoutMs { get; private set; } = 5000;

    /// <summary>
    /// Размер буфера для чтения входящих данных.
    /// </summary>
    public int ReadBufferSize { get; set; } = 1024;

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
        StartReceiveLoop();

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
        // Отменяем приём
        try
        {
          receiveCts?.Cancel();
        }
        catch { /* ignore */ }

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
      finally
      {
        receiveCts = null;
      }
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

      try
      {
        if (IsConnected())
        {
          netStream.Write(data, 0, data.Length);
        }
        else
        {
          RaiseConnectionError("Попытка записи при закрытом соединении.");
        }
      }
      catch (Exception ex)
      {
        RaiseConnectionError($"Ошибка записи: {ex.Message}");
      }
    }

    public event EventHandler<byte[]> DataReceived;
    public event EventHandler<string> ConnectionError;

    private void StartReceiveLoop()
    {
      // отмена предыдущей задачи, если есть
      try
      {
        receiveCts?.Cancel();
      }
      catch { }

      receiveCts = new CancellationTokenSource();
      var token = receiveCts.Token;

      Task.Run(async () =>
      {
        var buffer = new byte[ReadBufferSize];

        try
        {
          while (!token.IsCancellationRequested && IsConnected())
          {
            int read = 0;
            try
            {
              read = await netStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
              break;
            }
            catch (ObjectDisposedException)
            {
              break;
            }
            catch (Exception exRead)
            {
              RaiseConnectionError($"Ошибка при чтении: {exRead.Message}");
              break;
            }

            if (read == 0)
            {
              // Соединение закрыто удалённой стороной
              RaiseConnectionError("Соединение закрыто удалённой стороной.");
              Disconnect();
              break;
            }

            var outBuf = new byte[read];
            Buffer.BlockCopy(buffer, 0, outBuf, 0, read);

            try
            {
              DataReceived?.Invoke(this, outBuf);
            }
            catch
            {
              // Защищаем цикл от исключений обработчиков событий
            }
          }
        }
        finally
        {
          // При выходе из цикла — убедимся, что соединение закрыто
          try { Disconnect(); } catch { }
        }
      }, token);
    }

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
