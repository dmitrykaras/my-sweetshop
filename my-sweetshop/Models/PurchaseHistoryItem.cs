namespace my_sweetshop.Models;

public class PurchaseHistoryItem
{
    public string Title { get; }
    public string Price { get; }
    public bool IsLast { get; set; }

    public PurchaseHistoryItem(string title, string price)
    {
        Title = title;
        Price = price;
    }
}