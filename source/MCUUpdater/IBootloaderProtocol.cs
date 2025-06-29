using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCUUpdater
{
  public enum BootloaderProtocolActionResult
  {
    OK,
    Error,
    ConnectionLost,
    InternalError,
  }

  public delegate void BootloaderErasureProgressDelegate(int numBlocks, int currentBlock);

  internal interface IBootloaderProtocol
  {
    /// <summary>
    /// Данное событие возникает во время выполнения функции BootloaderBegin() после очистки каждого блока памяти
    /// </summary>
    event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;

    /// <summary>
    /// Данное событие возникает во время выполнения функции BootloaderEraseUserData() после очистки каждого блока памяти
    /// </summary>
    event BootloaderErasureProgressDelegate BootloaderUserDataErasureProgress;

    /// <summary>
    /// Перевести загрузчик в активное состояние
    /// </summary>
    BootloaderProtocolActionResult BootloaderActivate();

    /// <summary>
    /// Начало процесса обновления прошивки. Данная команда стирает старую прошивку
    /// </summary>
    BootloaderProtocolActionResult BootloaderBegin();

    /// <summary>
    /// Выполняет загрузку чанка и его расшивровку в ОЗУ ПЛК
    /// </summary>
    BootloaderProtocolActionResult BootloaderSend(string frame);

    /// <summary>
    /// Данная команда выполняет запись ранее загруженного чанка из ОЗУ во flash-память
    /// </summary>
    BootloaderProtocolActionResult BootloaderWrite();

    /// <summary>
    /// Финализация процесса обновления прошивки
    /// </summary>
    BootloaderProtocolActionResult BootloaderEnd();

    /// <summary>
    /// Проверяет контрольную сумму пользовательского приложения
    /// </summary>
    /// <param name="Result">true - контрольная сумма сошлась</param>
    BootloaderProtocolActionResult BootloaderCheckApplicationCRC(out bool Result);

    /// <summary>
    /// Передает управление приложению
    /// </summary>
    BootloaderProtocolActionResult BootloaderApplicationRun();

    /// <summary>
    /// Выполняет запись какой-либо информации в нестираемую облать памяти МК
    /// </summary>
    /// <param name="data">записываемый буфер, максимальный размер 128 байт</param>
    BootloaderProtocolActionResult BootloaderPermanentDataSet(byte[] data);

    /// <summary>
    /// Очистить пользовательскую область памяти
    /// </summary>
    BootloaderProtocolActionResult BootloaderEraseUserData();
  }
}
