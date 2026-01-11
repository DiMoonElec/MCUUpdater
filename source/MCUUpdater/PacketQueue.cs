using System.Collections.Concurrent;

namespace MCUUpdater
{
  internal class PacketQueue
  {
    private readonly BlockingCollection<byte[]> queue;

    public PacketQueue(int maxSize = 0)
    {
      if (maxSize > 0)
      {
        queue = new BlockingCollection<byte[]>(
            new ConcurrentQueue<byte[]>(),
            maxSize);
      }
      else
      {
        queue = new BlockingCollection<byte[]>(
            new ConcurrentQueue<byte[]>());
      }
    }

    /// <summary>
    /// Неблокирующая вставка.
    /// При переполнении удаляет старые элементы (FIFO),
    /// пока новый пакет не будет добавлен.
    /// </summary>
    public void Push(byte[] packet)
    {
      if (packet == null)
        return;

      // Пытаемся добавить; если переполнено — удаляем старые
      while (!queue.TryAdd(packet))
      {
        queue.TryTake(out _);
      }
    }

    /// <summary>
    /// Блокирующее извлечение с таймаутом.
    /// </summary>
    public byte[] Pop(int timeoutMs)
    {
      byte[] packet;
      if(queue.TryTake(out packet, timeoutMs))
        return packet;
      return null;
    }

    /// <summary>
    /// Очистка очереди.
    /// </summary>
    public void Clear()
    {
      while (queue.TryTake(out _)) { }
    }

    /// <summary>
    /// Текущее количество элементов.
    /// </summary>
    public int Count => queue.Count;
  }
}
