﻿using Domain;

namespace Catalog.Domain;

public class Product : Entity, IIdentifiable
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Brand Brand { get; private set; } = null!;
    public int BrandId { get; private set; }
    public Category Category { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public ICollection<VariationOption> VariationOptions { get; private set; } = null!;

    public Product(string name, string description, Brand brand, Category category, ICollection<VariationOption> variationOptions)
    {
        Name = name;
        Description = description;
        Brand = brand;
        Category = category;
        VariationOptions = variationOptions;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Product description cannot be null or white space.");

        Description = description;
    }
}
