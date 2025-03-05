﻿using API.Endpoints;
using System.Text.Json.Serialization;

namespace API.Features;

public class Pagination
{
    public static class Defaults
    {
        public const int PageSize = 10;
        public const string QueryKey = "paginationQuery";
    }

    public static class ArgumentName
    {
        public const string PageNumber = "page_number";
        public const string SortDirection = "sort_direction";
        public const string SortBy = "sort_by";
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public abstract class Request
    {
        [JsonPropertyName(ArgumentName.PageNumber)]
        public int? PageNumber { get; init; }

        [JsonPropertyName(ArgumentName.SortDirection)]
        public string? SortDirection { get; init; }

        [JsonPropertyName(ArgumentName.SortBy)]
        public string? SortBy { get; init; }
    }

    public sealed class Query
    {
        public int PageNumber { get; init; }
        public SortDirection SortDirection { get; init; }
        public required string SortBy { get; init; }
    }

    public sealed class Data
    {
        public required int TotalCount { get; init; }
        public required int PageNumber { get; init; }
        public int PageSize { get; } = Defaults.PageSize;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public sealed class RequestBindingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBindingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var pageNumber = context.Request.Query[ArgumentName.PageNumber].FirstOrDefault();
            var sortDirection = context.Request.Query[ArgumentName.SortDirection].FirstOrDefault();
            var sortBy = context.Request.Query[ArgumentName.SortBy].FirstOrDefault();

            var metadata = Routing.FeaturedEndpoints[context.Request.Path.Value!];
            var paginationQuery = new Query
            {
                PageNumber = int.TryParse(pageNumber, out var pn) ? pn : 1,
                SortDirection = Enum.TryParse<SortDirection>(sortDirection, true, out var sd) ? sd : SortDirection.Ascending,
                SortBy = sortBy is not null && metadata.AvailableSortOrders.TryGetValue(sortBy, out var sb)
                ? sb
                : metadata.AvailableSortOrders.First().Value,
            };
            context.Items[Defaults.QueryKey] = paginationQuery;

            await _next(context);
        }
    }
}
