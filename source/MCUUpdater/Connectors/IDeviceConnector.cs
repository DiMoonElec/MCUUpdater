using System;

namespace MCUUpdater.Connectors
{

  /// <summary>
  /// Интерфейс транспортного потока обмена данными
  /// с Bootloader-ом.
  /// </summary>
  internal interface IDeviceConnector
  {
    /// <summary>
    /// Выполняет попытку инициализировать канал связи, например, 
    /// для COM-порта успешно открыть порт, 
    /// для TCP успешно подключиться к серверу,
    /// для UDP успешно открыть UDP-соединение,
    /// и т.д.
    /// Тут не должен производиться какой-либо обмен данными с Bootloader-ом, 
    /// и реализация IDeviceConnector не должна заботиться 
    /// о фактическом наличии связи с Bootloader-ом.
    /// </summary>
    /// <returns>true - канал связи настроен, 
    /// false - не удалось настроить канал связи</returns>
    bool Connect();

    /// <summary>
    /// Закрыть канал связи, если он открыт.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Проверить состояние канала связи
    /// </summary>
    /// <returns>true - канал связи открыт, false - закрыт</returns>
    bool IsConnected();

    /// <summary>
    /// Отправить поток данных в Bootloader.
    /// </summary>
    /// <param name="data">Полезные денные</param>
    void Write(byte[] data);

    /// <summary>
    /// Были получены данные от Bootloader-а
    /// </summary>
    event EventHandler<byte[]> DataReceived;

    /// <summary>
    /// Событие, содержащее информацию об конкретной ошибке 
    /// при попытке установить соединение с помощью Connect().
    /// Удобно использовать для отладочных целей.
    /// </summary>
    event EventHandler<string> ConnectionError;
  }
}
