using Common.Errors.Exceptions;
using Domain;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class Product : Entity
{
    public ProductId Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; set; }
    public Brand Brand { get; init; } = null!;
    public BrandId BrandId { get; init; }
    public Category Category { get; init; } = null!;
    public CategoryId CategoryId { get; init; }
    public ICollection<VariationOption> VariationOptions { get; init; } = null!;

    private Product() { }

    public Product(
        string name,
        string description,
        decimal price,
        int quantity,
        Brand brand,
        Category category,
        ICollection<VariationOption> variationOptions)
    {
        Id = ProductId.New();
        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        Brand = brand;
        Category = category;
        VariationOptions = variationOptions;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw DomainException.InvalidProperty(nameof(description), description);

        Description = description;
    }

    public void DecreaseQuantity(int quantity)
    {
        if (Quantity < quantity)
            throw DomainException.CannotDescreaseQuantity(quantity, Id);

        Quantity -= quantity;
    }
}
