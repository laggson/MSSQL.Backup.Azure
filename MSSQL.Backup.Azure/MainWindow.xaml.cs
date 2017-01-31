using System.Windows;

namespace MSSQL.Backup.Azure
{
   /// <summary>
   /// Does whatever a spider pig does!
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
         var item = StorageItem.ReadFromFile();

         TbToken.Text = // 31.01.17 15:19
            "sv=2015-12-11&ss=b&srt=sco&sp=rwdlac&se=2018-01-30T19:38:57Z&st=2017-01-30T11:38:57Z&spr=https&sig=yQjjqs0APFJ8MS%2FGlkcJHOaCyH7NBZ0ZvXxKpuMlrjY%3D";
      }

      // Das Zeug nicht per Knopf starten, sondern über Argumente
      // Bei Knopfdruck nur Agent-Dienst erstellen

      // THEORIE: Azure-SAS-Tokens sind erst nach 1-2 Stunden gültig
      // -> Muss ausprobiert werden. Ggf. Runterladen über Library
      //    nur zum Wiederherstellen nötig (-> "Datei wird verwendet")

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         CbDataTable.ItemsSource = SqlHelper.GetAllTables(TbServer.Text, PbPassword.Password);
         CbDataTable.SelectedIndex = 0;
      }

      private void Button_Click_1(object sender, RoutedEventArgs e)
      {
         var value = CbDataTable.SelectedValue as string;

         if (value == null)
         {
            MessageBox.Show(this, "Es wurde keine Datenbank ausgewählt x.x");
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
            //SqlHelper.Einrichten(TbAccountName.Text, TbContainer.Text, TbToken.Text);
            //SqlHelper.BackupDatabase(CbDataTable.SelectedValue as string);
         }

      }

      private void CreateBakAndUpload()
      {
         var deploymentHelper = new DeploymentHelper("sv=2015-12-11&ss=b&srt=sco&sp=rwdlac&se=2018-01-30T19:38:57Z&st=2017-01-30T11:38:57Z&spr=https&sig=yQjjqs0APFJ8MS%2FGlkcJHOaCyH7NBZ0ZvXxKpuMlrjY%3D");
      }
   }
}
