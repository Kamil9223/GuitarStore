using Bogus;
using Catalog.Domain;
using Catalog.Infrastructure.Database;
using Domain.StronglyTypedIds;

namespace Tests.EndToEnd.Setup.Modules.Catalog;
internal static class CatalogDbSeeder
{
    public static VariationOption SeedVariationOption(
        this CatalogDbContext context,
        string? value = null,
        Variation? variation = null
        )
    {
        var faker = new Faker();
        var variationOption = new VariationOption
        {
            Id = VariationOptionId.New(),
            Value = value ?? faker.Random.String2(30),
            Variation = variation ?? context.SeedVariation()
        };
        context.Add(variationOption);
        return variationOption;
    }

    public static Variation SeedVariation(
        this CatalogDbContext context,
        string? name = null)
    {
        var faker = new Faker();
        var variation = new Variation
        {
            Id = VariationId.New(),
            Name = name ?? faker.Random.String2(30),
        };
        context.Add(variation);
        return variation;
    }

    public static Brand SeedBrand(
        this CatalogDbContext context,
        string? name = null
        )
    {
        var faker = new Faker();
        var brand = new Brand(name ?? faker.Random.String2(30));
        context.Add(brand);
        return brand;
    }

    public static Category SeedCategory(
        this CatalogDbContext context,
        IReadOnlyCollection<Variation>? variations = null,
        IReadOnlyCollection<Product>? products = null
        )
    {
        var faker = new Faker();
        var category = new Category
        {
            Id= CategoryId.New(),
            CategoryName = faker.Random.String2(30),
            Variations = variations?.ToList() ?? [],
            Products = products?.ToList() ?? []
        };
        context.Add(category);
        return category;
    }
}
