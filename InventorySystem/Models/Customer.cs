using System.Collections.Generic;

namespace InventorySystem;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Order> Orders { get; set; } = new();
    
    public void CreateOrder(OrderBook book, Order order)
    {
        Orders.Add(order);
        book.Queued.Add(order);
    }
}