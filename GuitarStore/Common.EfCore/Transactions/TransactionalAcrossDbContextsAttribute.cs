using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.EfCore.Transactions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TransactionalAcrossDbContextsAttribute : Attribute
{
    public Type[] DbContextTypes { get; }

    public TransactionalAcrossDbContextsAttribute(params Type[] dbContextTypes)
    {
        DbContextTypes = dbContextTypes;
    }
}
