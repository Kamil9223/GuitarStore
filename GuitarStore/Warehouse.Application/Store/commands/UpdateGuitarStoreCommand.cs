﻿using Application;

namespace Warehouse.Application.Store.Commands;

public class UpdateGuitarStoreCommand : ICommand
{
    public int Id { get; init; }

    public string? City { get; init; }

    public string? Street { get; init; }

    public string? PostalCode { get; init; }
}
