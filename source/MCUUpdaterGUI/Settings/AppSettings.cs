using System;
using System.IO;
using System.Xml.Serialization;

namespace MCUUpdaterGUI.Settings
{
  [Serializable]
  public class AppSettings
  {
    public TransportUISettings TransportUISettings { get; set; } = new TransportUISettings();

    public static void Save(AppSettings settings)
    {
      var dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MCUUpdater");

      Directory.CreateDirectory(dir);

      var filePath = Path.Combine(dir, "settings.xml");

      var serializer = new XmlSerializer(typeof(AppSettings));

      var tempFile = filePath + ".tmp";

      using (var fs = new FileStream(tempFile, FileMode.Create))
      {
        serializer.Serialize(fs, settings);
      }

      File.Copy(tempFile, filePath, true);
      File.Delete(tempFile);
    }

    public static AppSettings Load()
    {
      var dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MCUUpdater");

      var filePath = Path.Combine(dir, "settings.xml");

      if (!File.Exists(filePath))
        return new AppSettings();

      try
      {
        var serializer = new XmlSerializer(typeof(AppSettings));

        using (var fs = new FileStream(filePath, FileMode.Open))
        {
          return (AppSettings)serializer.Deserialize(fs);
        }
      }
      catch
      {
        return new AppSettings();
      }
    }
  }
}
