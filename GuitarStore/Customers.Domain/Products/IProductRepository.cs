﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.Domain.Products;

public interface IProductRepository
{
    void Add(Product product);
}
