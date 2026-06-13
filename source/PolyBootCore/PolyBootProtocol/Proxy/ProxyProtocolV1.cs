using PolyBootCore.Transport;
using System;
using System.Collections.Generic;

namespace PolyBootCore.PolyBootProtocol.Proxy
{
  internal class ProxyProtocolV1 : IProxyProtocolV1
  {
    // Команды управления прокси (обрабатываются самим мастером)
    const byte CMD_PROXY_INIT_SLAVES = 0xA0;
    const byte CMD_PROXY_GET_SLAVE_LIST = 0xA1;
    const byte CMD_PROXY_OPEN = 0xA2;
    const byte CMD_PROXY_CLOSE = 0xA3;

    // Инкапсуляция данных для слейва / от слейва
    const byte CMD_PROXY_DATA = 0xA4;

    private protected IBootloaderTransportChannel TransportChannel;

    public ProxyProtocolV1(IBootloaderTransportChannel transportChannel)
    {
      TransportChannel = transportChannel;
    }

    /// <summary>
    /// Инициализация подсистемы слейвов.
    /// Мастер присылает keep-alive пакеты (статус 0xFF) пока идёт инициализация,
    /// затем финальный пакет со статусом 0x00 или 0x01.
    /// </summary>
    public PolyBootActionResult ProxyInitSlaves()
    {
      var sendResult = TransportChannel.Send(new byte[] { CMD_PROXY_INIT_SLAVES });

      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      for (; ; )
      {
        var resp = TransportChannel.Receive();

        if (resp == null)
          return PolyBootActionResult.ConnectionLost;

        if (resp[0] != CMD_PROXY_INIT_SLAVES)
          return PolyBootActionResult.InternalError;

        // keep-alive пакет, инициализация ещё идёт, ждём следующий пакет
        if (resp[1] == 0xFF)
          continue;

        if (resp[1] == 0x00)
          return PolyBootActionResult.OK;

        return PolyBootActionResult.Error;

      }
    }

    /// <summary>
    /// Получить список идентификаторов доступных слейвов.
    /// Мастер разбивает ответ на пакеты если список не умещается в одну посылку:
    ///   [0xA1 | 0xFF | id0 | id1 | ...] — промежуточный пакет
    ///   [0xA1 | 0x00 | id0 | id1 | ...] — последний пакет
    /// </summary>
    public PolyBootActionResult ProxyGetSlaveList(out byte[] slaveIds)
    {
      slaveIds = null;

      var sendResult = TransportChannel.Send(new byte[] { CMD_PROXY_GET_SLAVE_LIST });

      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      var collected = new List<byte>();

      for (; ; )
      {
        var resp = TransportChannel.Receive();

        if (resp == null)
          return PolyBootActionResult.ConnectionLost;

        if (resp[0] != CMD_PROXY_GET_SLAVE_LIST)
          return PolyBootActionResult.InternalError;

        if (resp[1] == 0x01)
          return PolyBootActionResult.Error;

        bool isLast = (resp[1] == 0x00);
        bool isContinuation = (resp[1] == 0xFF);

        if (!isLast && !isContinuation)
          return PolyBootActionResult.InternalError;

        // Собираем идентификаторы из тела пакета (байты начиная с индекса 2)
        for (int i = 2; i < resp.Length; i++)
          collected.Add(resp[i]);

        if (isLast)
        {
          slaveIds = collected.ToArray();
          return PolyBootActionResult.OK;
        }
      }
    }

    /// <summary>
    /// Переключить канал мастера на выбранный слейв.
    /// </summary>
    public PolyBootActionResult ProxyOpen(byte slaveId)
    {
      var sendResult = TransportChannel.Send(new byte[] { CMD_PROXY_OPEN, slaveId });

      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      var resp = TransportChannel.Receive();

      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      if (resp[0] != CMD_PROXY_OPEN)
        return PolyBootActionResult.InternalError;

      if (resp[1] == 0x00)
        return PolyBootActionResult.OK;

      return PolyBootActionResult.Error;
    }

    /// <summary>
    /// Вернуть мастер в нормальный режим.
    /// Идемпотентна: безопасно вызывать из любого состояния мастера.
    /// </summary>
    public PolyBootActionResult ProxyClose()
    {
      var sendResult = TransportChannel.Send(new byte[] { CMD_PROXY_CLOSE });

      if (sendResult == false)
        return PolyBootActionResult.ConnectionLost;

      var resp = TransportChannel.Receive();

      if (resp == null)
        return PolyBootActionResult.ConnectionLost;

      if (resp[0] != CMD_PROXY_CLOSE)
        return PolyBootActionResult.InternalError;

      if (resp[1] == 0x00)
        return PolyBootActionResult.OK;

      return PolyBootActionResult.Error;
    }

    /// <summary>
    /// Инкапсулирует данные в CMD_PROXY_DATA и отправляет мастеру.
    /// Мастер пробрасывает payload слейву.
    /// Формат: [0xA4 | data...]
    /// </summary>
    public bool Send(byte[] data)
    {
      var frame = new byte[data.Length + 1];
      frame[0] = CMD_PROXY_DATA;
      data.CopyTo(frame, 1);
      return TransportChannel.Send(frame);
    }

    /// <summary>
    /// Получает пакет от мастера и разворачивает инкапсуляцию CMD_PROXY_DATA.
    /// Если первый байт не CMD_PROXY_DATA — это неожиданный пакет, возвращаем null.
    /// Верхний уровень (BootloaderProtocol) интерпретирует null как ConnectionLost.
    /// </summary>
    public byte[] Receive()
    {
      var resp = TransportChannel.Receive();

      if (resp == null)
        return null;

      // Ожидаем только CMD_PROXY_DATA пока открыт прокси-канал.
      // Любой другой байт — рассинхронизация протокола.
      if (resp[0] != CMD_PROXY_DATA)
        return null;

      // Убираем заголовочный байт, возвращаем чистый payload слейва
      var payload = new byte[resp.Length - 1];
      Array.Copy(resp, 1, payload, 0, payload.Length);
      return payload;
    }

  }
}
