using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   /// <summary>
   /// Does whatever a spider pig does!
   /// </summary>
   internal partial class MainWindow
   {
      internal MainWindow(StorageItem item)
      {
         InitializeComponent();

         if (DateTimeFormatInfo.CurrentInfo != null)
         {
            CbWochentage.ItemsSource = GetLocalizedDayOfWeekValues(CultureInfo.CurrentCulture, DayOfWeek.Monday);
            CbWochentage.SelectedValue = DateTimeFormatInfo.CurrentInfo?.GetDayName(DateTime.Now.DayOfWeek);
         }

         ItemToView(item);
      }
      
      // NEUES TOKEN: Key 2, ab 14:34: sv=2015-12-11&ss=b&srt=c&sp=rwac&se=2018-02-02T22:32:07Z&st=2017-02-02T13:34:07Z&spr=https&sig=gX7%2BuOASt4PpB1kfSe%2BvBOKtjh7WEbQHODfLZfw2tLw%3D

      /// <summary>
      /// Gibt anhand der Kultur und des Wochenanfangs eine Liste mit lokalisierten Namen der Wochentage zurück
      /// </summary>
      /// <param name="culture">Die Kultur, deren Wochentage gesucht werden</param>
      /// <param name="startDay">Der erste Tag der Woche</param>
      /// <returns>Eine Liste aller Wochentage, abhängig von Kultur und Starttag</returns>
      private IEnumerable<string> GetLocalizedDayOfWeekValues(CultureInfo culture, DayOfWeek startDay)
      {
         string[] dayNames = culture.DateTimeFormat.DayNames;
         IEnumerable<string> query = dayNames
        .Skip((int) startDay)
        .Concat(
            dayNames.Take((int) startDay)
        );

         return query.ToList();
      }

      /// <summary>
      /// Ist <paramref name="item"/> nicht null, werden die Werte des Objekts in die Controls des Windows übernommen.
      /// </summary>
      /// <param name="item"></param>
      private void ItemToView(StorageItem item)
      {
         if (item == null) return;

         TbAccountName.Text = item.AzureAccount;
         TbContainer.Text = item.AzureContainer;
         TbKey.Text = item.AzureKey;
         TbServer.Text = item.SqlServer;
         TbToken.Text = item.SasToken;
         PbPassword.Password = item.SqlPassword;
         TbRestoreName.Text = item.Database;

         TestSqlData();
         CbDataTable.SelectedValue = item.Database;
      }

      private StorageItem ViewToItem(bool validate = true)
      {
         var value = CbDataTable.SelectedValue as string;

         var item = new StorageItem
         {
            AzureAccount    = TbAccountName.Text,
            AzureContainer  = TbContainer.Text,
            AzureKey        = TbKey.Text,
            Database        = value,
            SqlServer       = TbServer.Text,
            SqlPassword     = PbPassword.Password,
            SasToken        = TbToken.Text
         };

         if (!validate || item.IsValid())
            return item;
         return null;
      }

      /// <summary>
      /// Erstellt ein <see cref="StorageItem"/> und speichert dieses in einer Textdatei.
      /// </summary>
      private void CreateAgentJob()
      {
         if (ViewToItem() != null)
         {
            SqlHelper.CreateAgentJobs();
            SqlHelper.CreateAgentJobs(true);

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

      /// <summary>
      /// Lädt die Azure-Dateien für den ausgewählten Wochentag herunter und führt ein Datenbank-Restore durch.
      /// </summary>
      private void Rollback()
      {
         DeploymentHelper.Init(TbAccountName.Text, TbKey.Text);
         var tagFiles = DeploymentHelper.GetFileNames(TbContainer.Text, CbWochentage.SelectedValue as string);

         try
         {
            //var item = ViewToItem(false);

            //if (item == null)
            //{
            //   MessageBox.Show("Die angegebenen Daten sind ungültig. Passen Sie die Daten an und versuchen sie es" +
            //                   "erneut.", "Angaben ungültig.", MessageBoxButton.OK, MessageBoxImage.Error);
            //   return;
            //}

            SqlHelper.Rollback(TbRestoreName.Text, tagFiles.ToArray());

            MessageBox.Show("Die Wiederherstellung der Datenbank " + TbRestoreName.Text + " war erfolgreich.",
               "Rollback erfolgreich", MessageBoxButton.OK, MessageBoxImage.Asterisk);
         }
         catch (SqlException e)
         {
            MessageBox.Show("Es ist ein Fehler aufgetreten. Weitere Informationen:\r\n"
                            + e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
         }

      }

      #region Events

      private void BtnCheckSql_OnClick(object sender, RoutedEventArgs e)
      {
         TestSqlData();
      }

      private void BtnCreateJob_OnClick(object sender, RoutedEventArgs e)
      {
         CreateAgentJob();
      }

      private void BtnRestore_OnClick(object sender, RoutedEventArgs e)
      {
         Rollback();
      }

      private void MainWindow_OnClosing(object sender, CancelEventArgs e)
      {
         var item = ViewToItem();

         item?.SaveAsJson();
      }

      #endregion
   }
}
