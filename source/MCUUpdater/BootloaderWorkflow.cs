using System;
using MCUUpdater.Bootloader;
using MCUUpdater.Connectors;

namespace MCUUpdater
{
  internal class MCUUpdater
  {
    public delegate void BootloaderEventDelegate();
    public delegate void BootloaderProgressDelegate(int percent);

    public enum BootloaderWorkflowResult
    {
      OK = 0,
      ConnectionError,
      ErasingError,
      IncompatibleDeviceError,
      UpdateError,
    }

    public class BootloaderWorkflow
    {
      IBootloaderProtocol Device;
      IDeviceConnector Connector;

      public event BootloaderEventDelegate EraseBegin;
      public event BootloaderEventDelegate EraseEnd;

      public event BootloaderEventDelegate UserDataEraseBegin;
      public event BootloaderEventDelegate UserDataEraseEnd;

      public event BootloaderEventDelegate UploadBegin;
      public event BootloaderEventDelegate UploadEnd;

      public event BootloaderProgressDelegate EraseProgress;
      public event BootloaderProgressDelegate UserDataEraseProgress;
      public event BootloaderProgressDelegate UploadProgress;


      public BootloaderWorkflow(IBootloaderProtocol dev, IDeviceConnector connector)
      {
        Device = dev;
        Connector = connector;
        dev.BootloaderMemoryErasureProgress += bootloaderMemoryErasureProgress;
        dev.BootloaderUserDataErasureProgress += bootloaderUserDataErasureProgress;
      }

      private void bootloaderUserDataErasureProgress(int numBlocks, int currentBlock)
      {
        if (UserDataEraseProgress != null)
        {
          int progress = (currentBlock * 100) / numBlocks;
          UserDataEraseProgress(progress);
        }
      }

      private void bootloaderMemoryErasureProgress(int numBlocks, int currentBlock)
      {
        if (EraseProgress != null)
        {
          int progress = (currentBlock * 100) / numBlocks;
          EraseProgress(progress);
        }
      }

      public BootloaderWorkflowResult Update(FirmwareUpdateFile updateFile, int connectionTimeout)
      {
        if (updateFile.ProtocolVersion == 0)
          return UpdateProtocolVersion0(updateFile, connectionTimeout);
        else if (updateFile.ProtocolVersion == 1)
          return UpdateProtocolVersion1(updateFile, connectionTimeout);
        else
          throw new Exception($"Protocol version {updateFile.ProtocolVersion} is not supported.");
      }

      private BootloaderWorkflowResult UpdateProtocolVersion1(FirmwareUpdateFile updateFile, int connectionTimeout)
      {
        int connectionIterations = connectionTimeout * 2;
        int i;
        for (i = 0; i < connectionIterations; i++)
        {
          if (InitializeConnection())
            break;

          System.Threading.Thread.Sleep(500);
        }

        if (i == connectionIterations)
          return BootloaderWorkflowResult.ConnectionError;

        BootloaderProtocolActionResult result;

        //Очистка flash-памяти
        if (EraseBegin != null)
          EraseBegin();

        result = Device.BootloaderBegin_V1(updateFile.HeaderChunkBase64.Trim());

        if (EraseEnd != null)
          EraseEnd();

        if (result == BootloaderProtocolActionResult.IncompatibleDeviceError)
          return BootloaderWorkflowResult.IncompatibleDeviceError;
        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ErasingError;

        //Отправка обновления
        if (UploadBegin != null)
          UploadBegin();

        var dataChunks = updateFile.DataChunksBase64;

        for (int b = 0; b < dataChunks.Count; b++)
        {
          string u = dataChunks[b].Trim();

          if (u != "")
          {
            result = Device.BootloaderSend(u);
            if (result != BootloaderProtocolActionResult.OK)
              return BootloaderWorkflowResult.UpdateError;

            result = Device.BootloaderWrite();
            if (result != BootloaderProtocolActionResult.OK)
              return BootloaderWorkflowResult.UpdateError;
          }

          if (UploadProgress != null)
          {
            int progress = (b * 100) / dataChunks.Count;
            UploadProgress(progress);
          }
        }

        if (UploadEnd != null)
          UploadEnd();

        //Завершаем процесс обновления
        result = Device.BootloaderEnd();
        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ConnectionError;

        //Проверяем CRC прошивки
        bool crcOK;
        result = Device.BootloaderCheckApplicationCRC(out crcOK);
        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ConnectionError;
        if (crcOK == false)
          return BootloaderWorkflowResult.UpdateError;

        //Запускаем прошивку
        result = Device.BootloaderApplicationRun();
        if (result == BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.OK;
        else
          return BootloaderWorkflowResult.UpdateError;
      }

      private BootloaderWorkflowResult UpdateProtocolVersion0(FirmwareUpdateFile updateFile, int connectionTimeout)
      {
        int connectionIterations = connectionTimeout * 2;
        int i;
        for (i = 0; i < connectionIterations; i++)
        {
          if (InitializeConnection())
            break;

          System.Threading.Thread.Sleep(500);
        }

        if (i == connectionIterations)
          return BootloaderWorkflowResult.ConnectionError;

        BootloaderProtocolActionResult result;

        //Очистка flash-памяти
        if (EraseBegin != null)
          EraseBegin();

        result = Device.BootloaderBegin_V0();

        if (EraseEnd != null)
          EraseEnd();

        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ErasingError;

        //Отправка обновления
        if (UploadBegin != null)
          UploadBegin();

        var dataChunks = updateFile.DataChunksBase64;

        for (int b = 0; b < dataChunks.Count; b++)
        {
          string u = dataChunks[b].Trim();

          if (u != "")
          {
            result = Device.BootloaderSend(u);
            if (result != BootloaderProtocolActionResult.OK)
              return BootloaderWorkflowResult.UpdateError;

            result = Device.BootloaderWrite();
            if (result != BootloaderProtocolActionResult.OK)
              return BootloaderWorkflowResult.UpdateError;
          }

          if (UploadProgress != null)
          {
            int progress = (b * 100) / dataChunks.Count;
            UploadProgress(progress);
          }
        }

        if (UploadEnd != null)
          UploadEnd();

        //Завершаем процесс обновления
        result = Device.BootloaderEnd();
        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ConnectionError;

        //Проверяем CRC прошивки
        bool crcOK;
        result = Device.BootloaderCheckApplicationCRC(out crcOK);
        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ConnectionError;
        if (crcOK == false)
          return BootloaderWorkflowResult.UpdateError;

        //Запускаем прошивку
        result = Device.BootloaderApplicationRun();
        if (result == BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.OK;
        else
          return BootloaderWorkflowResult.UpdateError;
      }

      public BootloaderWorkflowResult EraseUserData()
      {
        if (UserDataEraseBegin != null)
          UserDataEraseBegin();

        var result = Device.BootloaderEraseUserData();

        if (UserDataEraseEnd != null)
          UserDataEraseEnd();

        if (result == BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.OK;

        return BootloaderWorkflowResult.ErasingError;
      }

      private bool InitializeConnection()
      {
        if (Connector.Connect() == false)
          return false;

        if (Device.BootloaderActivate() != BootloaderProtocolActionResult.OK)
        {
          Connector.Disconnect();
          return false;
        }

        return true;
      }


    }
  }
}
