using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace MSSQL.Backup.Azure
{
   public class DeploymentHelper
   {
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

      public void UploadFile(string url, string containerName, string fileName)
      {
         var client = _storageAccount.CreateCloudBlobClient();
         var container = client.GetContainerReference(containerName);

         var reference = container.GetBlockBlobReference(fileName);
         reference.UploadFromFile(fileName);
      }
   }
}
