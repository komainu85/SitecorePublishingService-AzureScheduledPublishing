using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Sitecore.Framework.Publishing.PublishJobQueue;

namespace SitecoreScheduledPublishing
{
    public static class ScheduledPublisher
    {
        private const string JobQueueUrl = "/api/publishing/jobqueue";

        [FunctionName("ScheduledPublisher")]
        public static async Task RunAsync([TimerTrigger("0 0 */2 * * *")]TimerInfo myTimer, TraceWriter log)
        {     
            var jobOptions = CreateSitePublishOptions();

            var client = new HttpClient();

            var result = await client.PutAsync(Environment.GetEnvironmentVariable("SiteUrl") + JobQueueUrl,
                   new StringContent(JsonConvert.SerializeObject(jobOptions), Encoding.UTF8, "application/json"));

            log.Info("Success: " + result.IsSuccessStatusCode);
        }

        private static PublishOptions CreateSitePublishOptions()
        {
            var metaData = new Dictionary<string, string>
            {
                {"PublishType", Environment.GetEnvironmentVariable("PublishType")},
                {"DetectCloneSources",  Environment.GetEnvironmentVariable("DetectCloneSources")},
                {"Publish.Options.ItemBucketsEnabled", Environment.GetEnvironmentVariable("DetectCloneSources")},
                {"Publish.Options.BucketTemplateId", Environment.GetEnvironmentVariable("BucketTemplateId")},
                {"SitecoreInstanceName", Environment.GetEnvironmentVariable("SitecoreInstanceName")}
            };

            var languages = Environment.GetEnvironmentVariable("Languages").Split(new char[] { '|' }).ToArray();
            var targets = Environment.GetEnvironmentVariable("Targets").Split(new char[] { '|' }).ToArray();

            return new PublishOptions(
                true,
                true,
                Environment.GetEnvironmentVariable("PublishUser"),
                Environment.GetEnvironmentVariable("ContextLanguage"),
                languages,
                targets,
                null,
                metaData,
                Environment.GetEnvironmentVariable("SourceDatabase"));
        }
    }
}
