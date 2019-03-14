using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using IO.Swagger.Models;

namespace Utilities.EventHubUtility {

    public class EventHubUtility
    {
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = "Endpoint=sb://int-app.servicebus.windows.net/;SharedAccessKeyName=producer;SharedAccessKey=qC+4slhEb7iJNwdN2CsSaslVNSg+8ig/uRjClJI7HJ4=;EntityPath=incoming-app";
        private const string EventHubName = "incoming-app";

        public static async Task CreateEventHub(string body)
        {
            // Creates an EventHubsConnectionStringBuilder object from the connection string, and sets the EntityPath.
            // Typically, the connection string should have the entity path in it, but this simple scenario
            // uses the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await SendMessagesToEventHub(body);

            await eventHubClient.CloseAsync();
        }

            private static async Task SendMessagesToEventHub(string body)
        {
                try
                {
                    var message = body;
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch  (Exception exception)
                {
                }
        }
    }

}