using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;

namespace MCUUpdaterGUI.UserControls
{
  class SerialPortName
  {
    public string Name { get; private set; }
    public string Description { get; private set; }

    public SerialPortName(string name, string description)
    {
      Name = name;
      Description = description;
    }

    public override string ToString()
    {
      return Description;
    }
  }

  static class MISC
  {
    static public string[] GetComPorts()
    {
      return SerialPort.GetPortNames();
    }

    static public SerialPortName[] GetAvailablePorts()
    {
      var portsWithSescriptions = GetPortNames();
      var availablePorts = SerialPort.GetPortNames();

      List<SerialPortName> result = new List<SerialPortName>();

      foreach (var portName in availablePorts)
      {
        string portDescription = null;

        foreach (var p in portsWithSescriptions)
        {
          if (p.Key == portName)
          {
            portDescription = p.Value;
            break;
          }
        }

        if (portDescription == null)
          portDescription = portName;

        result.Add(new SerialPortName(portName, portDescription));
      }

      return result.ToArray();
    }

    public static List<KeyValuePair<string, string>> GetPortNames()
    {

      var result = new List<KeyValuePair<string, string>>();

      try
      {
        var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

        foreach (ManagementObject mo in searcher.Get())
        {
          string name = mo["Name"]?.ToString();
          if (string.IsNullOrWhiteSpace(name))
            continue;

          // вытаскиваем COMx
          var comMatch = Regex.Match(name, @"\(COM\d+\)");
          if (!comMatch.Success)
            continue;

          string comPort = comMatch.Value.Trim('(', ')');

          result.Add(new KeyValuePair<string, string>(comPort, name));
        }
      }
      catch
      {
      }

      return result;
    }

  }
}
