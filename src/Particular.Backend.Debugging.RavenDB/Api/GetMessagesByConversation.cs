﻿namespace Particular.Backend.Debugging.RavenDB.Api
{
    using System.Linq;
    using Nancy;
    using Particular.Backend.Debugging.Api;
    using Raven.Client;
    using Raven.Client.Linq;
    using ServiceBus.Management.Infrastructure.Extensions;
    using ServiceBus.Management.Infrastructure.Nancy.Modules;
    using ServiceControl.Infrastructure.Extensions;

    public class GetMessagesByConversation : BaseModule
    {
        public GetMessagesByConversation()
        {
            Get["/conversations/{conversationid}"] = parameters =>
            {
                using (var session = Store.OpenSession())
                {
                    string conversationId = parameters.conversationid;

                    RavenQueryStatistics stats;
                    var results = session.Query<MessagesViewIndex.SortAndFilterOptions, MessagesViewIndex>()
                            .Statistics(out stats)
                            .Where(m => m.ConversationId == conversationId)
                            .Sort(Request)
                            .Paging(Request)
                            .TransformWith<MessagesViewTransformer, MessagesView>()
                            .ToArray();

                    if (results.Length == 0)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    return Negotiate
                        .WithModel(results)
                        .WithPagingLinksAndTotalCount(stats, Request)
                        .WithEtagAndLastModified(stats);
                }
            };
        }
    }
}