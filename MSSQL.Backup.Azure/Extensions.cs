using System;
using System.Text;

namespace MSSQL.Backup.Azure
{
   static class Extensions
   {
      /// <summary>
      /// Konvertiert einen Text in den Base64-Standard
      /// </summary>
      /// <param name="text">Der Klartext, der umgewandelt werden soll</param>
      /// <returns>Den konvertierten Text im Base64-Format</returns>
      internal static string ToBase64(this string text)
      {
         if (string.IsNullOrEmpty(text))
            return string.Empty;

         var bytes = Encoding.UTF8.GetBytes(text);

         return Convert.ToBase64String(bytes);
      }

      /// <summary>
      /// Konvertiert einen Base64-String zurück zum Klartext
      /// </summary>
      /// <param name="text64">Der in Base64 kodierte Text.</param>
      /// <returns>Den eingegebenen String im Klartext.</returns>
      internal static string FromBase64(this string text64)
      {
         if (string.IsNullOrEmpty(text64))
            return string.Empty;

         var bytes = Convert.FromBase64String(text64);

         return Encoding.UTF8.GetString(bytes);
      }
   }
}
