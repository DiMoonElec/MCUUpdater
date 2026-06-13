using PolyBootCore.Transport;
using System;
using System.Collections.Generic;

namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal class BootloaderProtocolV1 : BootloaderBase, IBootloaderProtocolV1
  {
    const byte CMD_BOOTLOADER_BEGIN = 0x71;

    public event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;

    public BootloaderProtocolV1(IBootloaderTransport transport) : base(transport)
    {
    }

    public BootloaderProtocolActionResult BootloaderBegin(string header)
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_BEGIN);
      req.AddRange(Convert.FromBase64String(header));

      //Отправляем запрос
      var sendResult = Transport.Send(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      for (; ; )
      {
        //Ждем ответ
        var resp = Transport.Receive();

        //Проверяем ошибку таймаута ожедания ответа
        if (resp == null)
          return BootloaderProtocolActionResult.ConnectionLost;

        //Если тут вернули не то, то ожидаем, то выходим с ошибкой
        if (resp[0] != CMD_BOOTLOADER_BEGIN)
          return BootloaderProtocolActionResult.InternalError;

        //Если результат выполнения операции ОК, то выходим
        if (resp[1] == 0x00)
          return BootloaderProtocolActionResult.OK;

        //Если ошибка очистки
        if (resp[1] == 0x01)
          return BootloaderProtocolActionResult.Error;

        //Если идентификационный заголовок не совпал
        //то эта прошивка не подходит к данному устройству
        if (resp[1] == 0x02)
          return BootloaderProtocolActionResult.IncompatibleDeviceError;

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
