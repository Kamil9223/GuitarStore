using Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Abstractions;

public interface IQueryHandlerExecutor<TQuery, TResponse>
    where TQuery : IQuery
    where TResponse : class
{
    Task<TResponse> Execute(TQuery query);
}
