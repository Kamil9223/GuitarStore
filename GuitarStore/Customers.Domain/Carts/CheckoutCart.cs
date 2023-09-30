using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.Domain.Carts;

public class CheckoutCart : Entity, IIdentifiable
{
    public int Id { get; }
    public int CustomerId { get; }
    public Cart Cart { get; }

    internal CheckoutCart(Cart cart)
    {
        CustomerId = cart.CustomerId;
        Cart = cart;
    }


}
