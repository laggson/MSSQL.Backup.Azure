using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MSSQL.Backup.Azure
{
   public static class DeploymentHelper
   {
      private static CloudStorageAccount _storageAccount;
      private static CloudBlobClient _blobClient;
      private static CloudBlobContainer _blobContainer;

      public static void Init(string accountName, string azureKey)
      {
         var credentials = new StorageCredentials(accountName, azureKey);

         _storageAccount = new CloudStorageAccount(credentials, true);
         _blobClient = _storageAccount.CreateCloudBlobClient();
      }

      public static IEnumerable<string> GetFileNames(string containerName, string wochentag = null)
      {
         _blobContainer = _blobClient.GetContainerReference(containerName);

         var data = new List<string>();

         foreach (IListBlobItem item in _blobContainer.ListBlobs())
         {
            var blockBlob = item as CloudBlockBlob;
            if (blockBlob == null) continue;

            var blob = blockBlob;

            if(wochentag == null || blob.Name.Contains(wochentag))
               data.Add(blob.Name);
         }

         return data;
      }
   }
}
