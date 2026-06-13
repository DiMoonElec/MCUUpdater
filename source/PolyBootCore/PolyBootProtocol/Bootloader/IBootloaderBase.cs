namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal delegate void BootloaderErasureProgressDelegate(int numBlocks, int currentBlock);

  internal interface IBootloaderBase
  {
    /// <summary>
    /// Данное событие возникает во время выполнения функции BootloaderEraseUserData() после очистки каждого блока памяти
    /// </summary>
    event BootloaderErasureProgressDelegate BootloaderUserDataErasureProgress;

    /// <summary>
    /// Перевести загрузчик в активное состояние
    /// </summary>
    PolyBootActionResult BootloaderActivate();

    /// <summary>
    /// Выполняет загрузку чанка и его расшивровку в ОЗУ ПЛК
    /// </summary>
    PolyBootActionResult BootloaderSend(string frame);

    /// <summary>
    /// Данная команда выполняет запись ранее загруженного чанка из ОЗУ во flash-память
    /// </summary>
    PolyBootActionResult BootloaderWrite();

    /// <summary>
    /// Финализация процесса обновления прошивки
    /// </summary>
    PolyBootActionResult BootloaderEnd();

    /// <summary>
    /// Проверяет контрольную сумму пользовательского приложения
    /// </summary>
    /// <param name="Result">true - контрольная сумма сошлась</param>
    PolyBootActionResult BootloaderCheckApplicationCRC(out bool Result);

    /// <summary>
    /// Передает управление приложению
    /// </summary>
    PolyBootActionResult BootloaderApplicationRun();

    /// <summary>
    /// Очистить пользовательскую область памяти
    /// </summary>
    PolyBootActionResult BootloaderEraseUserData();
  }
}
