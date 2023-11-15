using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vichar.Service;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Vichar.Api
{
    public static class PostComment
    {
        [FunctionName("PostComment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string page = req.Query["page"];

            log.LogInformation("Adding comment to page " + page);

            string name = HttpUtility.HtmlEncode(req.Form["name"]);
            string email = req.Form["email"];
            string comment = HttpUtility.HtmlEncode(req.Form["comment"]);
            string imageHash = ComputeSha256Hash(email.Trim().ToLower());

            BlobStorageService blobStorageService = new BlobStorageService();

            string commentBody = @"<div class=""comment-container"">
                <div class=""comment-info"">
                    <img src=""https://gravatar.com/avatar/"+ imageHash +@"?s=128"" alt=""Profile Image"" width=""50"" height=""50"">
                    <div>
                        <strong>Name:</strong> " + name + @"
                    </div>
                </div>

                <div class=""comment-text"">
                    <p>" + comment + @"</p>
                </div>

                <div class=""comment-date"">
                    <small>" + DateTime.UtcNow.ToString("dddd, dd MMMM yyyy HH:mm") + @"</small>
                </div>
            </div>";

            string blockId = blobStorageService.StageBlob(page, commentBody);
            blobStorageService.InsertBlock(page, blockId);

            return new RedirectResult(Environment.GetEnvironmentVariable("BlogRoot") + page);
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
