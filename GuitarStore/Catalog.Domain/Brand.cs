using Common.Errors;
using Common.Errors.Exceptions;
using Domain;
using Domain.StronglyTypedIds;
using Domain.ValueObjects;
using Newtonsoft.Json.Linq;

namespace Catalog.Domain;

public class Brand : Entity
{
    public BrandId Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<Product> Products { get; private set; } = null!;

    public Brand(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw DomainException.InvalidProperty(nameof(name), name.ToString());

        Id = BrandId.New();
        Name = name;
    }
}
