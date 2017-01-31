using System;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         if(args.Length == 1)
         {
            var arg = args[0];

            // Entweder Voll- oder Log-Backup. Nie beides.
            if(arg == "-database")
            {
            }
            else if(arg == "-log")
            {

            }
         }
         else
         {
            // Fenster wird nur erstellt, wenn keine oder ungültige Argumente angegeben wurden.
            new Application().Run(new MainWindow());
         }
      }
   }
}
