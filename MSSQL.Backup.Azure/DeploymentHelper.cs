using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace MSSQL.Backup.Azure
{
   // ReSharper disable once UnusedMember.Global
   public class DeploymentHelper
   {
      // ReSharper disable once NotAccessedField.Local
      private CloudStorageAccount _storageAccount;

      public DeploymentHelper(string sasToken)
      {
         var credentials = new StorageCredentials(sasToken);
         _storageAccount = new CloudStorageAccount(credentials, true);
      }

      public DeploymentHelper(string accountName, string key)
      {
         var credentials = new StorageCredentials(accountName, key);
         _storageAccount = new CloudStorageAccount(credentials, true);
      }
   }
}
