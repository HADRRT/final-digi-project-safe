namespace ExpensesSystem.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string ChildName { get; set; } = string.Empty;
    }
}