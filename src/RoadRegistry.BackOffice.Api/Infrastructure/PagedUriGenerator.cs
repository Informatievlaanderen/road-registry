namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using Abstractions;
    using Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IPagedUriGenerator
    {
        public Uri FirstPage(string path);
        public Uri? NextPage<T>(IEnumerable<T> query, Pagination pagination, string path);
    }

    public class PagedUriGenerator : IPagedUriGenerator
    {
        private readonly Uri _baseUri;

        public PagedUriGenerator(ApiOptions apiOptions)
        {
            _baseUri = new Uri(apiOptions.BaseUrl);
        }

        public Uri? NextPage<T>(IEnumerable<T> query, Pagination pagination, string path)
        {
            var hasNextPage = query
                .Skip(pagination.NextPageOffset)
                .Any();

            path = path.TrimEnd('/');

            return hasNextPage
                ? new Uri(_baseUri, $"{path}?offset={pagination.NextPageOffset}")
                : null;
        }

        public Uri FirstPage(string path)
        {
            return new Uri(_baseUri, $"{path.TrimEnd('/')}?offset=0");
        }
    }
}
