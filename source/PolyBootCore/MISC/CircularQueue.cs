using System.Collections.Generic;

namespace PolyBootCore.MISC
{
  internal class CircularQueue<T>
  {
    private readonly Queue<T> _queue;
    public int Capacity { get; }

    public CircularQueue(int capacity)
    {
      Capacity = capacity;
      _queue = new Queue<T>(capacity);
    }

    public void Enqueue(T item)
    {
      if (_queue.Count >= Capacity)
      {
        _queue.Dequeue(); // Удаляем самый старый элемент
      }
      _queue.Enqueue(item);
    }

    public T Dequeue() => _queue.Dequeue();
    public void Clear() => _queue.Clear();
    public int Count => _queue.Count;
  }

}
