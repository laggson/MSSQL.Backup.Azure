using Newtonsoft.Json;
using System;
using System.IO;

namespace MSSQL.Backup.Azure
{
   /// <summary>
   /// Verwaltet die Daten, die vom SQL-Server und der Azure-Library 
   /// benötigt werden und speichert diese in einer Textdatei.
   /// </summary>
   internal class StorageItem
   {
      private static readonly string FilePath =
         Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Wichtig_NichtLoeschen.txt";

      /// <summary>
      /// Der Name des SQL-Servers, z.B. 192.168.178.319
      /// </summary>
      public string SqlServer { get; set; }
      /// <summary>
      /// Das Passwort des SA-Benutzers
      /// </summary>
      public string SqlPassword { get; set; }
      /// <summary>
      /// Der Name der Datenbank, z.B. VivPEP
      /// </summary>
      public string Database { get; set; }
      /// <summary>
      /// Der Name des Azure-Accounts, also vor .blob.core.windows.net
      /// </summary>
      public string AzureAccount { get; set; }
      /// <summary>
      /// Der Name des Blob-Containers, in der URL hinten angehängt
      /// </summary>
      public string AzureContainer { get; set; }
      /// <summary>
      /// Der Zugriffsschlüssel, der im Azure-Portal generiert wird.
      /// </summary>
      public string AzureKey { get; set; }
      /// <summary>
      /// Das Zugriffs-Token, generiert im Azure-Portal
      /// </summary>
      public string SasToken { get; set; }

      /// <summary>
      /// Konvertiert das <see cref="StorageItem"/> in ein Format, das auf der Festplatte
      /// gespeichert werden kann.
      /// </summary>
      public void SaveAsJson()
      {
         string test = JsonConvert.SerializeObject(this).ToBase64();
         SaveFile(test);
      }

      /// <summary>
      /// Prüft, ob in allen Feldern des Objekts gültige Werte eingetragen sind.
      /// </summary>
      /// <returns></returns>
      public bool IsValid()
      {
         return !string.IsNullOrEmpty(SqlPassword)
                && !string.IsNullOrEmpty(SqlServer)
                && !string.IsNullOrEmpty(Database)
                && !string.IsNullOrEmpty(AzureAccount)
                && !string.IsNullOrEmpty(AzureContainer)
                && !string.IsNullOrEmpty(AzureKey)
                && !string.IsNullOrEmpty(SasToken);
      }

      public override string ToString()
      {
         return AzureAccount + ".blob.core.windows.net/" + AzureContainer;
      }

      /// <summary>
      /// Speichert den eingegebenen Text in einer Datei in LocalAppdata
      /// </summary>
      /// <param name="text">Der Text, der gespeichert wird</param>
      private void SaveFile(string text)
      {
         if (File.Exists(FilePath))
            File.Delete(FilePath);

         File.WriteAllText(FilePath, text);
      }

      /// <summary>
      /// Konvertiert den Inhalt einer Textdatei in ein <see cref="StorageItem"/> und 
      /// löst eine Ausnahme aus, falls die Datei nicht gefunden wurde oder ihr Inhalt ungültig ist.
      /// </summary>
      /// <exception cref="ArgumentException">Die Datei existiert nicht</exception>
      /// <exception cref="InvalidDataException">Der Inhalt der Date ist ungültig.</exception>
      /// <returns>Das <see cref="StorageItem"/>, das ausgelesen wurde</returns>
      public static StorageItem ReadFromFile()
      {
         if (!File.Exists(FilePath))
            throw new IOException("Die Datei '" + FilePath + "' wurde nicht gefunden");

         try
         {
            var fileText = File.ReadAllText(FilePath);
            var text = fileText.FromBase64();
            var item = JsonConvert.DeserializeObject<StorageItem>(text);

            return item;
         }
         catch (Exception e) when (e is JsonReaderException || e is FormatException)
         {
            throw new InvalidDataException("Der Inhalt der gefundenen Datei ist ungültig.");
         }
      }
   }
}
