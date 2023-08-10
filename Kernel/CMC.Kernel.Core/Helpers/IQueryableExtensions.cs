using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMC.Kernel.Core.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace CMC.Kernel.Core.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize <= 0)
                return source;
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public static async Task<PagedResult<T>> ToQueryResultAsync<T>(this IQueryable<T> dbQuery, int pageNumber = 1, int pageSize = 10, bool isCountOnly = false) where T : class
        {
            if (dbQuery == null)
                throw new ArgumentNullException("dbQuery");
            int count = await dbQuery.CountAsync();
            dbQuery = dbQuery.Page(pageNumber, pageSize);
            var data = isCountOnly ? null : dbQuery;

            return PagedResult<T>.Success(data, count, pageNumber, pageSize);
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }
    }
}
