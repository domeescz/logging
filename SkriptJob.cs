using System;
using Newtonsoft.Json;
using Quartz;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Threading.Channels;

namespace Scheduler
{

    class SlackFile
    {
        public String id { get; set; }
        public String name { get; set; }
    }

    class SlackFileResponse
    {
        public bool ok { get; set; }
        public String error { get; set; }
        public SlackFile file { get; set; }
    }

    public class SkriptJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            const int PartnerId = 4;
            const int AreaId = 10023;
            string webhookULR = "https://hooks.slack.com/services/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";//, "https://hooks.slack.com/services/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";
            const string auth = "xoxb-535603854772-4901000622401-yLfB1jaa4jsp2FuXF9JgQQQz";

            var currentTime = DateTime.Now.ToString("HH:mm:ss");

            var connectionString = "Server=dodo-logistics-failover.secondary.database.windows.net;Authentication=ActiveDirectoryInteractive;Database=Logistics;";
            //var accessToken = TODO;

            using var sqlConnection = new SqlConnection(connectionString);

            Console.WriteLine("Taking a snaphshot...");

            await sqlConnection.OpenAsync();

            var query = $"SELECT RouteId, route.AreaId, a.Name, CumulativeWeight, TotalLength, StartTime, EndTime, RecommendedStart, RecommendedEnd, DepartureTime, Created FROM Route INNER JOIN (SELECT AreaId, Name FROM Area) a ON Route.AreaId = a.AreaId WHERE Created >= CAST(DATEADD(day, -1, GETDATE()) AS DATE) AND PartnerId = {PartnerId} AND route.AreaId = {AreaId} AND Created LIKE '%08:%'";
            var sqlCommand = new SqlCommand(query, sqlConnection);
            var sqlReader = await sqlCommand.ExecuteReaderAsync();
            var rows = "";
            var branch = "";
            while (sqlReader.Read())
            {
                var routeId = sqlReader.GetInt32(0);//sqlReader["RouteId"].ToString();
                var areaId = sqlReader.GetInt32(1);
                string name = sqlReader.GetString(2);
                var cumulativeWeight = sqlReader.GetDouble(3);
                var totalLength = sqlReader.GetDouble(4);
                DateTime? startTime = sqlReader.IsDBNull(5) ? null : sqlReader.GetDateTime(5);
                DateTime? endTime = sqlReader.IsDBNull(6) ? null : sqlReader.GetDateTime(6);
                DateTime? recommendedStart = sqlReader.IsDBNull(7) ? null : sqlReader.GetDateTime(7);
                DateTime? recommendedEnd = sqlReader.IsDBNull(8) ? null : sqlReader.GetDateTime(8);
                DateTime? departureTime = sqlReader.IsDBNull(9) ? null : sqlReader.GetDateTime(9);
                DateTime? created = sqlReader.IsDBNull(10) ? null : sqlReader.GetDateTime(10);

                rows += $"{routeId}, {areaId}, {name}, {cumulativeWeight}, {totalLength}, {startTime}, {endTime}, {recommendedStart}, {recommendedEnd}, {departureTime}, {created}\n";
                branch = name;
            }
            var fileContentis = "RouteId, AreaId, Branch, CumulativeWeight, TotalLenght, StartTime, EndTime, RecommendedStart, RecommendedEnd, DepartureTime, Created\n" + rows;
            Console.WriteLine($"{sqlReader.RecordsAffected} found. {rows}");

            var fileName = $"/Users/dominik.smida/Library/Mobile Documents/com~apple~CloudDocs/js/snapshoty/snapshot_{DateTime.UtcNow:yyyy-mm-dd_hh:mm}.csv";
            await File.WriteAllTextAsync(fileName, fileContentis);



            var token = auth;
            var channels = "C04S2MFP0GN";
            var notification = $"Snapshot tras 2.vlny pro pobočku {branch} byl úspěšně vytvořen.";
            // we need to send a request with multipart/form-data
            var multiForm = new MultipartFormDataContent();

            // add API method parameters
            multiForm.Add(new StringContent(auth), "token");
            multiForm.Add(new StringContent(channels), "channels");
            multiForm.Add(new StringContent(notification), "initial_comment");

            // add file and directly upload it
            FileStream fs = File.OpenRead(fileName);
            multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(fileName));

            // send request to API
            var client = new HttpClient();
            var url = "https://slack.com/api/files.upload";
            var response = await client.PostAsync(url, multiForm);

            // fetch response from API
            var responseJson = await response.Content.ReadAsStringAsync();

            // convert JSON response to object
            SlackFileResponse fileResponse =
                JsonConvert.DeserializeObject<SlackFileResponse>(responseJson);

            // throw exception if sending failed
            if (fileResponse.ok == false)
            {
                throw new Exception(
                    "failed to upload message: " + fileResponse.error
                );
            }
            else
            {
                Console.WriteLine(
                        "Uploaded new file with id: " + fileResponse.file.id
                );
            }
        }
    }
}

