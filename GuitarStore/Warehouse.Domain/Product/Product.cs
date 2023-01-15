using Domain;
using Warehouse.Domain.Product.Exceptions;
using Warehouse.Domain.Store;

namespace Warehouse.Domain.Product;

public class Product : Entity, IIdentifiable
{
    public int Id { get; }
    public int CategoryId { get; }
    public Category Category { get; }
    public ProductModel ProductModel { get; }
    public Money Price { get; }
    public string Description { get; }
    public int GuitarStoreId { get; }
    public GuitarStore GuitarStore { get; }

    //For EF Core
    private Product() { }

    private Product(int categoryId, ProductModel productModel, Money price, string description, int guitarStoreId)
    {
        CategoryId = categoryId;
        ProductModel = productModel;
        Price = price;
        Description = description;
        GuitarStoreId = guitarStoreId;
    }

    internal static Product Create(int categoryId, ProductModel productModel, Money price, string description, int guitarStoreId)
    {
        return new Product(categoryId, productModel, price, description, guitarStoreId);
    }
}
