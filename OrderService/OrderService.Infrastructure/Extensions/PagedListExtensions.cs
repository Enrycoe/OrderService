using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Models;

namespace OrderService.Infrastructure.Extensions;

public static class PagedListExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public async Task<PagedList<T>> ToPagedListAsync(int? page, int? pageSize, CancellationToken ct)
        {
            if (!page.HasValue || page <= 0)
                page = 1;

            if (!pageSize.HasValue || pageSize <= 0)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

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
