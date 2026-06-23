using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Models;

namespace OrderService.Infrastructure.Extensions;

public static class PagedListExtensions
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    extension<T>(IQueryable<T> source)
    {
        public async Task<PagedList<T>> ToPagedListAsync(int? page, int? pageSize, CancellationToken ct)
        {
            if (!page.HasValue || page <= 0)
                page = 1;

            if (!pageSize.HasValue || pageSize <= 0)
                pageSize = DefaultPageSize;

            if (pageSize > MaxPageSize)
                pageSize = MaxPageSize;

            var totalCount = source.Count();
            var data = await source
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToListAsync(ct);

            return new PagedList<T>
            {
                Page = page.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount,
                Data = data
            };
        }
    }
    
}
