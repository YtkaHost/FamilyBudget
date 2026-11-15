namespace FamilyBudget.Models
{
    public class BudgetPlan
    {
        private decimal _plannedAmount;

        public int Id { get; }

        public Category Category { get; }
        public int Year { get; }
        public int Month { get; }

        public decimal PlannedAmount
        {
            get => _plannedAmount;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Планова сума має бути більшою за 0.");

                if (decimal.Round(value, 2) != value)
                    throw new ArgumentException("Планова сума може мати не більше двох знаків після коми.");

                _plannedAmount = value;
            }
        }

        public BudgetPlan(int id, Category category, int year, int month, decimal plannedAmount)
        {
            if (category == null)
                throw new ArgumentException("Категорія не може бути порожньою.");

            if (category.Type != TransactionType.Expense)
                throw new ArgumentException("План можна складати лише для категорії витрат.");

            if (year < 2000 || year > 2100)
                throw new ArgumentException("Рік має бути від 2000 до 2100.");

            if (month < 1 || month > 12)
                throw new ArgumentException("Місяць має бути від 1 до 12.");

            Id = id;
            Category = category;
            Year = year;
            Month = month;
            PlannedAmount = plannedAmount;
        }
    }
}
