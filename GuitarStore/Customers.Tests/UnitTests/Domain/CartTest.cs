using Customers.Domain.Carts;
using Customers.Domain.Products;
using Domain;
using Xunit;

namespace Customers.Tests.UnitTests.Domain;
public class CartTest
{
    [Fact]
    public void AddProduct_WhenProductNotExist_NewProductShouldBeAddedToCartAsCartItem()
    {
        var cart = Cart.Create(1);

        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), 1);

        Assert.Single(cart.CartItems);
    }

    [Fact]
    public void AddProduct_WhenProductAlreadyExists_QuantityOfCartItemsIncreaseAnd()
    {
        var cart = Cart.Create(1);
        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), quantity: 1);

        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), quantity: 3);

        Assert.Single(cart.CartItems);
        Assert.Collection(cart.CartItems, item =>
        {
            Assert.Equal(4, item.Quantity);
        });
        Assert.Equal(4 * 12, cart.TotalPrice);
    }

    [Fact]
    public void AddProduct_WhenOtherProductIsInTheCart_newCartItemAdded()
    {
        var cart = Cart.Create(1);
        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), quantity: 1);

        cart.AddProduct(Product.Create(2, "testProduct1", 19, 1), quantity: 2);

        Assert.Equal(2, cart.CartItems.Count);
        Assert.Equal(2*19+12, cart.TotalPrice);
    }

    [Fact]
    public void RemoveProduct_WhenCartIsEmpty_DomainExceptionIsThrown()
    {
        var cart = Cart.Create(1);

        var removingProduct = () => cart.RemoveProduct(1, 1);

        Assert.Throws<DomainException>(removingProduct);
    }

    [Fact]
    public void RemoveProduct_WhenCartContainCartItem_CartItemIsRemoved()
    {
        var cart = Cart.Create(1);
        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), 3);

        cart.RemoveProduct(1, 3);

        Assert.Empty(cart.CartItems);
    }

    [Fact]
    public void RemoveProduct_WhenCartContainsCartItemWithQuantityLessThanQuantityWhenTryingToRemove_DomainExceptionIsThrown()
    {
        var cart = Cart.Create(1);
        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), 3);

        var removingProduct = () => cart.RemoveProduct(1, 4);

        Assert.Throws<DomainException>(removingProduct);
    }

    [Fact]
    public void RemoveProduct_WhenCartContainsProductWithQuantityGreaterThanQuantityInRemoveRequest_QuantityOfCartItemDecreased()
    {
        var cart = Cart.Create(1);
        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), 3);

        cart.RemoveProduct(1, 1);

        Assert.Single(cart.CartItems);
        Assert.Equal(2, cart.CartItems.Single().Quantity);
    }

    [Fact]
    public void Checkout_WhenNoItems_DomainExceptionIsThrown()
    {
        var cart = Cart.Create(1);

        var checkouting = () => cart.Checkout();

        Assert.Throws<DomainException>(checkouting);
    }

    [Fact]
    public void Checkout_WhenSomeItemsExists_CheckoutCartReturned()
    {
        var cart = Cart.Create(1);
        cart.AddProduct(Product.Create(1, "testProduct", 12, 1), 3);

        var checkotut = cart.Checkout();

        Assert.IsType<CheckoutCart>(checkotut);
        Assert.Equal(1, checkotut.CustomerId);
        Assert.Single(checkotut.CartItems);
    }
}
