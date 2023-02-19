﻿using Application;

namespace Warehouse.Application.Store.Commands;

public class AddGuitarStoreCommand : ICommand
{
    public string? Name { get; init; }

    public string? City { get; init; }

    public string? Street { get; init; }

    public string? PostalCode { get; init; }
}
