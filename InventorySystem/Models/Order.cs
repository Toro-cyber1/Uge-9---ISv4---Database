using System;
using System.Collections.Generic;
using System.Linq;

namespace InventorySystem;

public class Order
{
    public int Id { get; set; }

    public DateTime Time { get; set; } = DateTime.Now;
    public List<OrderLine> OrderLines { get; set; } = new();
    
    public bool IsProcessed { get; set; }

    public decimal Total => OrderLines.Sum(ol => ol.LineTotal);

    public override string ToString() => $"{Time:HH.mm.ss} | {Total:C}";
}