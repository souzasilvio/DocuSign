using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AppWebHook.Helper
{
    public class StorageHelper
    {
        public static void UploadFileFromStream(IConfiguration config, Stream fileStream, string fileName)
        {
            string connection = config["Storage"];
            string nomeContainerStorage = config["NomeContainerStorage"];

            CloudStorageAccount storage = CloudStorageAccount.Parse(connection);
            CloudBlobClient client = storage.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(nomeContainerStorage);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            blob.UploadFromStream(fileStream);

        }

        public static void UploadFile(IConfiguration config, string fileName)
        {
            string connection = config["Storage"];
            string nomeContainerStorage = config["NomeContainerStorage"];

            CloudStorageAccount storage = CloudStorageAccount.Parse(connection);
            CloudBlobClient client = storage.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(nomeContainerStorage);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            using (var fileStream = System.IO.File.OpenRead(fileName))
            {
                blob.UploadFromStream(fileStream);
            }
        }
    }
}
