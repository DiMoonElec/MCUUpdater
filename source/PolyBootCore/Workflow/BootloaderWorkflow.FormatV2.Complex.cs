using PolyBootCore.PolyBootProtocol;
using PolyBootCore.PolyBootProtocol.Bootloader;
using PolyBootCore.PolyBootProtocol.Proxy;
using PolyBootCore.UpdateFile;
using System.Collections.Generic;

namespace PolyBootCore.Workflow
{
  public partial class BootloaderWorkflow
  {
    private BootloaderWorkflowResult UpdateFormatV2Complex(FirmwareUpdateFileV2 updateFile, bool forceUpdate)
    {
      BootloaderWorkflowResult result;
      // Все секции COMPLEX обязаны использовать протокол V2+
      if (!ValidateComplexVersions(updateFile))
        return BootloaderWorkflowResult.IncompatibleDeviceError;

      // Создаем экземпляр протокола обмена для корневого устройства (мастера)
      var bootloader = new ReconnectBootloaderProtocolV2(Transport, Transport);

      // Выполняем подключение к корневому устройству
      if (!WaitForConnection(bootloader))
        return BootloaderWorkflowResult.ConnectionError;

      // Обновляем мастер
      result = UpdateMaster(bootloader, updateFile.RootFirmware, forceUpdate);
      if (result != BootloaderWorkflowResult.OK)
        return result;

      // Инициализируем подсистему прокси мастера
      var proxy = new ReconnectProxyProtocolV1(Transport, Transport);
      result = ProxyInitialize(proxy);
      if (result != BootloaderWorkflowResult.OK)
        return result;

      // Получаем список id слейвов, которые есть у мастера
      byte[] slaveIds;
      result = ProxyGetListSlaves(proxy, out slaveIds);
      if (result != BootloaderWorkflowResult.OK)
        return result;

      // Создаем протокол обмена со слейвами, инкапсулированный в прокси
      // поток данных будет направляться через proxy,
      // а управление переподключением транспорта будет идти
      // через физический интерфейс Transport
      var proxyBootloader = new ReconnectBootloaderProtocolV2(proxy, Transport);

      // Получаем строковые идентификаторы всех слейвов
      Dictionary<byte, string> slaveIdentities = null;
      result = QuerySlaveIdentities(proxyBootloader, proxy, slaveIds, out slaveIdentities);
      if (result != BootloaderWorkflowResult.OK)
        return result;

      // ToDo: распечатка всех найденных слейвов пользователю в консоль

      // Обновляем слейвы
      result = UpdateSlaves(proxyBootloader, proxy,
        slaveIdentities, updateFile.SlaveFirmwares,
        forceUpdate);

      if(result != BootloaderWorkflowResult.OK)
        return result;

      // Запускаем основную прошивку мастера
      result = RunApplication(bootloader);

      return result;
    }

    private bool ValidateComplexVersions(FirmwareUpdateFileV2 updateFile)
    {
      if (updateFile.RootFirmware.BootloaderVersion < 2)
        return false;

      foreach (var slave in updateFile.SlaveFirmwares)
        if (slave.BootloaderVersion < 2)
          return false;

      return true;
    }

    /// <summary>
    /// Обновление прошивки мастера 
    /// без запуска основной прошивки после обновления
    /// </summary>
    private BootloaderWorkflowResult UpdateMaster(IBootloaderProtocolV2 bootloader,
      FirmwareData firmware, bool forceUpdate)
    {
      BootloaderWorkflowResult result;

      var checkResult = CheckDeviceIdentity(bootloader, firmware);
      if (checkResult != BootloaderWorkflowResult.OK)
        return checkResult;

      if (forceUpdate == false)
      {
        bool alreadyFlashed;

        result = IsVersionAlreadyFlashed(bootloader, firmware, out alreadyFlashed);
        if (result != BootloaderWorkflowResult.OK)
          return result;

        // Если актуальная версия прошивки уже зашита в мастер,
        // то пропускаем процесс перепрошивки, при этом
        // не передаем управление основному приложению
        if (alreadyFlashed == true)
          return BootloaderWorkflowResult.OK;
      }

      bootloader.BootloaderMemoryErasureProgress += OnMemoryErasureProgress;
      result = FlashDevice(bootloader, firmware);
      bootloader.BootloaderMemoryErasureProgress -= OnMemoryErasureProgress;

      return result;
    }

    private BootloaderWorkflowResult ProxyGetListSlaves(ReconnectProxyProtocolV1 proxy, out byte[] slaveIds)
    {
      slaveIds = null;
      var result = proxy.ProxyGetSlaveList(out slaveIds);

      if (result == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;
      else if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.UpdateError;
      else if (slaveIds == null)
        return BootloaderWorkflowResult.UpdateError;

      return BootloaderWorkflowResult.OK;
    }

    private BootloaderWorkflowResult QuerySlaveIdentities(IBootloaderProtocolV2 bootloader,
      IProxyProtocolV1 proxy,
      byte[] slaveIds,
      out Dictionary<byte, string> slaveIdentities)
    {
      slaveIdentities = new Dictionary<byte, string>();

      for (int i = 0; i < slaveIds.Length; i++)
      {
        byte slaveId = slaveIds[i];

        // Заранее добавляем с null — если что-то пойдёт не так, останется null
        slaveIdentities[slaveId] = null;

        var openResult = proxy.ProxyOpen(slaveId);
        if (openResult != PolyBootActionResult.OK)
        {
          // Потеря связи с мастером — дальше продолжать бессмысленно
          return BootloaderWorkflowResult.ConnectionLost;
        }

        string deviceId;
        var idResult = bootloader.BootloaderGetDeviceID(out deviceId);
        if (idResult != PolyBootActionResult.OK)
        {
          // Слейв не ответил
          continue;
        }

        slaveIdentities[slaveId] = deviceId;
      }

      // После опроса всех слейвов закрываем канал прокси
      var closeResult = proxy.ProxyClose();
      if (closeResult == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;

      return BootloaderWorkflowResult.OK;
    }

    private BootloaderWorkflowResult ProxyInitialize(IProxyProtocolV1 proxy)
    {
      var result = proxy.ProxyInitSlaves();
      if (result != PolyBootActionResult.OK)
        return BootloaderWorkflowResult.ConnectionError;

      return BootloaderWorkflowResult.OK;
    }

    private BootloaderWorkflowResult UpdateSlaves(
      IBootloaderProtocolV2 proxyBootloader, IProxyProtocolV1 proxy,
      Dictionary<byte, string> slaveIdentities,
      IReadOnlyList<FirmwareData> slaveFirmwares,
      bool forceUpdate)
    {
      foreach (var kvp in slaveIdentities)
      {
        byte slaveId = kvp.Key;
        string slaveDeviceId = kvp.Value;

        // Слейв не ответил при опросе — пропускаем
        if (slaveDeviceId == null)
          continue;

        // Ищем прошивку для этого слейва по строковому идентификатору
        FirmwareData firmware = null;
        foreach (var f in slaveFirmwares)
        {
          if (f.DeviceId == slaveDeviceId)
          {
            firmware = f;
            break;
          }
        }

        // Прошивки для этого слейва в файле нет — пропускаем
        if (firmware == null)
        {
          // ToDo: выводить сообщение в консоль
          continue;
        }

        // Переключаем прокси на данный слейв
        var openResult = proxy.ProxyOpen(slaveId);
        if (openResult != PolyBootActionResult.OK)
          return BootloaderWorkflowResult.ConnectionLost;

        var slaveResult = UpdateSlave(proxyBootloader, firmware, forceUpdate);

        if (slaveResult != BootloaderWorkflowResult.OK)
        {
          // ToDo: вывод ошибки в консоль
          // слейв отвалился в процессе, переходим к следующему
        }
      }

      var closeResult = proxy.ProxyClose();
      if (closeResult == PolyBootActionResult.ConnectionLost)
        return BootloaderWorkflowResult.ConnectionLost;

      return BootloaderWorkflowResult.OK;
    }

    private BootloaderWorkflowResult UpdateSlave(IBootloaderProtocolV2 bootloader, FirmwareData firmware, bool forceUpdate)
    {
      BootloaderWorkflowResult result;

      var checkResult = CheckDeviceIdentity(bootloader, firmware);
      if (checkResult != BootloaderWorkflowResult.OK)
        return checkResult;

      if (forceUpdate == false)
      {
        bool alreadyFlashed;

        result = IsVersionAlreadyFlashed(bootloader, firmware, out alreadyFlashed);
        if (result != BootloaderWorkflowResult.OK)
          return result;

        // Если в девайс уже прошита актуальная версия, то пропускаем прошивку,
        // и сразу запускаем основное приложение
        if (alreadyFlashed == true)
          return RunApplication(bootloader);
      }

      bootloader.BootloaderMemoryErasureProgress += OnMemoryErasureProgress;
      result = FlashDevice(bootloader, firmware);
      bootloader.BootloaderMemoryErasureProgress -= OnMemoryErasureProgress;

      if (result != BootloaderWorkflowResult.OK)
        return result;

      // Запускаем прошивку слейва
      result = RunApplication(bootloader);

      return result;
    }
  }
}
