using System.Windows;

namespace MSSQL.Backup.Azure
{
   /// <summary>
   /// Does whatever a spider pig does!
   /// </summary>
   internal partial class MainWindow : Window
   {
      internal MainWindow(StorageItem item)
      {
         InitializeComponent();
         
            ItemToView(item);
      }

      /// <summary>
      /// Ist <paramref name="item"/> nicht null, werden die Werte des Objekts in die Controls des Windows übernommen.
      /// </summary>
      /// <param name="item"></param>
      private void ItemToView(StorageItem item)
      {
         if (item == null) return;

         TbAccountName.Text  = item.AzureAccount;
         TbContainer.Text    = item.AzureContainer;
         TbServer.Text       = item.SqlServer;
         TbToken.Text        = item.SasToken;
         PbPassword.Password = item.SqlPassword;

         TestSqlData();
         CbDataTable.SelectedValue = item.Database;
      }

      /// <summary>
      /// Erstellt ein <see cref="StorageItem"/> und speichert dieses in einer Textdatei.
      /// </summary>
      private void SavePreferencesAndCreateAgentJob()
      {
         var value = CbDataTable.SelectedValue as string;

         var storageItem = new StorageItem
         {
            AzureAccount = TbAccountName.Text,
            AzureContainer = TbContainer.Text,
            Database = value,
            SqlServer = TbServer.Text,
            SqlPassword = PbPassword.Password,
            SasToken = TbToken.Text
         };

         if (storageItem.IsValid())
         {
            storageItem.SaveAsJson();
            SqlHelper.CreateAgentJobs();

            MessageBox.Show(this, "Der Agent-Job wurde erfolgreich eingetragen.", "Erfolg",
               MessageBoxButton.OK, MessageBoxImage.Asterisk);
         }
         else
         {
            MessageBox.Show(this, "Du musst zuerst eine Datenbank auswählen...",
               "Ungültige Eingabe", MessageBoxButton.OK, MessageBoxImage.Exclamation);
         }
      }

      /// <summary>
      /// Prüft, ob die angegebenen SQL-Server Anmeldedaten gültig sind.
      /// </summary>
      private void TestSqlData()
      {
         SqlHelper.Init(TbServer.Text, PbPassword.Password);
         CbDataTable.ItemsSource = SqlHelper.GetAllTables();
         CbDataTable.SelectedIndex = 0;
      }

      #region Click-Events

      private void BtnCheckSql_OnClick(object sender, RoutedEventArgs e)
      {
         TestSqlData();
      }

      private void BtnCreateJob_OnClick(object sender, RoutedEventArgs e)
      {
         SavePreferencesAndCreateAgentJob();
      }

      private void BtnRestore_OnClick(object sender, RoutedEventArgs e)
      {
         // TODO: Logik o_O
      }

      #endregion
   }
}
