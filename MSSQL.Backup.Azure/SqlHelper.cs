using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   public class SqlHelper
   {
      private static string _ConnectionString;

      /// <summary>
      /// Fragt alle bestehenden Datenbanken vom SQL-Server ab und gibt diese als <see cref="IEnumerable{string}"/> zurück.
      /// Falls die angegebenen Daten gültig sind, werden sie in der Klasse für den weiteren Gebrauch gespeichert.
      /// </summary>
      /// <param name="serverName">Der Name, bzw. die IP des SQL-Servers</param>
      /// <param name="password">Das Passwort, das für den Benutzer 'sa' verwendet wird.</param>
      /// <returns></returns>
      public static IEnumerable<string> GetAllTables(string serverName, string password)
      {
         var connStr = $"Server={serverName};User Id=sa;Password={password}";

         var data = new List<string>();

         using (var cmd = new SqlCommand("select * from sys.databases ORDER BY name asc"))
         {
            try
            {
               cmd.Connection = new SqlConnection(connStr);
               cmd.Connection.Open();

               _ConnectionString = connStr;
            }
            catch(SqlException e)
            {
               if (e.Message.Contains("Verbindung mit SQL Server konnte nicht geöffnet werden"))
               {
                  MessageBox.Show("Verbindung mit dem SQL-Server konnte nicht hergestellt werden.");
               }
               else if (e.Message.Equals("Fehler bei der Anmeldung für den Benutzer \"sa\"."))
               {
                  MessageBox.Show("Die Anmeldedaten für den Nutzer sa waren ungültig");
               }

               return null;
            }

            using (var reader = cmd.ExecuteReader())
            {
               while (reader.Read())
               {
                  data.Add(reader[0].ToString());
               }
               reader.Close();
            }
            cmd.Connection.Close();
         }

         return data;
      }

      /// <summary>
      /// Weist den SQL-Server an, ein Backup der eingestellten Datenbank anzufertigen.
      /// </summary>
      /// <returns>Den Pfad, auf dem die Datei abgelegt wurde.</returns>
      public static string BackupDatabase(string database)
      {
         using (var cmd = new SqlCommand("SELECT * FROM sys.tables")) // TODO: änder mich
         {
            cmd.Connection = new SqlConnection(_ConnectionString);
            cmd.Connection.Open();
            
            // Dateinamen zusammensetzen
            string fileName = @"C:\Temp\SqlBackup\CO_" 
               + DateTimeFormatInfo.CurrentInfo.GetDayName(DateTime.Now.DayOfWeek)
               + ".bak";

            // Backup erstellen


            cmd.Connection.Close();

            return fileName;
         }
      }

      public static void Einrichten(string accName, string container, string sasToken)
      {
         string url = $"https://{accName}.blob.core.windows.net/{container}";

         string cmdString = "IF EXISTS"
            + $"(SELECT * FROM sys.credentials WHERE name = '{url}')"
            + $"DROP CREDENTIAL [{url}]";

         Execute(cmdString);

         cmdString = $"CREATE CREDENTIAL [{url}]"
            + $"WITH IDENTITY='Shared Access Signature',"
            + $"SECRET='{sasToken}'";

         Execute(cmdString);
      }

      /// <summary>
      /// Führt den übergebenen Befehl auf dem SQL-Server aus.
      /// </summary>
      /// <param name="command">Der SQL-Befehl</param>
      /// <returns>Anzahl der betroffenen Zeilen</returns>
      private static int Execute(string command)
      {
         int result;

         using (var cmd = new SqlCommand(command))
         {
            cmd.Connection = new SqlConnection(_ConnectionString);
            cmd.Connection.Open();

            result = cmd.ExecuteNonQuery();

            cmd.Connection.Close();
         }

            return result;
      }
   }
}
