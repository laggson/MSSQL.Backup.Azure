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
      }

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         SqlHelper.Init(TbServer.Text, PbPassword.Password);
         CbDataTable.ItemsSource = SqlHelper.GetAllTables();
         CbDataTable.SelectedIndex = 0;
      }

      private void Button_Click_1(object sender, RoutedEventArgs e)
      {
      }

      private void SavePreferences()
      {
         var value = CbDataTable.SelectedValue as string;

         if (value == null)
         {
            MessageBox.Show(this, "Du musst zuerst eine Datenbank auswählen...");
         }
         else
         {
            var storageItem = new StorageItem
            {
               AzureAccount = TbAccountName.Text,
               AzureContainer = TbContainer.Text,
               Database = value,
               SqlServer = TbServer.Text,
               SqlPassword = PbPassword.Password,
               SasToken = TbToken.Text
            };
            storageItem.SaveAsJson();
         }
      }
   }
}
