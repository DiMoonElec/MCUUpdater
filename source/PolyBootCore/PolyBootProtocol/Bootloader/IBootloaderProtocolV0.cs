namespace PolyBootCore.PolyBootProtocol.Bootloader
{
  internal interface IBootloaderProtocolV0 : IBootloaderBase
  {
    /// <summary>
    /// Данное событие возникает во время выполнения функции BootloaderBegin() после очистки каждого блока памяти
    /// </summary>
    event BootloaderErasureProgressDelegate BootloaderMemoryErasureProgress;

    PolyBootActionResult BootloaderBegin();
  }
}
