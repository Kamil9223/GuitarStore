using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Application.Products.Queries;

namespace Warehouse.Application.Products;

internal class ProductQueryHandler :
    IQueryHandler<ProductDetailsQuery>,
    IQueryHandler<ListProductsQuery>
{
    
}
