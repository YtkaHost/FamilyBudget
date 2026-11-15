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

    // План бюджету посилається на категорію за Id, як і операція
    public class BudgetPlanData
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal PlannedAmount { get; set; }
    }

    public class SavingGoalData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal TargetAmount { get; set; }
        public DateTime Deadline { get; set; }
        public decimal SavedAmount { get; set; }
    }

    // Увесь вміст файлу
    public class BudgetFileData
    {
        public List<CategoryData> Categories { get; set; } = new List<CategoryData>();
        public List<UserData> Users { get; set; } = new List<UserData>();
        public List<TransactionData> Transactions { get; set; } = new List<TransactionData>();
        public List<BudgetPlanData> Plans { get; set; } = new List<BudgetPlanData>();
        public List<SavingGoalData> Goals { get; set; } = new List<SavingGoalData>();
    }
}
