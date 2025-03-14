namespace FoodDeliveryApp.ViewModels
{
    public class MultipleOrderItemViewModel
    {
        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        // Quantity the user wishes to order (0 means not selected)
        public int Quantity { get; set; }
    }
}
