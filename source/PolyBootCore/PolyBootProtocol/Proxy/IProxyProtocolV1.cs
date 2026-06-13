using PolyBootCore.Transport;

namespace PolyBootCore.PolyBootProtocol.Proxy
{
  /// <summary>
  /// Протокол управления прокси-каналом к слейвам.
  /// Реализует IBootloaderTransport, притворяясь транспортом для вышестоящего
  /// BootloaderProtocol — все Send/Receive инкапсулируются в CMD_PROXY_DATA.
  /// </summary>
  internal interface IProxyProtocolV1 : IBootloaderTransportChannel
  {
    /// <summary>
    /// Инициализация подсистемы слейвов на стороне мастера.
    /// Выполняется долго; мастер присылает keep-alive (статус 0xFF) до завершения.
    /// </summary>
    PolyBootActionResult ProxyInitSlaves();

    /// <summary>
    /// Получить список идентификаторов доступных слейвов.
    /// Ответ может приходить несколькими пакетами (0xFF — продолжение, 0x00 — последний).
    /// </summary>
    PolyBootActionResult ProxyGetSlaveList(out byte[] slaveIds);

    /// <summary>
    /// Открыть прокси-канал к выбранному слейву.
    /// После успеха Send/Receive начинают инкапсулировать данные в CMD_PROXY_DATA.
    /// </summary>
    PolyBootActionResult ProxyOpen(byte slaveId);

    /// <summary>
    /// Закрыть прокси-канал, вернуть мастер в нормальный режим.
    /// Идемпотентна: безопасно вызывать из любого состояния мастера.
    /// </summary>
    PolyBootActionResult ProxyClose();
  }
}