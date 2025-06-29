using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      UpdateError,
    }

    public class BootloaderWorkflow
    {
      IBootloaderProtocol Device;

      public event BootloaderEventDelegate EraseBegin;
      public event BootloaderEventDelegate EraseEnd;

      public event BootloaderEventDelegate UserDataEraseBegin;
      public event BootloaderEventDelegate UserDataEraseEnd;

      public event BootloaderEventDelegate UploadBegin;
      public event BootloaderEventDelegate UploadEnd;

      public event BootloaderProgressDelegate EraseProgress;
      public event BootloaderProgressDelegate UserDataEraseProgress;
      public event BootloaderProgressDelegate UploadProgress;


      public BootloaderWorkflow(IBootloaderProtocol dev)
      {
        Device = dev;
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

      /// <summary>
      /// Обновить прошивку
      /// </summary>
      /// <param name="data">данные обновления</param>
      public BootloaderWorkflowResult Update(string[] data)
      {
        int i;
        for (i = 0; i < 10; i++)
        {
          if (Device.BootloaderActivate() == BootloaderProtocolActionResult.OK)
            break;

          System.Threading.Thread.Sleep(100);
        }

        if (i == 10)
          return BootloaderWorkflowResult.ConnectionError;

        BootloaderProtocolActionResult result;

        //Очистка flash-памяти
        if (EraseBegin != null)
          EraseBegin();

        result = Device.BootloaderBegin();

        if (EraseEnd != null)
          EraseEnd();

        if (result != BootloaderProtocolActionResult.OK)
          return BootloaderWorkflowResult.ErasingError;

        //Отправка обновления
        if (UploadBegin != null)
          UploadBegin();

        for (int b = 0; b < data.Length; b++)
        {
          string u = data[b].Trim();

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
            int progress = (b * 100) / data.Length;
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
    }
  }
}
