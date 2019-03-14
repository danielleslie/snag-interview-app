using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Text;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace app_int_listener
{
    public static class IncomingApplication
    {
        [FunctionName("IncomingApplication")]
        public async static void Run([EventHubTrigger("incoming-app", Connection = "EventHubConnectionAppSetting")]string myEventHubMessage, ILogger log, ExecutionContext context)
        {
            Application application = JsonConvert.DeserializeObject<Application>(myEventHubMessage);
            application.applicationId = Guid.NewGuid().ToString();
            Boolean passed = true;

            //Set connection variables here
            FeedOptions queryOptions = new FeedOptions {  EnableCrossPartitionQuery = true };
            string EndpointUri = Environment.GetEnvironmentVariable("EndpointUri");
            string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
            string databaseName = Environment.GetEnvironmentVariable("databaseName");
            string answerKeyCollection = Environment.GetEnvironmentVariable("answerKeyCollection");
            string applicationCollection = Environment.GetEnvironmentVariable("applicationCollection");
            DocumentClient client;

            client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            List<string> questionIds = new List<string>();

            //Make questionIds for SQL Query
            foreach (var question in application.QuestionAndAnswer) {
                questionIds.Add(question.questionId);
            }
            string questionIdsForSql = string.Join("','", questionIds);

            try{
                IQueryable<AnswerKey> Answers = client.CreateDocumentQuery<AnswerKey>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, answerKeyCollection), "SELECT * FROM AnswerKey WHERE AnswerKey.id IN ('" + questionIdsForSql + "')", 
                    queryOptions);
                
                log.LogInformation(Answers.ToString());

                List<AnswerKey> answersList = Answers.ToList();

                foreach (QuestionAndAnswer q in application.QuestionAndAnswer) {
                    AnswerKey answer = answersList.SingleOrDefault(s => s.questionId == q.questionId);
                    if (!answer.answers.Contains(q.answer.ToLower())){
                        passed = false;
                    };
                }

            } catch (Exception e){
                log.LogInformation(e.ToString());
            }

            if (passed) {
                log.LogInformation("PASSED");
                application.passed = true;
            }
            else {
                log.LogInformation("FAILED");
                application.passed = false;
            }

            try {
                //Ideally here you would pass to another event hub that interacted with a applications microservice
                //But for demo purposes I have directly pushed to another collection
                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, applicationCollection), application);
                log.LogInformation("Stored Application");
            } catch{

            }

            log.LogInformation($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
        }
    }

    public class Applicant
    {
        public string name { get; set; }
        public string contactNumber { get; set; }
    }

    public class QuestionAndAnswer {
        public string questionId { get; set; }
        public string answer { get; set; }
    }
    

    public class Application
    {
        [JsonProperty(PropertyName = "id")]
        public string applicationId {get; set;}

        [DataMember(Name="questionAndAnswer")]
        public List<QuestionAndAnswer> QuestionAndAnswer { get; set; }

        /// <summary>
        /// Gets or Sets Applicant
        /// </summary>
        [DataMember(Name="applicant")]
        public Object Applicant { get; set; }

        public Boolean passed {get; set;}
    }

    public class AnswerKey
    {
        [JsonProperty(PropertyName = "id")]
        public string questionId { get; set; }
        public List<string> answers { get; set; }
    }
}
