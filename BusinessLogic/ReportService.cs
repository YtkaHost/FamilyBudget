using FamilyBudget.Models;

namespace FamilyBudget.BusinessLogic
{
    public class ReportService
    {
        private const int TopExpensesCount = 3;
        private readonly BudgetService _service;

        public ReportService(BudgetService service)
        {
            _service = service;
        }

        public Report BuildReport(ReportPeriod period, DateTime date)
        {

            if (date.Year < 2000 || date.Year > 2100)
                throw new ArgumentException("Рік має бути від 2000 до 2100.");

            (DateTime from, DateTime to) = GetRange(period, date);

            List<Transaction> inPeriod = _service.FindTransactions(null, null, from, to);
            List<Transaction> incomes = inPeriod.Where(t => t.Type == TransactionType.Income).ToList();
            List<Transaction> expenses = inPeriod.Where(t => t.Type == TransactionType.Expense).ToList();

            decimal totalIncome = incomes.Sum(t => t.Amount);
            decimal totalExpense = expenses.Sum(t => t.Amount);

            decimal opening = _service.GetTransactions()
                .Where(t => t.Date < from)
                .Sum(t => Signed(t));

            DateTime lastDay = to > DateTime.Today ? DateTime.Today : to;
            int days = (lastDay - from).Days + 1;
            if (days < 1)
                days = 1;

            var report = new Report();
            report.Period = period;
            report.From = from;
            report.To = to;
            report.OperationsCount = inPeriod.Count;
            report.TotalIncome = totalIncome;
            report.TotalExpense = totalExpense;
            report.OpeningBalance = opening;
            report.ClosingBalance = opening + totalIncome - totalExpense;
            report.Days = days;
            report.AverageExpensePerDay = Math.Round(totalExpense / days, 2);
            report.AverageExpensePerWeek = Math.Round(totalExpense * 7 / days, 2);
            report.ExpenseShares = BuildShares(expenses);
            report.IncomeShares = BuildShares(incomes);
            report.BalanceDynamics = BuildDynamics(inPeriod, opening, period == ReportPeriod.Year);
            report.LargestExpenses = expenses
                .OrderByDescending(t => t.Amount)
                .ThenBy(t => t.Date)
                .Take(TopExpensesCount)
                .ToList();

            return report;
        }


        private (DateTime, DateTime) GetRange(ReportPeriod period, DateTime date)
        {
            date = date.Date;

            switch (period)
            {
                case ReportPeriod.Day:
                    return (date, date);

                case ReportPeriod.Week:

                    int shift = ((int)date.DayOfWeek + 6) % 7;
                    DateTime monday = date.AddDays(-shift);
                    return (monday, monday.AddDays(6));

                case ReportPeriod.Month:
                    DateTime first = new DateTime(date.Year, date.Month, 1);
                    return (first, first.AddMonths(1).AddDays(-1));

                default:
                    return (new DateTime(date.Year, 1, 1), new DateTime(date.Year, 12, 31));
            }
        }


        private decimal Signed(Transaction t)
        {
            if (t.Type == TransactionType.Income)
                return t.Amount;

            return -t.Amount;
        }



        private List<CategoryShare> BuildShares(List<Transaction> transactions)
        {
            decimal total = transactions.Sum(t => t.Amount);
            var shares = new List<CategoryShare>();


            foreach (var group in transactions.GroupBy(t => t.Category.Id))
            {
                decimal amount = group.Sum(t => t.Amount);

                var share = new CategoryShare();
                share.CategoryName = group.First().Category.Name;
                share.Amount = amount;
                share.Percent = Math.Round(amount * 100 / total, 1);
                shares.Add(share);
            }

            return shares.OrderByDescending(s => s.Amount).ToList();
        }



        private List<BalancePoint> BuildDynamics(List<Transaction> inPeriod, decimal openingBalance, bool byMonth)
        {
            var points = new List<BalancePoint>();
            decimal balance = openingBalance;


            var groups = inPeriod
                .GroupBy(t => byMonth ? new DateTime(t.Date.Year, t.Date.Month, 1) : t.Date)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                decimal income = group.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                decimal expense = group.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                balance = balance + income - expense;

                var point = new BalancePoint();
                point.Date = group.Key;
                point.Income = income;
                point.Expense = expense;
                point.Balance = balance;
                points.Add(point);
            }

            return points;
        }
    }
}
