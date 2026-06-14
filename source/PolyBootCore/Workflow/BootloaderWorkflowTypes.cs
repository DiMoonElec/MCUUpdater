namespace PolyBootCore.Workflow
{
  public delegate void BootloaderEventDelegate();
  public delegate void BootloaderProgressDelegate(int percent);

  public enum BootloaderWorkflowResult
  {
    OK = 0,
    ConnectionError,
    ConnectionLost,
    ErasingError,
    IncompatibleDeviceError,
    UpdateError,
    Cancel,
  }

  public static class BootloaderWorkflowResultExtensions
  {
    public static string GetDescription(this BootloaderWorkflowResult result)
    {
      switch (result)
      {
        case BootloaderWorkflowResult.OK:
          return "Operation completed successfully";
        case BootloaderWorkflowResult.ConnectionError:
          return "Failed to connect to device";
        case BootloaderWorkflowResult.ConnectionLost:
          return "Connection to device lost";
        case BootloaderWorkflowResult.ErasingError:
          return "Error while erasing device memory";
        case BootloaderWorkflowResult.IncompatibleDeviceError:
          return "Device is incompatible";
        case BootloaderWorkflowResult.UpdateError:
          return "Firmware update failed";
        case BootloaderWorkflowResult.Cancel:
          return "Operation was canceled";
        default:
          return "Unknown result";
      }
    }
  }
}