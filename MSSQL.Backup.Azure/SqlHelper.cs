﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   public class SqlHelper
   {
      private static string _ConnectionString;
      private static string _ContainerUrl;

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
            _ConnectionString = connStr;
         }
      }

      /// <summary>
      /// Weist den SQL-Server an, ein Backup der eingestellten Datenbank anzufertigen.
      /// </summary>
      /// <returns>Den Pfad, auf dem die Datei abgelegt wurde.</returns>
      public static string CreateBackupAndUpload(string database, bool istLog = false, bool ueberschreiben = true)
      {
         // Dateinamen zusammensetzen
         string fileName = _ContainerUrl + "/" + GetFileName();

         string command = $"BACKUP "
            + (istLog ? "LOG" : "DATABASE")
            + $" [{database}] TO URL='{fileName}' WITH COMPRESSION, "
            + (ueberschreiben ? "FORMAT" : "NOFORMAT");

         using (var cmd = new SqlCommand(command))
         {
            cmd.Connection = new SqlConnection(_ConnectionString);
            cmd.Connection.Open();

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

            return fileName;
         }
      }

      /// <summary>
      /// Erstellt den aktuellen Dateinamen mit Wochentag und ggf. der aktuellen Stunde, falls es ein Log-Backup ist.
      /// </summary>
      /// <param name="istLog"></param>
      /// <returns></returns>
      public static string GetFileName(bool istLog = false)
      {
         string fileName = "CO_"
                 + DateTimeFormatInfo.CurrentInfo.GetDayName(DateTime.Now.DayOfWeek);

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
         _ContainerUrl = $"https://{accName}.blob.core.windows.net/{container}";

         string cmdString = "IF EXISTS "
            + $"(SELECT * FROM sys.credentials WHERE name = '{_ContainerUrl}') "
            + $"DROP CREDENTIAL [{_ContainerUrl}]";

         Execute(cmdString);

         cmdString = $"CREATE CREDENTIAL [{_ContainerUrl}]"
            + $"WITH IDENTITY='Shared Access Signature',"
            + $"SECRET='{sasToken}'";

         Execute(cmdString);
      }

      /// <summary>
      /// Führt den übergebenen Befehl auf dem SQL-Server aus und gibt die Anzahl der betroffenen Spalten zurück.
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

      #endregion

      #region Fuer Gui

      /// <summary>
      /// Fragt alle bestehenden Datenbanken vom SQL-Server ab und gibt diese als <see cref="IEnumerable{string}"/> zurück.
      /// </summary>
      /// <param name="serverName">Der Name, bzw. die IP des SQL-Servers</param>
      /// <param name="password">Das Passwort, das für den Benutzer 'sa' verwendet wird.</param>
      /// <returns></returns>
      public static IEnumerable<string> GetAllTables()
      {
         var data = new List<string>();

         using (var cmd = new SqlCommand("select * from sys.databases ORDER BY name asc"))
         {
            try
            {
               cmd.Connection = new SqlConnection(_ConnectionString);
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

      #endregion
   }
}
