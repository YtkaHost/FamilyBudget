namespace FamilyBudget.Persistence
{

    public class CategoryData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public Models.TransactionType Type { get; set; }
    }

    public class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public Models.FamilyRole FamilyRole { get; set; }
    }

    public class TransactionData
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = "";
    }

    // Увесь вміст файлу
    public class BudgetFileData
    {
        public List<CategoryData> Categories { get; set; } = new List<CategoryData>();
        public List<UserData> Users { get; set; } = new List<UserData>();
        public List<TransactionData> Transactions { get; set; } = new List<TransactionData>();
    }
}
