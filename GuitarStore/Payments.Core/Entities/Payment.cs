using Domain;
using Domain.StronglyTypedIds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Core.Entities;
public class Payment : Entity
{
    public PaymentId Id { get; init; }
    public string PaymentIntentId { get; init; } = null!;
}
