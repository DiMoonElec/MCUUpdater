using System;

namespace DiMoon.Protocols
{
  public class BinexLibReceiver
  {
    /// <summary>
    /// Событие приема пакета
    /// </summary>
    public event Action<byte[]> PackageReceivedEvent;

    bool flag_prev_rx_esc = false;
    int rxstate = 0;
    int cntr = 0;
    byte[] crc16 = new byte[2];

    byte[] rxbuff = null;
    int rxpack_size = 0;
    RxSymbolStatus r;

    readonly int RxPackMaxSize;

    public BinexLibReceiver(int rxPackMaxSize)
    {
      RxPackMaxSize = rxPackMaxSize;
    }

    enum RxSymbolStatus
    {
      SymbolEscape,
      SymbolStart,
      SymbolChar,
      SymbolInvalid,
    };

    public void Reset()
    {
      flag_prev_rx_esc = false;
      rxstate = 0;
      cntr = 0;
      rxbuff = null;
      rxpack_size = 0;
    }

    public bool Input(byte c)
    {
      switch (rxstate)
      {
        case 0:
          // Начало приема пакета
          // Ожидается символ начала пакета
          if (RXCharParce((byte)c) == RxSymbolStatus.SymbolStart)
            rxstate = 1;

          break;
        //////////////////////////////////////////////////
        case 1:
          /// Прием 1го байта заголовка пакета ///

          r = RXCharParce((byte)c);
          if (r == RxSymbolStatus.SymbolChar)
          {
            // приняли символ
            rxpack_size = (byte)c;
            rxstate = 2;
          }
          else if ((r == RxSymbolStatus.SymbolStart) || (r == RxSymbolStatus.SymbolInvalid))
          {
            // Если приняли неожиданное начало пакета
            // либо некорректную esc-последовательность
            rxstate = 0;
            break;
          }
          break;
        //////////////////////////////////////////////////
        case 2:
          /// Прием 2го байта заголовка пакета ///

          r = RXCharParce((byte)c);
          if (r == RxSymbolStatus.SymbolChar)
          {
            rxpack_size |= (((byte)c) << 8);
            cntr = 0;

            if (rxpack_size > RxPackMaxSize)
            {
              // Пакет слишком большой.
              // Это могло произойти по 2м причинам:
              // 1. пакет действительно слишком большой
              // 2. возникла ошибка при приеме размера пакета
              rxstate = 0;
              break;
            }
            else if (rxpack_size == 0) // Пустой пакет
            {
              rxbuff = new byte[0];
              // Состояние получения CRC16
              rxstate = 4;
              break;
            }

            rxbuff = new byte[rxpack_size];
            rxstate = 3;
          }
          else if ((r == RxSymbolStatus.SymbolStart) || (r == RxSymbolStatus.SymbolInvalid))
          {
            rxstate = 0;
            break;
          }
          break;
        //////////////////////////////////////////////////
        case 3:
          /// Прием тела пакета ///

          r = RXCharParce((byte)c);

          if (r == RxSymbolStatus.SymbolChar)
          {
            // приняли символ
            rxbuff[cntr++] = (byte)c;
            if (cntr == rxpack_size) // приняли весь пакет
            {
              // Если crc проверяем
              rxstate = 4; // прием и проверка CRC
            }
          }
          else if ((r == RxSymbolStatus.SymbolStart) || (r == RxSymbolStatus.SymbolInvalid))
          {
            rxstate = 0;
            break;
          }
          break;
        //////////////////////////////////////////////////
        case 4:
          /// Получение 1го байта crc ///

          r = RXCharParce((byte)c);

          if (r == RxSymbolStatus.SymbolChar)
          {
            crc16[0] = (byte)c;
            rxstate = 5;
          }
          else if ((r == RxSymbolStatus.SymbolStart) || (r == RxSymbolStatus.SymbolInvalid))
          {
            rxstate = 0;
            break;
          }
          break;
        //////////////////////////////////////////////////
        case 5:
          /// Получение 2го байта crc ///

          r = RXCharParce((byte)c);

          if (r == RxSymbolStatus.SymbolChar)
          {
            crc16[1] = (byte)c;

            // Вычисление crc для поля длины пакета
            // и поля полезных данных
            ushort crc = 0xFFFF;
            //crc = CalcCrc16((uint8_t*)((void*)(&rxpack_size)), 2, crc);
            crc = Crc.CalcCrc16(BitConverter.GetBytes((ushort)rxpack_size), crc);
            crc = Crc.CalcCrc16(rxbuff, crc);
            crc = Crc.CalcCrc16(crc16, crc);

            if (crc == 0)
            {
              rxstate = 0;
              PackageReceivedEvent?.Invoke(rxbuff);
              return true;
            }
            else
            {
              rxstate = 0;
              break;
            }
          }
          else if ((r == RxSymbolStatus.SymbolStart) || (r == RxSymbolStatus.SymbolInvalid))
          {
            rxstate = 0;
            break;
          }
          break;
          //////////////////////////////////////////////////
      }

      return false;
    }

    public byte[] GetReceiveData()
    {
      return rxbuff;
    }

    private RxSymbolStatus RXCharParce(byte c)
    {
      if (flag_prev_rx_esc)
      {
        // Если предыдущий символ является esc-символом
        // Сбрасываем флаг, так как
        // оба сценария развития событий
        // предусматирвают сброс этого флага
        flag_prev_rx_esc = false;

        // Если пришла экранированная последовательность
        if ((c == BinexLib.BinexEscSymbol) || (c == BinexLib.BinexStartSymbol))
          return RxSymbolStatus.SymbolChar;

        // Если предыдущее условие не выполнилось,
        // то получили некорректную последовательность
        return RxSymbolStatus.SymbolInvalid;
      }

      // Если попали сюда, то это означает, что
      // прошлый раз был принят не esc-символ
      if (c == BinexLib.BinexEscSymbol)
      {
        // Если приняли esc-символ,
        // то устанавливаем соответствующий флаг
        flag_prev_rx_esc = true;

        // Просто выходим
        return RxSymbolStatus.SymbolEscape;
      }

      // Если пришел стартовый символ
      if (c == BinexLib.BinexStartSymbol)
        return RxSymbolStatus.SymbolStart;

      // Если дошли сюда, то это означает, что приняли
      // обычный символ, возвращаем его
      return RxSymbolStatus.SymbolChar;
    }
  }
}
