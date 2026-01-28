namespace MCUUpdater.CLI
{
  // ===== Root =====

  public class CLIOptions
  {
    // Global options
    public int WaitTimeoutSec { get; set; } = 60;
    public bool ShowHelp { get; set; } = false;
    public bool Verbose { get; set; } = false;

    // Command
    public ICLICommand Command { get; set; }
  }

  // ===== Commands =====

  public interface ICLICommand
  {
  }

  public class CLIUpdateCommand : ICLICommand
  {
    public string FirmwareFile { get; set; }
    public ICLITransport Transport { get; set; }
  }

  // ===== Transport =====

  public interface ICLITransport
  {
    int ResponseTimeoutMs { get; set; }
  }

  // ===== Serial-based =====

  public abstract class CLISerialTransportBase : ICLITransport
  {
    public string Port { get; set; }
    public int BaudRate { get; set; } = 115200;
    public int ResponseTimeoutMs { get; set; } = 2000;
  }

  public class CLISerialTransport : CLISerialTransportBase
  {
  }

  public class CLIModbusRtuTransport : CLISerialTransportBase
  {
  }

  // ===== TCP =====

  public abstract class CLITcpTransportBase : ICLITransport
  {
    public string Host { get; set; }
    public int Port { get; set; }
    public int ConnectTimeoutMs { get; set; } = 2000;
    public int ResponseTimeoutMs { get; set; } = 2000;
  }

  public class CLIRawTcpTransport : CLITcpTransportBase
  {
    public CLIRawTcpTransport()
    {
      Port = 5000;
    }
  }

  public class CLIModbusTcpTransport : CLITcpTransportBase
  {
    public CLIModbusTcpTransport()
    {
      Port = 502;
    }
  }

  public class CLIModbusRtuOverTcpTransport : CLITcpTransportBase
  {
    public CLIModbusRtuOverTcpTransport()
    {
      Port = 502;
    }
  }

  // ===== UDP =====

  public abstract class CLIUdpTransportBase : ICLITransport
  {
    public string Host { get; set; }
    public int Port { get; set; }
    public int ResponseTimeoutMs { get; set; } = 2000;
  }

  public class CLIRawUdpTransport : CLIUdpTransportBase
  {
    public CLIRawUdpTransport()
    {
      Port = 5000;
    }
  }

  public class CLIModbusUdpTransport : CLIUdpTransportBase
  {
    public CLIModbusUdpTransport()
    {
      Port = 502;
    }
  }

  public class CLIModbusRtuOverUdpTransport : CLIUdpTransportBase
  {
    public CLIModbusRtuOverUdpTransport()
    {
      Port = 502;
    }
  }
}
