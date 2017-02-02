using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   public static class SqlHelper
   {
      private static string _connectionString;
      private static string _containerUrl;

      #region Automatisierung

      /// <summary>
      /// Überprürft die angegebenen Daten auf ihre Richtigkeit.
      /// Falls diese gültig sind, werden sie in der Klasse für den weiteren Gebrauch gespeichert.
      /// </summary>
      /// <param name="serverName">Der Name, bzw. die IP des SQL-Servers</param>
      /// <param name="password">Das Passwort, das für den Benutzer 'sa' verwendet wird.</param>
      public static void Init(string serverName, string password)
      {
         var connStr = $"Server={serverName};User Id=sa;Password={password}";

         using (SqlConnection con = new SqlConnection(connStr + ";Connection Timeout=5"))
         {
            try
            {
               con.Open();
            }
            catch (SqlException)
            {
               return;
            }
            con.Close();
            _connectionString = connStr;
         }
      }

      /// <summary>
      /// Weist den SQL-Server an, ein Backup der eingestellten Datenbank anzufertigen.
      /// </summary>
      /// <returns>Den Pfad, auf dem die Datei abgelegt wurde.</returns>
      public static void CreateBackupAndUpload(string database, bool istLog = false, bool ueberschreiben = true)
      {
         // Dateinamen zusammensetzen
         var fileName = _containerUrl + "/" + GetFileName(istLog);

         var command = "BACKUP "
            + (istLog ? "LOG" : "DATABASE")
            + $" [{database}] TO URL='{fileName}' WITH COMPRESSION, "
            + (ueberschreiben ? "FORMAT" : "NOFORMAT");

         using (var cmd = new SqlCommand(command))
         {
            cmd.Connection = new SqlConnection(_connectionString);
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
         }
      }

      /// <summary>
      /// Erstellt den aktuellen Dateinamen mit Wochentag und ggf. der aktuellen Stunde, falls es ein Log-Backup ist.
      /// </summary>
      /// <param name="istLog"></param>
      /// <returns></returns>
      private static string GetFileName(bool istLog = false)
      {
         string fileName = "CO_"
                 + DateTimeFormatInfo.CurrentInfo?.GetDayName(DateTime.Now.DayOfWeek);

         if (istLog)
            fileName += "_" + DateTime.Now.Hour.ToString("00");
         // + DateTime.Now.Minute.ToString("00");

         return fileName + ".bak";
      }

      /// <summary>
      /// Erstellt die User-Credentials im SQL-Server, über die das Backup auf den Azure-Speicher geladen wird.
      /// </summary>
      /// <param name="accName">Name des Speichers (siehe Subdomain)</param>
      /// <param name="container">Name des Containers (hinter / in der URL)</param>
      /// <param name="sasToken">Das im Azure-Portal generierte Shared-Access-Token</param>
      public static void VerbindungEinrichten(string accName, string container, string sasToken)
      {
         _containerUrl = $"https://{accName}.blob.core.windows.net/{container}";

         string cmdString = "IF EXISTS "
            + $"(SELECT * FROM sys.credentials WHERE name = '{_containerUrl}') "
            + $"DROP CREDENTIAL [{_containerUrl}]";

         Execute(cmdString);

         cmdString = $"CREATE CREDENTIAL [{_containerUrl}]"
            + "WITH IDENTITY=\'Shared Access Signature\',"
            + $"SECRET='{sasToken}'";

         // TODO: Prüfen, ob SAS gültig. Sonst Exception schmeißen.

         Execute(cmdString);
      }

      /// <summary>
      /// Führt den übergebenen Befehl auf dem SQL-Server aus und gibt die Anzahl der betroffenen Spalten zurück.
      /// </summary>
      /// <param name="command">Der SQL-Befehl</param>
      private static void Execute(string command)
      {
         using (var cmd = new SqlCommand(command))
         {
            cmd.Connection = new SqlConnection(_connectionString);
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();

            cmd.Connection.Close();
         }
      }

      #endregion

      #region Gui

      /// <summary>
      /// Fragt alle bestehenden Datenbanken vom SQL-Server ab und gibt diese als <see cref="IEnumerable{T}"/> zurück.
      /// </summary>
      /// <returns></returns>
      public static IEnumerable<string> GetAllTables()
      {
         var data = new List<string>();

         using (var cmd = new SqlCommand("select * from sys.databases ORDER BY name asc"))
         {
            try
            {
               cmd.Connection = new SqlConnection(_connectionString);
               cmd.Connection.Open();
            }
            catch (SqlException e)
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

      public static void Rollback(string database, string[] files)
      {
         var last = files[files.Length -1];
         var command = "";

         foreach (var file in files)
         {
            var path = _containerUrl + "/" + file;
            var current = "RESTORE DATABASE " + database + " FROM URL ='" + path
               + "' WITH NORECOVERY;";

            if (file == last)
               current = current.Replace("NORECOVERY", "RECOVERY");

            command += " " + current;
         }

         TryDropDatabase(database);
         
         using (var cmd = new SqlCommand(command))
         {
            cmd.CommandTimeout = 3600;
            cmd.Connection = new SqlConnection(_connectionString);
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
         }
      }

      private static void TryDropDatabase(string database)
      {
         try
         {
            Execute($"USE master; ALTER DATABASE {database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
         }
         catch
         {
            // ignored 
         }
         try
         {
            Execute($"DROP DATABASE {database}");
         }
         catch (SqlException)
         {
         }
      }

      /// <summary>
      /// Erstellt die Jobs im SQL-ServerAgent, die das Programm in regelmäßigen Abständen ausführen.
      /// </summary>
      public static void CreateAgentJobs(bool istLog = false)
      {
         var jobName = istLog ? "Azure_Log" : "Azure_Backup";

         var commands = new List<string>
         {
            "USE msdb;",

            $"IF EXISTS(select name from msdb.dbo.sysjobs_view WHERE name='{jobName}') "
            + $"EXEC sp_delete_job @job_name='{jobName}'",

            $"DECLARE @jobId BINARY(16) EXEC sp_add_job @job_name='{jobName}', "
            + "@category_name=N'Database Maintenance', @job_id = @jobId OUTPUT",

            "EXEC sp_add_jobstep @step_name='Backup', @job_id=@jobId, @subsystem=N'CmdExec', " 
            + "@command=N'" + AppDomain.CurrentDomain.BaseDirectory
            + "SQLBackup.exe " + (istLog ? "-log" : "-database") + "'",
            
            "EXEC sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'"
         };

         string schedule = istLog
            ? "EXEC sp_add_jobschedule @job_id=@jobId, @name='Alle 4 Stunden', @freq_type=4, "
               + "@freq_interval=1, @freq_subday_type=0x8, @freq_subday_interval=4"

            : "EXEC sp_add_jobschedule @job_id=@jobId, @name='Einmal täglich', @freq_type=4, "
               + "@freq_interval=1";

         commands.Insert(commands.Count -1, schedule);

         using (var cmd = new SqlCommand(string.Join(" ", commands)))
         {
            cmd.Connection = new SqlConnection(_connectionString);
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();

            cmd.Connection.Close();
         }

         // Am Ende vom Job Prüfen, ob alles geklappt hat.
      }

      #endregion
   }
}
