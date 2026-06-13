using PolyBootCore.PolyBootProtocol.Bootloader;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PolyBootCore.UpdateFile
{
  public enum UpdateFileType
  {
    Single,
    Complex,
  }

  /// <summary>
  /// Данные одной прошивки из файла обновления.
  /// Поля DeviceId, FirmwareVersion, BootloaderVersion, ProxyVersion
  /// заполнены только для FORMAT=2; для старых форматов равны null.
  /// </summary>
  public sealed class FirmwareData
  {
    /// <summary>
    /// Строковый идентификатор устройства (ID="...").
    /// Сверяется с результатом BootloaderGetDeviceID().
    /// Null для FORMAT=0/1.
    /// </summary>
    public string DeviceId { get; private set; }

    /// <summary>
    /// Версия прошивки из файла обновления (VER=x.y.z).
    /// Сверяется с результатом BootloaderGetFirmwareVersion().
    /// Null для FORMAT=0/1.
    /// </summary>
    public FirmwareVersion FirmwareVersion { get; private set; }

    /// <summary>
    /// Требуемая версия системы команд загрузчика (BOOTLOADER=n).
    /// Null для FORMAT=0/1.
    /// </summary>
    public int? BootloaderVersion { get; private set; }

    /// <summary>
    /// Требуемая версия прокси-протокола (PROXY=n).
    /// Null если устройство не поддерживает прокси или FORMAT < 2.
    /// </summary>
    public int? ProxyVersion { get; private set; }

    /// <summary>
    /// Заголовочный чанк для BootloaderBegin().
    /// Null для FORMAT=0.
    /// </summary>
    public string HeaderChunkBase64 { get; private set; }

    /// <summary>
    /// Чанки прошивки для BootloaderSend(). Всегда непустой список.
    /// </summary>
    public IReadOnlyList<string> DataChunksBase64 { get; private set; }

    // FORMAT=0/1
    public FirmwareData(
      string headerChunkBase64,
      IReadOnlyList<string> dataChunksBase64)
    {
      DeviceId = null;
      FirmwareVersion = null;
      BootloaderVersion = null;
      ProxyVersion = null;
      HeaderChunkBase64 = headerChunkBase64;
      DataChunksBase64 = dataChunksBase64;
    }

    // FORMAT=2
    public FirmwareData(
      string deviceId,
      FirmwareVersion firmwareVersion,
      int bootloaderVersion,
      int? proxyVersion,
      string headerChunkBase64,
      IReadOnlyList<string> dataChunksBase64)
    {
      DeviceId = deviceId;
      FirmwareVersion = firmwareVersion;
      BootloaderVersion = bootloaderVersion;
      ProxyVersion = proxyVersion;
      HeaderChunkBase64 = headerChunkBase64;
      DataChunksBase64 = dataChunksBase64;
    }
  }

  /// <summary>
  /// Контейнер файла обновления. Хранит все форматы (0, 1, 2).
  /// Workflow проверяет FormatVersion и использует только актуальные поля.
  /// </summary>
  public sealed class FirmwareUpdateFileV2
  {
    /// <summary>
    /// Версия формата файла: 0 (legacy), 1, 2.
    /// </summary>
    public int FormatVersion { get; private set; }

    /// <summary>
    /// Тип файла обновления. Для FORMAT=0/1 всегда Single.
    /// </summary>
    public UpdateFileType Type { get; private set; }

    /// <summary>
    /// Минимальная версия утилиты обновления (MIN_UPDATER=x.y.z).
    /// Null для FORMAT=0/1.
    /// </summary>
    public FirmwareVersion MinUpdaterVersion { get; private set; }

    /// <summary>
    /// Строки #REM из файла обновления, в порядке появления.
    /// Пустой список для FORMAT=0/1.
    /// </summary>
    public IReadOnlyList<string> Comments { get; private set; }

    /// <summary>
    /// Прошивка корневого устройства (ROOT или единственное устройство).
    /// Присутствует всегда.
    /// </summary>
    public FirmwareData RootFirmware { get; private set; }

    /// <summary>
    /// Прошивки слейвов. Пустой список для TYPE=Single.
    /// </summary>
    public IReadOnlyList<FirmwareData> SlaveFirmwares { get; private set; }

    // FORMAT=0/1
    public FirmwareUpdateFileV2(
      int formatVersion,
      FirmwareData rootFirmware)
    {
      FormatVersion = formatVersion;
      Type = UpdateFileType.Single;
      MinUpdaterVersion = null;
      Comments = new ReadOnlyCollection<string>(new List<string>());
      RootFirmware = rootFirmware;
      SlaveFirmwares = new ReadOnlyCollection<FirmwareData>(new List<FirmwareData>());
    }

    // FORMAT=2 SINGLE
    public FirmwareUpdateFileV2(
      FirmwareVersion minUpdaterVersion,
      IList<string> comments,
      FirmwareData rootFirmware)
    {
      FormatVersion = 2;
      Type = UpdateFileType.Single;
      MinUpdaterVersion = minUpdaterVersion;
      Comments = new ReadOnlyCollection<string>(new List<string>(comments));
      RootFirmware = rootFirmware;
      SlaveFirmwares = new ReadOnlyCollection<FirmwareData>(new List<FirmwareData>());
    }

    // FORMAT=2 COMPLEX
    public FirmwareUpdateFileV2(
      FirmwareVersion minUpdaterVersion,
      IList<string> comments,
      FirmwareData rootFirmware,
      IList<FirmwareData> slaveFirmwares)
    {
      FormatVersion = 2;
      Type = UpdateFileType.Complex;
      MinUpdaterVersion = minUpdaterVersion;
      Comments = new ReadOnlyCollection<string>(new List<string>(comments));
      RootFirmware = rootFirmware;
      SlaveFirmwares = new ReadOnlyCollection<FirmwareData>(new List<FirmwareData>(slaveFirmwares));
    }
  }
}