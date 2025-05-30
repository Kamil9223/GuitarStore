﻿using Domain;
using Domain.Exceptions;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class Brand : Entity
{
    public BrandId Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<Product> Products { get; private set; } = null!;

    public Brand(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Brand Name cannot be null or white space.");

        Id = BrandId.New();
        Name = name;
    }
}
