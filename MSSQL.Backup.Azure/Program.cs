using System;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         var storageItem = GetSavedAndInit();

         if (args.Length == 1 && storageItem != null)
         {
            // Entweder Voll- oder Log-Backup. Nie beides.
            if(args[0] == "-database")
            {
               SqlHelper.CreateBackupAndUpload(storageItem.Database);
            }
            else if(args[0] == "-log")
            {
               SqlHelper.CreateBackupAndUpload(storageItem.Database, true);
            }
         }
         else
         {
            // Fenster wird nur erstellt, wenn keine oder ungültige Argumente angegeben wurden.
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
         StorageItem item;

         try
         {
            item = StorageItem.ReadFromFile();

            SqlHelper.Init(item.SqlServer, item.SqlPassword);
            SqlHelper.VerbindungEinrichten(item.AzureAccount, item.AzureContainer, item.SasToken);

            return item;
         }
         catch(Exception)
         {
            return null;
         }
      }
   }
}
