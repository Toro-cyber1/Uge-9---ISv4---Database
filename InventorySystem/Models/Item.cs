namespace InventorySystem;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal PricePerUnit { get; set; }
    public uint InventoryLocation { get; set; } // 1=a, 2=b, 3=c
    public override string ToString() => $"{Name} ({PricePerUnit:C}/unit)";
}