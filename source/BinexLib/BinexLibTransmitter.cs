using System;
using System.Collections.Generic;

namespace DiMoon.Protocols
{
  public class BinexLibTransmitter
  {
    public byte[] BuildPackage(byte[] data)
    {
      List<byte> buff = new List<byte>();

      ushort crc = 0xFFFF;
      var len = BitConverter.GetBytes((ushort)data.Length);
      crc = Crc.CalcCrc16(len, crc);
      crc = Crc.CalcCrc16(data, crc);

      buff.Add(BinexLib.BinexStartSymbol);

      foreach (var b in len)
        putc(buff, b);

      foreach (var b in data)
        putc(buff, b);

      foreach (var b in BitConverter.GetBytes(crc))
        putc(buff, b);

      return buff.ToArray();
    }

    void putc(List<byte> buff, byte c)
    {
      if ((c == BinexLib.BinexStartSymbol) || (c == BinexLib.BinexEscSymbol))
        buff.Add(BinexLib.BinexEscSymbol);
      buff.Add(c);
    }
  }
}
