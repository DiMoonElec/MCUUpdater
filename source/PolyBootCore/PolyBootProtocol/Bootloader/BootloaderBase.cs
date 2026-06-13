using PolyBootCore.Transport;
using System;
using System.Collections.Generic;

namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal abstract class BootloaderBase : IBootloaderBase
  {
    const byte CMD_BOOTLOADER_ACTIVATE = 0x70;
    const byte CMD_BOOTLOADER_SEND = 0x72;
    const byte CMD_BOOTLOADER_WRITE = 0x73;
    const byte CMD_BOOTLOADER_END = 0x74;
    const byte CMD_BOOTLOADER_CHECK_CRC = 0x75;
    const byte CMD_BOOTLOADER_APP_RUN = 0x76;
    const byte CMD_BOOTLOADER_SET_PERMANENT_DATA = 0x77;
    const byte CMD_BOOTLOADER_ERASE_USER_DATA = 0x78;

    public event BootloaderErasureProgressDelegate BootloaderUserDataErasureProgress;

    private protected IBootloaderTransportChannel TransportChannel;

    public BootloaderBase(IBootloaderTransportChannel transportChannel)
    {
      TransportChannel = transportChannel;
    }

    public PolyBootActionResult BootloaderActivate()
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_ACTIVATE);
      req.AddRange(System.Text.Encoding.ASCII.GetBytes("ACTIVATE"));

      //Отправляем запрос
      var sendResult = TransportChannel.Send(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      //Ждем ответ
      var resp = TransportChannel.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_ACTIVATE)
        return PolyBootActionResult.InternalError;

      //Если результат выполнения операции не ОК,
      //то выходим с ошибкой
      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      return PolyBootActionResult.OK;
    }

    public PolyBootActionResult BootloaderSend(string frame)
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_SEND);
      req.AddRange(Convert.FromBase64String(frame));

      //Отправляем запрос
      var sendResult = TransportChannel.Send(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      //Ждем ответ
      var resp = TransportChannel.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_SEND)
        return PolyBootActionResult.InternalError;

      //Если результат выполнения операции не ОК,
      //то выходим с ошибкой
      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      return PolyBootActionResult.OK;
    }

    public PolyBootActionResult BootloaderWrite()
    {
      //Отправляем запрос
      var sendResult = TransportChannel.Send(new byte[] { CMD_BOOTLOADER_WRITE });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      //Ждем ответ
      var resp = TransportChannel.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_WRITE)
        return PolyBootActionResult.InternalError;

      //Если результат выполнения операции не ОК, то выходим
      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      return PolyBootActionResult.OK;
    }

    public PolyBootActionResult BootloaderEnd()
    {
      //Отправляем запрос
      var sendResult = TransportChannel.Send(new byte[] { CMD_BOOTLOADER_END });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      //Ждем ответ
      var resp = TransportChannel.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_END)
        return PolyBootActionResult.InternalError;

      //Если результат выполнения операции не ОК, то выходим
      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      return PolyBootActionResult.OK;
    }

    public PolyBootActionResult BootloaderCheckApplicationCRC(out bool Result)
    {
      //Отправляем запрос
      var sendResult = TransportChannel.Send(new byte[] { CMD_BOOTLOADER_CHECK_CRC });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
      {
        Result = false;
        return PolyBootActionResult.ConnectionLost;
      }

      //Ждем ответ
      var resp = TransportChannel.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
      {
        Result = false;
        return PolyBootActionResult.ConnectionLost;
      }

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_CHECK_CRC)
      {
        Result = false;
        return PolyBootActionResult.InternalError;
      }


      if (resp[1] == 0x00)
      {
        Result = true;
        return PolyBootActionResult.OK;
      }

      if (resp[1] == 0x01)
      {
        Result = false;
        return PolyBootActionResult.OK;
      }

      Result = false;
      return PolyBootActionResult.Error;
    }

    public PolyBootActionResult BootloaderApplicationRun()
    {
      //Отправляем запрос
      var sendResult = TransportChannel.Send(new byte[] { CMD_BOOTLOADER_APP_RUN });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      //Ждем ответ
      var resp = TransportChannel.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_APP_RUN)
        return PolyBootActionResult.InternalError;

      //Если результат выполнения операции не ОК, то выходим
      if (resp[1] != 0x00)
        return PolyBootActionResult.Error;

      return PolyBootActionResult.OK;
    }

    public PolyBootActionResult BootloaderEraseUserData()
    {
      //Отправляем запрос
      var sendResult = TransportChannel.Send(new byte[] { CMD_BOOTLOADER_ERASE_USER_DATA });

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
        if (resp[0] != CMD_BOOTLOADER_ERASE_USER_DATA)
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
          if (BootloaderUserDataErasureProgress != null)
          {
            int numBlocks = BitConverter.ToInt32(resp, 2);
            int currentBlock = BitConverter.ToInt32(resp, 6);
            BootloaderUserDataErasureProgress(numBlocks, currentBlock);
          }
        }
      }
    }

  }
}
