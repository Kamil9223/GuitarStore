﻿using Application.CQRS;

namespace Catalog.Application.Products.Queries;

public class ProductDetailsQuery : IQuery
{
    public int ProductId { get; init; }
}
