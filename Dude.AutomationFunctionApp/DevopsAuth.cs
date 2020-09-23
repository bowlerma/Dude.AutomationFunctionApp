using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dude.AutomationFunctionApp
{
    public static class DevopsAuth
    {
        private static readonly HttpClient Client = new HttpClient();
        
        [FunctionName("DevopsAuth")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string pat = req.Query["pat"];
            var token = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("dude:" + pat));

            string devopsUriType = req.Query["devopsUriType"];
            string url = "";

            if (devopsUriType.Equals("repo"))
            {
                string runbookFile = req.Query["runbookFile"];
                url =
                    "https://dev.azure.com/mhrengineering/Trent11%20Hosted/_apis/git/repositories/PF.Automation/items?path=%2FDeploymentSpecific%2FSubprocesses%2F" +
                    runbookFile + "&api-version=5.1";
            }
            else if (devopsUriType.Equals("package"))
            {
                string package = req.Query["package"];
                string packageVersion = req.Query["packageVersion"];
                url =
                    "https://pkgs.dev.azure.com/mhrengineering/Engineering%20Playground/_apis/packaging/feeds/DudePWSHTest/nuget/packages/" + package + "/versions/" + packageVersion + "/content?api-version=6.0-preview.1";
            }

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

            var response = Client.GetAsync(url).Result;
            
            return new OkObjectResult(response.Content.ReadAsStreamAsync().Result);
        }
    }
}