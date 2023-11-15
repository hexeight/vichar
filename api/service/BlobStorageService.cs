using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs.Description;

namespace Vichar.Service
{
    // Prototype class to manipulate web pages stored as block blobs
    public class BlobStorageService
    {
        private BlobContainerClient _blobContainerClient;
        public BlobStorageService()
        {
            string containerUri = Environment.GetEnvironmentVariable("ContainerUri");
            this._blobContainerClient = new BlobContainerClient(
                    new Uri(containerUri),
                    new DefaultAzureCredential());
        }

        public string StageBlob(string blobPath, string content)
        {
            // Stage blob
            Guid id = Guid.NewGuid();
            string base64Id = System.Convert.ToBase64String(GetByteArray(64));
            Stream contentStream = GenerateStreamFromString(content);
            //contentStream.Position = 0;
            contentStream.Seek(0, SeekOrigin.Begin);

            BlockBlobClient blockBlobClient = this._blobContainerClient.GetBlockBlobClient(blobPath);
            Response<BlockInfo> result = blockBlobClient.StageBlock(base64Id, contentStream);
            return base64Id;
        }

        public string InsertBlock(string blobPath,string blockId)
        {
            // Get Blob and Block IDs
            BlockBlobClient blockBlobClient = this._blobContainerClient.GetBlockBlobClient(blobPath);
            Response<BlockList> blockList = blockBlobClient.GetBlockList();

            // Modify Block List and Update Blob
            List<String> blockIds = blockList.Value.CommittedBlocks.Select(b => b.Name).ToList();
            blockIds.InsertRange(blockIds.Count - 1, new List<String>{ blockId });
            //List<String> blockIds = new List<string> { blockId };
            var blobHttpHeader = new BlobHttpHeaders { ContentType = "text/html" };
            CommitBlockListOptions options = new CommitBlockListOptions
            {
                HttpHeaders = blobHttpHeader,
                AccessTier = AccessTier.Hot
            };
            Response<BlobContentInfo> resp = blockBlobClient.CommitBlockList(blockIds, options);
            return resp.Value.ETag.ToString();
        }

        public string RemoveBlock(string blobPath, string blockId)
        {
            // Get Blob and Block IDs

            // Modify Block List and Update Blob

            return "";
        }

        #region private helpers
        private Stream GenerateStreamFromString(String p)
        {
            Byte[] bytes = Encoding.UTF8.GetBytes(p);
            MemoryStream strm = new MemoryStream();
            strm.Write(bytes, 0, bytes.Length);
            return strm;
        }

        private byte[] GetByteArray(int size)
        {
            Random rnd = new Random();
            byte[] b = new byte[size];
            rnd.NextBytes(b);
            return b;
        }
        #endregion
    }
}