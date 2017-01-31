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
      }

      // Das Zeug nicht per Knopf starten, sondern über Argumente
      // Bei Knopfdruck nur Agent-Dienst erstellen
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         CbDataTable.ItemsSource = SqlHelper.GetAllTables(TbServer.Text, PbPassword.Password);
         CbDataTable.SelectedIndex = 0;
      }

      private void Button_Click_1(object sender, RoutedEventArgs e)
      {
         SqlHelper.BackupDatabase(CbDataTable.SelectedValue as string);
      }

      private void CreateBakAndUpload()
      {
         var deploymentHelper = new DeploymentHelper("sv=2015-12-11&ss=b&srt=sco&sp=rwdlac&se=2018-01-30T19:38:57Z&st=2017-01-30T11:38:57Z&spr=https&sig=yQjjqs0APFJ8MS%2FGlkcJHOaCyH7NBZ0ZvXxKpuMlrjY%3D");
      }
   }
}
