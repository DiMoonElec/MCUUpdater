using System;
using System.Collections.Generic;
using System.Threading;
using MCUUpdater.Connectors;

namespace MCUUpdater
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


    static AutoResetEvent respEvent = new AutoResetEvent(false);
    static byte[] resp;
    protected IDeviceConnector DeviceConnector;

    public event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;
    public event BootloaderErasureProgressDelegate BootloaderUserDataErasureProgress;

    public BootloaderProtocol(IDeviceConnector deviceConnector)
    {
      DeviceConnector = deviceConnector;
      DeviceConnector.DataReceived += DeviceConnector_DataReceived;
    }

    public BootloaderProtocolActionResult BootloaderActivate()
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_ACTIVATE);
      req.AddRange(System.Text.Encoding.ASCII.GetBytes("ACTIVATE"));

      //Отправляем запрос
      var sendResult = SendReq(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var respResult = WaitResp(500);

      //Проверяем ошибку таймаута ожедания ответа
      if (respResult == false)
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

    public BootloaderProtocolActionResult BootloaderBegin()
    {
      //Отправляем запрос
      var sendResult = SendReq(new byte[] { CMD_BOOTLOADER_BEGIN });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      for (; ; )
      {
        //Ждем ответ
        var respResult = WaitResp(500);

        //Проверяем ошибку таймаута ожедания ответа
        if (respResult == false)
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

    public BootloaderProtocolActionResult BootloaderSend(string frame)
    {
      //Формируем запрос
      List<byte> req = new List<byte>();
      req.Add(CMD_BOOTLOADER_SEND);
      req.AddRange(Convert.FromBase64String(frame));

      //Отправляем запрос
      var sendResult = SendReq(req.ToArray());

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var respResult = WaitResp(500);

      //Проверяем ошибку таймаута ожедания ответа
      if (respResult == false)
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
      var sendResult = SendReq(new byte[] { CMD_BOOTLOADER_WRITE });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var respResult = WaitResp(500);

      //Проверяем ошибку таймаута ожедания ответа
      if (respResult == false)
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
      var sendResult = SendReq(new byte[] { CMD_BOOTLOADER_END });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var respResult = WaitResp(500);

      //Проверяем ошибку таймаута ожедания ответа
      if (respResult == false)
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
      var sendResult = SendReq(new byte[] { CMD_BOOTLOADER_CHECK_CRC });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
      {
        Result = false;
        return BootloaderProtocolActionResult.ConnectionLost;
      }

      //Ждем ответ
      var respResult = WaitResp(500);

      //Проверяем ошибку таймаута ожедания ответа
      if (respResult == false)
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
      var sendResult = SendReq(new byte[] { CMD_BOOTLOADER_APP_RUN });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      //Ждем ответ
      var respResult = WaitResp(500);

      //Проверяем ошибку таймаута ожедания ответа
      if (respResult == false)
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
      var sendResult = SendReq(new byte[] { CMD_BOOTLOADER_ERASE_USER_DATA });

      //Если возникла ошибка  во время отправки, то выходим
      if (sendResult == false)
        return BootloaderProtocolActionResult.ConnectionLost;

      for (; ; )
      {
        //Ждем ответ
        var respResult = WaitResp(500);

        //Проверяем ошибку таймаута ожедания ответа
        if (respResult == false)
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

    #region Вспомогательные методы

    private void DeviceConnector_DataReceived(object sender, byte[] e)
    {
      resp = e;
      respEvent.Set();
    }

    bool WaitResp(int timeout)
    {
      return respEvent.WaitOne(timeout);
    }

    bool SendReq(byte[] req)
    {
      if (DeviceConnector.IsConnected() == false)
        return false;

      respEvent.Reset();
      DeviceConnector.Write(req);
      return true;
    }

    #endregion
  }
}
