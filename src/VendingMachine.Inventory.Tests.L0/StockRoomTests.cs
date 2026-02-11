using VendingMachine.Inventory;
using Xunit;

namespace VendingMachine.Inventory.Tests.L0;

[Trait("Level", "L0")]
public sealed class StockRoomTests
{
    [Fact]
    public void AddStock_IncreasesQuantity()
    {
        var stockRoom = new StockRoom();

        stockRoom.AddStock("C01", 3);

        Assert.Equal(3, stockRoom.GetQuantity("C01"));
    }

    [Fact]
    public void RemoveStock_DecreasesQuantity()
    {
        var stockRoom = new StockRoom();
        stockRoom.AddStock("C01", 4);

        stockRoom.RemoveStock("C01", 2);

        Assert.Equal(2, stockRoom.GetQuantity("C01"));
    }
}
