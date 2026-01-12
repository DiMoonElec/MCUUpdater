using System;
using MCUUpdater.Bootloader;
using MCUUpdater.CLI;
using MCUUpdater.Connectors;

namespace MCUUpdater
{
  static class BootloaderTransportFactory
  {
    public static IBootloaderTransport Create(ICLITransport cliTransport)
    {
      if (cliTransport == null)
        throw new ArgumentNullException(nameof(cliTransport));

      IDeviceConnector connector;

      if (cliTransport is CLISerialTransportBase serial)
        connector = CreateSerialConnector(serial);
      else if (cliTransport is CLITcpTransportBase tcp)
        connector = CreateTcpConnector(tcp);
      else
        throw new NotSupportedException(
          $"Transport '{cliTransport.GetType().Name}' is not supported.");

      var transport = new BootloaderTransport(connector)
      {
        ResponseTimeout_ms = cliTransport.ResponseTimeoutSec * 1000
      };

      return transport;
    }

    private static IDeviceConnector CreateSerialConnector(CLISerialTransportBase t)
    {
      var connector = new SerialPortConnector();
      connector.SetConnectionParams(t.Port, t.BaudRate);
      return connector;
    }

    private static IDeviceConnector CreateTcpConnector(CLITcpTransportBase t)
    {
      var connector = new TCPClientConnector();
      connector.SetConnectionParams(
        t.Host,
        t.Port,
        t.ConnectTimeoutSec * 1000);
      return connector;
    }
  }
}
