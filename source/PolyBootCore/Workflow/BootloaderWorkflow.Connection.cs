using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using System.Threading;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    /// <summary>
    /// Ожидает подключения устройства и перевода загрузчика в активное состояние.
    /// Повторяет попытки каждые 500 мс в течение ConnectionTimeout секунд.
    /// </summary>
    private bool WaitForConnection(IBootloaderBase bootloader)
    {
      int iterations = ConnectionTimeout * 2;

      for (int i = 0; i < iterations; i++)
      {
        if (HasCancellation)
          CancellationToken.ThrowIfCancellationRequested();

        if (InitializeConnection(bootloader))
          return true;

        Thread.Sleep(500);
      }

      return false;
    }

    /// <summary>
    /// Выполняет одну попытку подключения и активации загрузчика.
    /// </summary>
    private bool InitializeConnection(IBootloaderBase bootloader)
    {
      if (Transport.Connect() == false)
        return false;

      // Вызов метода BootloaderActivate в обход механизма Reconnect
      if (bootloader.BootloaderActivate() != PolyBootActionResult.OK)
      {
        Transport.Disconnect();
        return false;
      }

      return true;
    }
  }
}