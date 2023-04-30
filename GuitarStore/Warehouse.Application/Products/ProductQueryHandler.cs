using Application.Exceptions;
using Domain;
using MapsterMapper;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Products.Dtos;
using Warehouse.Application.Products.Queries;
using Warehouse.Domain.Products;

namespace Warehouse.Application.Products;

internal class ProductQueryHandler :
    IQueryHandler<ProductDetailsQuery, ProductDetailsDto>,
    IQueryHandler<ListProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IMapper _mapper;

    public ProductQueryHandler(IRepository<Product> productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductDetailsDto> Handle(ProductDetailsQuery query)
    {
        var product = await _productRepository.Get(query.ProductId);
        if (product is null)
        {
            throw new NotFoundException($"Product with Id: [{query.ProductId}] not exists.");
        }

        return _mapper.Map<ProductDetailsDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> Handle(ListProductsQuery query)
    {
        var products = await _productRepository.GetAll();

        return _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDto>>(products);
    }
}
