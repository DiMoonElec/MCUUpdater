using System;
using System.Collections.Generic;

namespace MCUUpdater.Bootloader
{
  internal class BootloaderProtocol : IBootloaderProtocol
  {
    const byte CMD_BOOTLOADER_ACTIVATE = 0x70;
    const byte CMD_BOOTLOADER_BEGIN = 0x71;
    const byte CMD_BOOTLOADER_SEND = 0x72;
    const byte CMD_BOOTLOADER_WRITE = 0x73;
    const byte CMD_BOOTLOADER_END = 0x74;
    const byte CMD_BOOTLOADER_CHECK_CRC = 0x75;
    const byte CMD_BOOTLOADER_APP_RUN = 0x76;
    const byte CMD_BOOTLOADER_SET_PERMANENT_DATA = 0x77;
    const byte CMD_BOOTLOADER_ERASE_USER_DATA = 0x78;

    public event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;
    public event BootloaderErasureProgressDelegate BootloaderUserDataErasureProgress;

    private IBootloaderTransport Transport;

    public BootloaderProtocol(IBootloaderTransport transport)
    {
      Transport = transport;
    }

    public BootloaderProtocolActionResult BootloaderActivate()
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_ACTIVATE);
      req.AddRange(System.Text.Encoding.ASCII.GetBytes("ACTIVATE"));

      //Отправляем запрос
      var sendResult = Transport.Send(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var resp = Transport.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_ACTIVATE)
        return BootloaderProtocolActionResult.InternalError;

      //Если результат выполнения операции не ОК,
      //то выходим с ошибкой
      if (resp[1] != 0x00)
        return BootloaderProtocolActionResult.Error;

      return BootloaderProtocolActionResult.OK;
    }

    public BootloaderProtocolActionResult BootloaderBegin_V0()
    {
      //Отправляем запрос
      var sendResult = Transport.Send(new byte[] { CMD_BOOTLOADER_BEGIN });

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

    public BootloaderProtocolActionResult BootloaderBegin_V1(string header)
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

    public BootloaderProtocolActionResult BootloaderSend(string frame)
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_SEND);
      req.AddRange(Convert.FromBase64String(frame));

      //Отправляем запрос
      var sendResult = Transport.Send(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var resp = Transport.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_SEND)
        return BootloaderProtocolActionResult.InternalError;

      //Если результат выполнения операции не ОК,
      //то выходим с ошибкой
      if (resp[1] != 0x00)
        return BootloaderProtocolActionResult.Error;

      return BootloaderProtocolActionResult.OK;
    }

    public BootloaderProtocolActionResult BootloaderWrite()
    {
      //Отправляем запрос
      var sendResult = Transport.Send(new byte[] { CMD_BOOTLOADER_WRITE });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var resp = Transport.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_WRITE)
        return BootloaderProtocolActionResult.InternalError;

      //Если результат выполнения операции не ОК, то выходим
      if (resp[1] != 0x00)
        return BootloaderProtocolActionResult.Error;

      return BootloaderProtocolActionResult.OK;
    }

    public BootloaderProtocolActionResult BootloaderEnd()
    {
      //Отправляем запрос
      var sendResult = Transport.Send(new byte[] { CMD_BOOTLOADER_END });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var resp = Transport.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_END)
        return BootloaderProtocolActionResult.InternalError;

      //Если результат выполнения операции не ОК, то выходим
      if (resp[1] != 0x00)
        return BootloaderProtocolActionResult.Error;

      return BootloaderProtocolActionResult.OK;
    }

    public BootloaderProtocolActionResult BootloaderCheckApplicationCRC(out bool Result)
    {
      //Отправляем запрос
      var sendResult = Transport.Send(new byte[] { CMD_BOOTLOADER_CHECK_CRC });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
      {
        Result = false;
        return BootloaderProtocolActionResult.ConnectionLost;
      }

      //Ждем ответ
      var resp = Transport.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
      {
        Result = false;
        return BootloaderProtocolActionResult.ConnectionLost;
      }

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_CHECK_CRC)
      {
        Result = false;
        return BootloaderProtocolActionResult.InternalError;
      }


      if (resp[1] == 0x00)
      {
        Result = true;
        return BootloaderProtocolActionResult.OK;
      }

      if (resp[1] == 0x01)
      {
        Result = false;
        return BootloaderProtocolActionResult.OK;
      }

      Result = false;
      return BootloaderProtocolActionResult.Error;
    }

    public BootloaderProtocolActionResult BootloaderApplicationRun()
    {
      //Отправляем запрос
      var sendResult = Transport.Send(new byte[] { CMD_BOOTLOADER_APP_RUN });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var resp = Transport.Receive();

      //Проверяем ошибку таймаута ожедания ответа
      if (resp == null)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Если тут вернули не то, то ожидаем, то выходим с ошибкой
      if (resp[0] != CMD_BOOTLOADER_APP_RUN)
        return BootloaderProtocolActionResult.InternalError;

      //Если результат выполнения операции не ОК, то выходим
      if (resp[1] != 0x00)
        return BootloaderProtocolActionResult.Error;

      return BootloaderProtocolActionResult.OK;
    }

    public BootloaderProtocolActionResult BootloaderEraseUserData()
    {
      //Отправляем запрос
      var sendResult = Transport.Send(new byte[] { CMD_BOOTLOADER_ERASE_USER_DATA });

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
        if (resp[0] != CMD_BOOTLOADER_ERASE_USER_DATA)
          return BootloaderProtocolActionResult.InternalError;

        //Если результат выполнения операции ОК, то выходим
        if (resp[1] == 0x00)
          return BootloaderProtocolActionResult.OK;

        //Если ошибка очистки
        if (resp[1] == 0x01)
          return BootloaderProtocolActionResult.Error;

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

    public BootloaderProtocolActionResult BootloaderPermanentDataSet(byte[] data)
    {
      throw new NotImplementedException();
    }
  }
}
