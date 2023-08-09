using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.IRepositories;

public interface IVariationOptionRepository
{
    Task<ICollection<VariationOption>> Get(IEnumerable<int> Ids);
}
