using MCUUpdater.CLI;
using PolyBootCore;
using System;

namespace MCUUpdater
{
  static class BootloaderConnectionTransportFactory
  {
    public static ConnectionConfig Create(ICLITransport cliTransport)
    {
      if (cliTransport == null)
        throw new ArgumentNullException(nameof(cliTransport));

      ConnectionConfig connectionConfig;

      if (cliTransport is CLISerialTransportBase serial)
        connectionConfig = CreateSerialConnector(serial);
      else if (cliTransport is CLITcpTransportBase tcp)
        connectionConfig = CreateTcpConnector(tcp);
      else
        throw new NotSupportedException(
          $"Transport '{cliTransport.GetType().Name}' is not supported.");

      connectionConfig.ResponseTimeout = cliTransport.ResponseTimeoutMs;

      return connectionConfig;
    }

    private static ConnectionConfig CreateSerialConnector(CLISerialTransportBase t)
    {
      return new SerialConnectionConfig()
      {
        ComPort = t.Port,
        BaudRate = t.BaudRate
      };
    }

    private static ConnectionConfig CreateTcpConnector(CLITcpTransportBase t)
    {
      return new RawTcpConnectionConfig()
      {
        Host = t.Host,
        Port = t.Port,
        ConnectTimeout = t.ConnectTimeoutMs
      };
    }
  }
}
