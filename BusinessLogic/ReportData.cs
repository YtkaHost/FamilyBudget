using FamilyBudget.Models;

namespace FamilyBudget.BusinessLogic
{
    public enum ReportPeriod
    {
        Day,
        Week,
        Month,
        Year
    }

    public class CategoryShare
    {
        public string CategoryName { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Percent { get; set; }
    }

    public class BalancePoint
    {
        public DateTime Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Balance { get; set; }
    }

    public class Report
    {
        public ReportPeriod Period { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int OperationsCount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public int Days { get; set; }
        public decimal AverageExpensePerDay { get; set; }
        public decimal AverageExpensePerWeek { get; set; }

        public List<CategoryShare> ExpenseShares { get; set; } = new List<CategoryShare>();
        public List<CategoryShare> IncomeShares { get; set; } = new List<CategoryShare>();
        public List<BalancePoint> BalanceDynamics { get; set; } = new List<BalancePoint>();
        public List<Transaction> LargestExpenses { get; set; } = new List<Transaction>();
    }
}
