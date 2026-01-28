using System.IO.Ports;

namespace MCUUpdaterGUI.UserControls
{
  static class MISC
  {
    static public string[] GetComPorts()
    {
      return SerialPort.GetPortNames();
    }
  }
}
