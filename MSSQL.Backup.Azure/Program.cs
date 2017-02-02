using System;
using System.Data.SqlClient;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   internal static class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         AktionsAuswahl(args.Length > 0 ? args[0] : "");
      }

      /// <summary>
      /// Ruft anhand des angegebenen Arguments verschiedene Methoden auf und lässt eine Hinweisbox erscheinen, falls
      /// ein Fehler abgefangen wurde. Bei keinen oder ungültigen Argumenten wird ein <see cref="MainWindow"/> erstellt.
      /// </summary>
      /// <param name="arg"></param>
      private static void AktionsAuswahl(string arg)
      {
         bool hasExecuted = false;
         var storageItem = GetSavedAndInit();

         try
         {
            // Entweder Voll- oder Log-Backup. Nie beides.
            if (arg == "-database" ^ arg == "-log")
            {
               SqlHelper.CreateBackupAndUpload(storageItem.Database, arg == "-log");
               hasExecuted = true;

            }
            else if (arg == "-restore")
            {
               // TODO - hasExecuted nach Action
               throw new NotImplementedException();
            }

            if (hasExecuted)
               MessageBox.Show("Die " + (arg == "-database" ? "Komplett" : "Verlaufs") + "-Sicherung der Datenbank '"
                               + storageItem.Database + "' wurde erfolgreich abgeschlossen.", "SQL-Backup erfolgreich",
                  MessageBoxButton.OK, MessageBoxImage.Asterisk);
         }
         catch (SqlException e)
         {
               MessageBox.Show("Es ist ein Fehler aufgetreten. Weitere Informationen:\r\n"
                               + e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
         }
         finally
         {
            if (!hasExecuted)
               new Application().Run(new MainWindow(storageItem));
         }
      }

      /// <summary>
      /// Sucht nach einer gespeicherten Datei mit Einstellungen und liest diese aus.
      /// Bei einem Fehler wird null zurück gegeben.
      /// </summary>
      /// <returns></returns>
      private static StorageItem GetSavedAndInit()
      {
         try
         {
            var item = StorageItem.ReadFromFile();

            SqlHelper.Init(item.SqlServer, item.SqlPassword);
            SqlHelper.VerbindungEinrichten(item.AzureAccount, item.AzureContainer, item.SasToken);

            return item;
         }
         catch (Exception)
         {
            return null;
         }
      }
   }
}
