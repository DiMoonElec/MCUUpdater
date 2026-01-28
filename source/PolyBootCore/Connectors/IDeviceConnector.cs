using System;

namespace PolyBootCore.Connectors
{

  /// <summary>
  /// Интерфейс транспортного потока обмена данными
  /// с Bootloader-ом.
  /// </summary>
  internal interface IDeviceConnector
  {
    int ReadTimeout { get; set; }
    int WriteTimeout { get; set; }

    /// <summary>
    /// Выполняет попытку инициализировать транспортный уровень канала связи
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
    /// Отправить массив в поток вывода.
    /// Может генерировать исключение 
    /// при возникновении ошибок потока передачи
    /// </summary>
    /// <param name="data">Полезные денные</param>
    void Write(byte[] data);

 
    /// <summary>
    /// Выполняет чтение из потока ввода.
    /// Может генерировать исключение 
    /// при возникновении ошибок потока приема
    /// </summary>
    /// <param name="buffer">Буфер, в который производится чтение</param>
    /// <param name="offset">Смещение в буфере</param>
    /// <param name="count">Количество байт для чтения</param>
    /// <returns>Количество прочитанных байт, если 0, то выход по тайм-ауту</returns>
    int Read(byte[] buffer, int offset, int count);

    /// <summary>
    /// Событие, содержащее информацию об конкретной ошибке 
    /// при попытке установить соединение с помощью Connect().
    /// Удобно использовать для отладочных целей.
    /// </summary>
    event EventHandler<string> ConnectionError;
  }
}
