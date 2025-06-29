using System;
using System.IO;
using MCUUpdater.Connectors;
using static MCUUpdater.MCUUpdater;

namespace MCUUpdater
{
  internal class Program
  {
    private static int currentUploadProgress = 0;

    static void Main(string[] args)
    {
      if (args.Length < 3)
      {
        ShowUsage();
        return;
      }

      string port = null;
      int baudRate = 115200;
      string filePath = null;
      bool doUpdate = false;
      bool eraseUserData = false;

      // Parse arguments
      for (int i = 0; i < args.Length; i++)
      {
        switch (args[i])
        {
          case "--port":
            port = args[++i];
            break;
          case "--baud":
            if (!int.TryParse(args[++i], out baudRate))
            {
              Console.WriteLine("Invalid baud rate.");
              return;
            }
            break;
          case "--file":
            filePath = args[++i];
            break;
          case "--update":
            doUpdate = true;
            break;
          case "--erase-user":
            eraseUserData = true;
            break;
          default:
            Console.WriteLine($"Unknown argument: {args[i]}");
            ShowUsage();
            return;
        }
      }

      if (port == null)
      {
        Console.WriteLine("Serial port not specified.");
        ShowUsage();
        return;
      }

      if (doUpdate && filePath == null)
      {
        Console.WriteLine("Update mode requires a firmware file.");
        ShowUsage();
        return;
      }

      if (doUpdate && eraseUserData)
      {
        Console.WriteLine("Please choose only one operation: --update or --erase-user");
        ShowUsage();
        return;
      }

      try
      {
        Console.WriteLine($"Connecting to {port} at {baudRate} baud...");
        SerialPortConnector serialPortConnector = new SerialPortConnector();
        serialPortConnector.Open(port, baudRate);

        BootloaderProtocol bootloaderProtocol = new BootloaderProtocol(serialPortConnector);
        BootloaderWorkflow bootloader = new BootloaderWorkflow(bootloaderProtocol);

        bootloader.EraseProgress += Bootloader_EraseProgress;
        bootloader.UserDataEraseProgress += Bootloader_UserDataEraseProgress;
        bootloader.UploadProgress += Bootloader_UploadProgress;
        bootloader.UploadEnd += Bootloader_UploadEnd;

        BootloaderWorkflowResult result;

        if (doUpdate)
        {
          var lines = File.ReadAllLines(filePath);
          Console.WriteLine("Starting firmware update...");
          result = bootloader.Update(lines);
          Console.WriteLine();
        }
        else if (eraseUserData)
        {
          Console.WriteLine("Erasing user settings area...");
          result = bootloader.EraseUserData();
          Console.WriteLine("Done.\n");
        }
        else
        {
          Console.WriteLine("No operation specified.");
          ShowUsage();
          return;
        }

        switch (result)
        {
          case BootloaderWorkflowResult.OK:
            Console.WriteLine("Operation completed successfully.");
            break;
          case BootloaderWorkflowResult.ConnectionError:
            Console.WriteLine("Connection error occurred.");
            break;
          case BootloaderWorkflowResult.ErasingError:
            Console.WriteLine("Erasing error occurred.");
            break;
          case BootloaderWorkflowResult.UpdateError:
            Console.WriteLine("Firmware update failed.");
            break;
          default:
            Console.WriteLine("Unknown error.");
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Fatal error: {ex.Message}");
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
      currentUploadProgress = percent;
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

    private static void ShowUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("  MCUUpdater.exe --update --port COMx --baud 115200 --file firmware.xbin");
      Console.WriteLine("  MCUUpdater.exe --erase-user --port COMx --baud 115200");
      Console.WriteLine();
      Console.WriteLine("  --update        Perform firmware update");
      Console.WriteLine("  --erase-user    Erase user data flash area");
      Console.WriteLine("  --port          Serial COM port");
      Console.WriteLine("  --baud          Baud rate (e.g., 115200)");
      Console.WriteLine("  --file          Path to firmware update file");
    }
  }
}
