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

namespace Vichar.Api
{
    public static class PostComment
    {
        [FunctionName("PostComment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string page = req.Query["page"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            page = page ?? data?.page;
            log.LogInformation("Adding comment to page " + page);

            BlobStorageService blobStorageService = new BlobStorageService();
            string blockId = blobStorageService.StageBlob(page, "<p>" + data.comment + "</p>");
            blobStorageService.InsertBlock(page, blockId);

            return new OkObjectResult(blockId);
        }
    }
}
