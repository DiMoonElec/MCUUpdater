using PolyBootCore.Transport;
using System;

namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal class BootloaderProtocolV0 : BootloaderBase, IBootloaderProtocolLegacy
  {
    const byte CMD_BOOTLOADER_BEGIN = 0x71;

    public event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;

    public BootloaderProtocolV0(IBootloaderTransportChannel transportChannel) : base(transportChannel)
    {
    }

    public PolyBootActionResult BootloaderBegin()
    {
      //Отправляем запрос
      var sendResult = TransportChannel.Send(new byte[] { CMD_BOOTLOADER_BEGIN });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      for (; ; )
      {
        //Ждем ответ
        var resp = TransportChannel.Receive();

        //Проверяем ошибку таймаута ожедания ответа
        if (resp == null)
          return PolyBootActionResult.ConnectionLost;

        //Если тут вернули не то, то ожидаем, то выходим с ошибкой
        if (resp[0] != CMD_BOOTLOADER_BEGIN)
          return PolyBootActionResult.InternalError;

        //Если результат выполнения операции ОК, то выходим
        if (resp[1] == 0x00)
          return PolyBootActionResult.OK;

        //Если ошибка очистки
        if (resp[1] == 0x01)
          return PolyBootActionResult.Error;

        //Если очистка в процессе
        if (resp[1] == 0xFF)
        {
          if (BootloaderMemoryErasureProgress != null)
          {
            int numBlocks = BitConverter.ToInt32(resp, 2);
            int currentBlock = BitConverter.ToInt32(resp, 6);
            BootloaderMemoryErasureProgress(numBlocks, currentBlock);
          }
        }
      }

    }

  }
}
