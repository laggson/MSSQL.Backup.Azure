using System;
using System.Windows;

namespace MSSQL.Backup.Azure
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         new Application().Run(new MainWindow());
      }
   }
}
