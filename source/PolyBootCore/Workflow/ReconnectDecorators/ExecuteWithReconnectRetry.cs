using PolyBootCore.PolyBootProtocol;
using System;

namespace PolyBootCore.Workflow
{
  internal static class ExecuteWithReconnectRetry
  {
    public static PolyBootActionResult Retry(
      Func<PolyBootActionResult> operation,
      Func<bool> Reconnect,
      Action Disconnect,
      int maxAttempts = 10,
      int delayMsOnReconnectFail = 1000)
    {
      if (operation == null) throw new ArgumentNullException(nameof(operation));
      if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
      if (delayMsOnReconnectFail < 0) throw new ArgumentOutOfRangeException(nameof(delayMsOnReconnectFail));

      // Первая попытка отправки запроса
      PolyBootActionResult result = operation();

      // Если результат не требует повторения — возвращаем его
      if (result != PolyBootActionResult.ConnectionLost)
        return result;

      // В результате первой попытки вызова operation()
      // получили ConnectionLost, поэтому запускаем
      // повтор попыток вызова operation() с предварительным
      // Reconnect()
      for (int attempt = 1; attempt < maxAttempts; attempt++)
      {
        if (Reconnect())
        {
          // В случае успешного переподключения
          // повторяем отправку команды
          result = operation();

          // Если результат не требует повторения — возвращаем его
          if (result != PolyBootActionResult.ConnectionLost)
            return result;
        }
        else
        {
          // Если не удалось переподключиться, то не отправляем команду,
          // однако, это все равно защитывается как попытка.
          // Пауза перед следующим переподключением
          System.Threading.Thread.Sleep(delayMsOnReconnectFail);
        }
      }

      // Если попали сюда, то все попытки повторной отправки команды
      // были исчерпаны, возвращаем ошибку потери связи
      Disconnect?.Invoke();
      return PolyBootActionResult.ConnectionLost;
    }

  }
}
