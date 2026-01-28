using System;

namespace MCUUpdater.CLI
{
  public static class CLIParser
  {
    public static CLIOptions Parse(string[] args)
    {
      var options = new CLIOptions();
      int i = 0;

      // ===== Global options =====
      while (i < args.Length && args[i].StartsWith("-"))
      {
        switch (args[i])
        {
          case "-h":
          case "--help":
            options.ShowHelp = true;
            return options; // <-- ВАЖНО: немедленный выход

          case "-w":
          case "--wait":
            options.WaitTimeoutSec = ParseInt(args, ref i);
            break;

          case "-v":
          case "--verbose":
            options.Verbose = true;
            i++;
            break;

          default:
            throw new ArgumentException($"Unknown global option: {args[i]}");
        }
      }

      if (i >= args.Length)
        throw new ArgumentException("Command is not specified");

      // ===== Command =====
      string commandName = args[i++];
      switch (commandName)
      {
        case "update":
          options.Command = ParseUpdateCommand(args, ref i);
          break;

        default:
          throw new ArgumentException($"Unknown command: {commandName}");
      }

      return options;
    }

    // ===== Update =====

    private static CLIUpdateCommand ParseUpdateCommand(string[] args, ref int i)
    {
      var cmd = new CLIUpdateCommand();

      // update options
      while (i < args.Length && args[i].StartsWith("-"))
      {
        switch (args[i])
        {
          case "-f":
          case "--file":
            cmd.FirmwareFile = ParseString(args, ref i);
            break;

          default:
            throw new ArgumentException($"Unknown update option: {args[i]}");
        }
      }

      if (string.IsNullOrEmpty(cmd.FirmwareFile))
        throw new ArgumentException("Firmware file (-f) is required");

      if (i >= args.Length)
        throw new ArgumentException("Transport is not specified");

      // transport
      string transportName = args[i++];
      cmd.Transport = ParseTransport(transportName, args, ref i);

      return cmd;
    }

    // ===== Transport =====

    private static ICLITransport ParseTransport(string name, string[] args, ref int i)
    {
      switch (name)
      {
        case "serial":
          return ParseSerial(new CLISerialTransport(), args, ref i);

        case "modbus-rtu":
          return ParseSerial(new CLIModbusRtuTransport(), args, ref i);

        case "raw-tcp":
          return ParseTcp(new CLIRawTcpTransport(), args, ref i);

        case "modbus-tcp":
          return ParseTcp(new CLIModbusTcpTransport(), args, ref i);

        case "modbus-rtu-over-tcp":
          return ParseTcp(new CLIModbusRtuOverTcpTransport(), args, ref i);

        case "raw-udp":
          return ParseUdp(new CLIRawUdpTransport(), args, ref i);

        case "modbus-udp":
          return ParseUdp(new CLIModbusUdpTransport(), args, ref i);

        case "modbus-rtu-over-udp":
          return ParseUdp(new CLIModbusRtuOverUdpTransport(), args, ref i);

        default:
          throw new ArgumentException($"Unknown transport: {name}");
      }
    }

    // ===== Serial =====

    private static ICLITransport ParseSerial(CLISerialTransportBase t, string[] args, ref int i)
    {
      while (i < args.Length && args[i].StartsWith("-"))
      {
        switch (args[i])
        {
          case "-p":
          case "--port":
            t.Port = ParseString(args, ref i);
            break;

          case "-b":
          case "--baud":
            t.BaudRate = ParseInt(args, ref i);
            break;

          case "-r":
          case "--response-timeout":
            t.ResponseTimeoutMs = ParseInt(args, ref i);
            break;

          default:
            return t;
        }
      }

      if (string.IsNullOrEmpty(t.Port))
        throw new ArgumentException("Serial port is required");

      return t;
    }

    // ===== TCP =====

    private static ICLITransport ParseTcp(CLITcpTransportBase t, string[] args, ref int i)
    {
      while (i < args.Length && args[i].StartsWith("-"))
      {
        switch (args[i])
        {
          case "-H":
          case "--host":
            t.Host = ParseString(args, ref i);
            break;

          case "-p":
          case "--port":
            t.Port = ParseInt(args, ref i);
            break;

          case "-t":
          case "--connect-timeout":
            t.ConnectTimeoutMs = ParseInt(args, ref i);
            break;

          case "-r":
          case "--response-timeout":
            t.ResponseTimeoutMs = ParseInt(args, ref i);
            break;

          default:
            return t;
        }
      }

      if (string.IsNullOrEmpty(t.Host))
        throw new ArgumentException("Host is required");

      return t;
    }

    // ===== UDP =====

    private static ICLITransport ParseUdp(CLIUdpTransportBase t, string[] args, ref int i)
    {
      while (i < args.Length && args[i].StartsWith("-"))
      {
        switch (args[i])
        {
          case "-H":
          case "--host":
            t.Host = ParseString(args, ref i);
            break;

          case "-p":
          case "--port":
            t.Port = ParseInt(args, ref i);
            break;

          case "-r":
          case "--response-timeout":
            t.ResponseTimeoutMs = ParseInt(args, ref i);
            break;

          default:
            return t;
        }
      }

      if (string.IsNullOrEmpty(t.Host))
        throw new ArgumentException("Host is required");

      return t;
    }

    // ===== Helpers =====

    private static int ParseInt(string[] args, ref int i)
    {
      // Current position i is the flag, so value is at i+1
      if (i + 1 >= args.Length)
        throw new ArgumentException($"Value expected after {args[i]}");

      i++; // move to value
      int val;
      if (!int.TryParse(args[i], out val))
        throw new ArgumentException($"Invalid integer value: {args[i]}");

      i++; // move past value
      return val;
    }

    private static string ParseString(string[] args, ref int i)
    {
      if (i + 1 >= args.Length)
        throw new ArgumentException($"Value expected after {args[i]}");

      i++; // move to value
      string val = args[i];
      i++; // move past value
      return val;
    }
  }
}
