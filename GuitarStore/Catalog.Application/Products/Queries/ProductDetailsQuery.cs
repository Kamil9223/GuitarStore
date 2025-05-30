﻿using Application.CQRS;
using Catalog.Application.Products.Dtos;
using Catalog.Application.Products.Services;
using Domain.Exceptions;
using Domain.StronglyTypedIds;

namespace Catalog.Application.Products.Queries;

public sealed record ProductDetailsQuery(ProductId ProductId) : IQuery;

internal sealed class ProductDetailsQueryHandler : IQueryHandler<ProductDetailsQuery, ProductDetailsDto>
{
    private readonly IProductQueryService _productQueryService;

    public ProductDetailsQueryHandler(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService;
    }

    public async Task<ProductDetailsDto> Handle(ProductDetailsQuery query)
    {
        var product = await _productQueryService.Get(query.ProductId);
        if (product is null)
        {
            throw new NotFoundException(query.ProductId);
        }

        return product;
    }
}


