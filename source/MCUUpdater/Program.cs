using System;
using System.IO;
using System.Reflection;
using MCUUpdater.CLI;
using PolyBootCore;
using PolyBootCore.UpdateFile;

namespace MCUUpdater
{
  internal class Program
  {
    static int Main(string[] args)
    {
      try
      {
        CLIOptions options = CLIParser.Parse(args);

        if (options.ShowHelp)
        {
          DisplayHelp();
          return 0;
        }

        // ===== Normal execution =====
        return Run(options);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: " + ex.Message);
        Console.WriteLine("Use -h or --help to see usage information.");
        Console.WriteLine();
        return 1;
      }
    }

    private static int Run(CLIOptions options)
    {
      try
      {
        var command = options.Command;

        if (command is CLIUpdateCommand updateCommand)
          return RunUpdateCommand(options, updateCommand);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Fatal error: {ex.Message}");
      }
      return 1;
    }

    private static int RunUpdateCommand(CLIOptions options, CLIUpdateCommand updateCommand)
    {
      var connectionConfig = BootloaderConnectionTransportFactory.Create(updateCommand.Transport);
      connectionConfig.DeviceWaitTimeout = options.WaitTimeoutSec;

      BootloaderWorkflow bootloaderWorkflow = new BootloaderWorkflow(connectionConfig);

      bootloaderWorkflow.EraseProgress += Bootloader_EraseProgress;
      bootloaderWorkflow.UserDataEraseProgress += Bootloader_UserDataEraseProgress;
      bootloaderWorkflow.UploadProgress += Bootloader_UploadProgress;
      bootloaderWorkflow.UploadEnd += Bootloader_UploadEnd;

      BootloaderWorkflowResult result;

      var update_file = FirmwareUpdateParser.Parse(updateCommand.FirmwareFile);
      Console.WriteLine($"File loaded: Protocol Version {update_file.ProtocolVersion}, Format Version {update_file.FormatVersion}");
      Console.WriteLine("Starting firmware update...");
      result = bootloaderWorkflow.Update(update_file);
      Console.WriteLine();

      switch (result)
      {
        case BootloaderWorkflowResult.OK:
          Console.WriteLine("Operation completed successfully.");
          return 0;
        case BootloaderWorkflowResult.ConnectionError:
          Console.WriteLine("Connection error occurred.");
          return 1;
        case BootloaderWorkflowResult.ErasingError:
          Console.WriteLine("Erasing error occurred.");
          return 1;
        case BootloaderWorkflowResult.UpdateError:
          Console.WriteLine("Firmware update failed.");
          return 1;
        case BootloaderWorkflowResult.IncompatibleDeviceError:
          Console.WriteLine("The firmware is not compatible with this device.");
          return 1;
        case BootloaderWorkflowResult.ConnectionLost:
          Console.WriteLine("Connection lost.");
          return 1;
        default:
          Console.WriteLine("Unknown error.");
          return 1;
      }
    }

    private static void Bootloader_EraseProgress(int percent)
    {
      Console.Write($"\rErasing flash: {percent}%   ");
      if (percent == 100)
        Console.WriteLine("\nFlash erase completed.");
    }

    private static void Bootloader_UserDataEraseProgress(int percent)
    {
      Console.Write($"\rErasing user data: {percent}%   ");
      if (percent == 100)
        Console.WriteLine("\n User data erase completed.");
    }

    private static void Bootloader_UploadProgress(int percent)
    {
      DrawProgressBar(percent, 50, "Uploading");
    }

    private static void Bootloader_UploadEnd()
    {
      DrawProgressBar(100, 50, "Uploading");
      Console.WriteLine("\nUpload completed.");
    }

    private static void DrawProgressBar(int percent, int width, string label)
    {
      int filled = percent * width / 100;
      string bar = new string('#', filled) + new string('-', width - filled);
      Console.Write($"\r{label}: [{bar}] {percent}%");
    }

    private static void DisplayHelp()
    {
      // Получаем текущую сборку
      Assembly assembly = Assembly.GetExecutingAssembly();

      // Имя ресурса: <Namespace>.<Filename> (если файл в корне проекта)
      string resourceName = "MCUUpdater.CLI.Help.txt";

      // Открываем поток для чтения ресурса
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream == null)
        {
          Console.WriteLine("Ошибка: Ресурс справки не найден.");
          return;
        }

        // Читаем текст из потока
        using (StreamReader reader = new StreamReader(stream))
        {
          string helpText = reader.ReadToEnd();
          Console.WriteLine(helpText);
        }
      }
    }
  }
}
