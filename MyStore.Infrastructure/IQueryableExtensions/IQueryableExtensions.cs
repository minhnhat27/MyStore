using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.IQueryableExtensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int currentPage, int pageSize)
        {
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize);
        }
    }
}
