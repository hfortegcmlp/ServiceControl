namespace Particular.Backend.Debugging.RavenDB.Api
{
    using System;
    using System.Linq;
    using Nancy;
    using Particular.Backend.Debugging.Api;
    using Raven.Client;
    using ServiceBus.Management.Infrastructure.Extensions;
    using ServiceBus.Management.Infrastructure.Nancy.Modules;
    using ServiceControl.Infrastructure.Extensions;

    public class ApiModule : BaseModule
    {
        public ApiModule()
        {
            Get["/sagas/{id}"] = parameters =>
            {
                Guid sagaId = parameters.id;
                using (var documentSession = Store.OpenSession())
                {
                    SagaHistory history;
                    DateTime lastModified;
                    if (!SagaSnapshotIndex.TryGetSagaHistory(documentSession, sagaId, out history, out lastModified))
                    {
                        return HttpStatusCode.NotFound;
                    }
                    return Negotiate
                        .WithModel(history)
                        .WithLastModified(lastModified);
                }
            };


            Get["/sagas"] = _ =>
            {
                using (var session = Store.OpenSession())
                {
                    RavenQueryStatistics stats;
                    var results = session.Query<SagaListIndex.Result, SagaListIndex>()
                            .Statistics(out stats)
                            .Paging(Request)
                            .ToArray();

                    return Negotiate
                        .WithModel(results)
                        .WithPagingLinksAndTotalCount(stats, Request)
                        .WithEtagAndLastModified(stats);
                }
            };

        }
    }
}