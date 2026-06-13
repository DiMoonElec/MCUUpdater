using PolyBootCore.MISC;
using System.Collections.Generic;

namespace PolyBootCore.UpdateFile
{
  public class FirmwareData
  {
    /// <summary>
    /// Версия прошивки файла обновления
    /// </summary>
    public FirmwareVersion FirmwareVersion { get; private set; }

    /// <summary>
    /// Версия протокола
    /// </summary>
    public FirmwareVersion ProtocolVersion { get; private set; }

    /// <summary>
    /// Версия GATEWAY-расширений
    /// </summary>
    public FirmwareVersion GatewayExtensionsVersion { get; private set; }

    /// <summary>
    /// DEVID-поле
    /// </summary>
    public string DevID { get; private set; }

    /// <summary>
    /// DESCR-поле
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Заголовок для BootloaderBegin()
    /// </summary>
    public string HeaderChunkBase64 { get; private set; }

    /// <summary>
    /// Прошивка
    /// </summary>
    public IReadOnlyList<string> DataChunksBase64 { get; private set; }
  }

  public enum UpdateFileType
  {
    SINGLE,
    COMPLEX
  };

  public sealed class FirmwareUpdateFileV2
  {
    /// <summary>
    /// Версия формата файла
    /// </summary>
    public int FormatVersion { get; private set; }

    /// <summary>
    /// Тип файла обновления
    /// </summary>
    public UpdateFileType UpdateFileType { get; private set; }

    /// <summary>
    /// Основной файл прошивки, единственный девайс или Gateway
    /// </summary>
    public FirmwareData MainFirmwareData { get; private set; }

    /// <summary>
    /// Если основной девайс это Gateway, то тут прошивки подчиненных устройств
    /// </summary>
    public IReadOnlyList<FirmwareData> SecondaryFirmwareData { get; private set; }
  }
}
