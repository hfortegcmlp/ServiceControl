﻿namespace ServiceBus.Management.Conversations
{
    using System.Linq;
    using Nancy;
    using Raven.Client;

    public class ConversationsModule : NancyModule
    {
        public IDocumentStore Store { get; set; }

        public ConversationsModule()
        {
            Get["/conversations/{conversationid}"] = parameters =>
            {
                using (var session = Store.OpenSession())
                {
                    string conversationId = parameters.conversationid;

                    RavenQueryStatistics stats;
                    var results = session.Query<Message>()
                        .Statistics(out stats)
                        .Where(m => m.ConversationId == conversationId)
                        .OrderBy(m=>m.TimeSent)
                        .Take(50)
                        .ToArray();

                    

                    return Negotiate
                            .WithModel(results)
                            .WithHeader("Total-Count", stats.TotalResults.ToString());
                }
            };
        }
    }
}