using Application.Contracts;
using System.Linq.Expressions;

namespace Application.Extensions;
public static class QueryableExtensions
{
    public static IOrderedQueryable<TResonse> SortBy<TResonse, TKeySelector>(
        this IQueryable<TResonse> query,
        Expression<Func<TResonse, TKeySelector>> keySelector,
        SortType sortType)
    {
        return sortType == SortType.Asc
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
    }
}
