namespace PolyBootCore.Bootloader
{
  internal enum BootloaderProtocolActionResult
  {
    OK,
    Error,
    IncompatibleDeviceError,
    ConnectionLost,
    InternalError,
  }

  internal delegate void BootloaderErasureProgressDelegate(int numBlocks, int currentBlock);

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

    bool Connect();

    void Disconnect();

    /// <summary>
    /// Перевести загрузчик в активное состояние
    /// </summary>
    BootloaderProtocolActionResult BootloaderActivate();

    /// <summary>
    /// Начало процесса обновления прошивки для протокола обмена версии 0. 
    /// Данная команда стирает старую прошивку
    /// </summary>
    BootloaderProtocolActionResult BootloaderBegin_V0();

    /// <summary>
    /// Начало процесса обновления прошивки для протокола обмена версии 1. 
    /// Данная команда стирает старую прошивку
    /// </summary>
    /// <param name="header">Header Chunk</param>
    BootloaderProtocolActionResult BootloaderBegin_V1(string header);

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
