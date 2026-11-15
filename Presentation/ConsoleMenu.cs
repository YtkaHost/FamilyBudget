using System.Globalization;
using FamilyBudget.BusinessLogic;
using FamilyBudget.Models;

namespace FamilyBudget.Presentation
{
    public class ConsoleMenu
    {
        // Кольори виводу
        private const ConsoleColor IncomeColor = ConsoleColor.Green;
        private const ConsoleColor ExpenseColor = ConsoleColor.Red;

        private readonly BudgetService _service;
        private readonly ReportService _reports;

        public ConsoleMenu(BudgetService service, ReportService reports)
        {
            _service = service;
            _reports = reports;
        }

        public void Run()
        {
            try
            {

                Menu("СІМЕЙНИЙ БЮДЖЕТ", "Вийти",
                    new[] { "Категорії", "Користувачі", "Операції", "Баланс і звіти", "Плани бюджету", "Цілі накопичення" },
                    new Action[] { CategoriesMenu, UsersMenu, TransactionsMenu, ReportsMenu, PlansMenu, GoalsMenu },
                    false);
            }
            catch (EndOfStreamException)
            {
                
            }

            Console.WriteLine("До побачення!");
        }

        private void Menu(string title, string backText, string[] items, Action[] actions, bool pauseAfterAction)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("===== " + title + " =====");
                for (int i = 0; i < items.Length; i++)
                    Console.WriteLine((i + 1) + ". " + items[i]);
                Console.WriteLine("0. " + backText);

                int choice = ReadNumber("Ваш вибір", 0, items.Length);

                if (choice == 0)
                    return;

                try
                {
                    actions[choice - 1]();
                }
                catch (EndOfStreamException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка: " + ex.Message);
                }

                if (pauseAfterAction)
                    WaitForEnter();
            }
        }

        // Зупиняє програму, поки користувач не натисне Enter
        private void WaitForEnter()
        {
            Console.WriteLine();
            Console.Write("Натисніть Enter, щоб продовжити...");

            if (Console.ReadLine() == null)
                throw new EndOfStreamException();
        }

        // ---------- Кольори ----------

        // Колір для типу операції: доходи зелені, витрати червоні
        private ConsoleColor ColorFor(TransactionType type)
        {
            return type == TransactionType.Income ? IncomeColor : ExpenseColor;
        }

        private void WriteColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private void WriteLineColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }


        private void CategoriesMenu()
        {
            Menu("КАТЕГОРІЇ", "Назад",
                new[] { "Додати категорію", "Показати категорії", "Змінити назву категорії", "Видалити категорію" },
                new Action[] { AddCategory, ShowCategories, RenameCategory, DeleteCategory },
                true);
        }

        private void UsersMenu()
        {
            Menu("КОРИСТУВАЧІ", "Назад",
                new[] { "Додати користувача", "Показати користувачів" },
                new Action[] { AddUser, ShowUsers },
                true);
        }

        private void TransactionsMenu()
        {
            Menu("ОПЕРАЦІЇ", "Назад",
                new[] { "Додати операцію", "Показати операції", "Змінити операцію", "Видалити операцію",
                        "Знайти операції (за категорією, користувачем, періодом)" },
                new Action[] { AddTransaction, ShowTransactions, EditTransaction, DeleteTransaction, FindTransactions },
                true);
        }

        private void ReportsMenu()
        {
            Menu("БАЛАНС І ЗВІТИ", "Назад",
                new[] { "Показати баланс", "Звіт і статистика за період (день, тиждень, місяць, рік)" },
                new Action[] { ShowBalance, ShowReport },
                true);
        }

        private void PlansMenu()
        {
            Menu("ПЛАНИ БЮДЖЕТУ", "Назад",
                new[] { "Додати план", "Показати плани", "Змінити суму плану", "Видалити план",
                        "План і факт за місяць" },
                new Action[] { AddPlan, ShowPlans, EditPlan, DeletePlan, ShowPlanVsActual },
                true);
        }

        private void GoalsMenu()
        {
            Menu("ЦІЛІ НАКОПИЧЕННЯ", "Назад",
                new[] { "Додати ціль", "Показати цілі", "Внести кошти в ціль", "Видалити ціль" },
                new Action[] { AddGoal, ShowGoals, AddContribution, DeleteGoal },
                true);
        }

        // ---------- Категорії ----------

        private void AddCategory()
        {
            string name = ReadText("Назва категорії");
            TransactionType type = ReadType();

            Category category = _service.AddCategory(name, type);
            Console.WriteLine("Категорію додано (Id = " + category.Id + ").");
        }

        private void ShowCategories()
        {
            List<Category> categories = _service.GetCategories();

            if (categories.Count == 0)
            {
                Console.WriteLine("Категорій поки немає.");
                return;
            }

            PrintCategories(categories);
        }

        private void RenameCategory()
        {
            if (!HasCategories())
                return;

            int id = ReadNumber("Id категорії", 1, int.MaxValue);
            _service.GetCategory(id);
            string newName = ReadText("Нова назва");

            _service.RenameCategory(id, newName);
            Console.WriteLine("Назву змінено.");
        }

        private void DeleteCategory()
        {
            if (!HasCategories())
                return;

            int id = ReadNumber("Id категорії", 1, int.MaxValue);
            Category category = _service.GetCategory(id);

            if (!Confirm("Видалити категорію \"" + category.Name + "\"?"))
            {
                Console.WriteLine("Скасовано.");
                return;
            }

            _service.DeleteCategory(id);
            Console.WriteLine("Категорію видалено.");
        }

        // Показує список категорій; повертає false, якщо категорій немає
        private bool HasCategories()
        {
            List<Category> categories = _service.GetCategories();

            if (categories.Count == 0)
            {
                Console.WriteLine("Категорій поки немає.");
                return false;
            }

            PrintCategories(categories);
            return true;
        }

        private void PrintCategories(List<Category> categories)
        {
            Console.WriteLine("Id  Тип      Назва");
            foreach (Category c in categories)
                WriteLineColor($"{c.Id,2}  {TypeToText(c.Type),-8} {c.Name}", ColorFor(c.Type));
        }

        // ---------- Користувачі ----------

        private void AddUser()
        {
            string name = ReadText("Ім'я");
            FamilyRole role = ReadRole();

            User user = _service.AddUser(name, role);
            Console.WriteLine("Користувача додано (Id = " + user.Id + ").");
        }

        private void ShowUsers()
        {
            List<User> users = _service.GetUsers();

            if (users.Count == 0)
            {
                Console.WriteLine("Користувачів поки немає.");
                return;
            }

            PrintUsers(users);
        }

        private void PrintUsers(List<User> users)
        {
            Console.WriteLine("Id  Роль     Ім'я");
            foreach (User u in users)
                Console.WriteLine($"{u.Id,2}  {RoleToText(u.FamilyRole),-8} {u.Name}");
        }

        // ---------- Операції ----------

        private void AddTransaction()
        {
            List<Category> categories = _service.GetCategories();
            List<User> users = _service.GetUsers();

            if (categories.Count == 0 || users.Count == 0)
            {
                Console.WriteLine("Спочатку додайте хоча б одну категорію і одного користувача.");
                return;
            }

            PrintCategories(categories);
            int categoryId = ReadNumber("Id категорії", 1, int.MaxValue);

            PrintUsers(users);
            int userId = ReadNumber("Id користувача", 1, int.MaxValue);

            decimal amount = ReadAmount("Сума");
            DateTime date = ReadDate("Дата", DateTime.Today);
            string description = ReadLine("Опис (можна пропустити)");

            Transaction t = _service.AddTransaction(categoryId, userId, amount, date, description);
            Console.WriteLine("Операцію додано (Id = " + t.Id + ").");
        }

        private void ShowTransactions()
        {
            List<Transaction> transactions = _service.GetTransactions();

            if (transactions.Count == 0)
            {
                Console.WriteLine("Операцій поки немає.");
                return;
            }

            PrintTransactions(transactions);
        }

        private void EditTransaction()
        {
            List<Transaction> transactions = _service.GetTransactions();

            if (transactions.Count == 0)
            {
                Console.WriteLine("Операцій поки немає.");
                return;
            }

            PrintTransactions(transactions);
            int id = ReadNumber("Id операції, яку змінити", 1, int.MaxValue);
            Transaction old = _service.GetTransaction(id);

            Console.WriteLine("Нові значення вводьте або натисніть Enter, щоб залишити поточне.");

            PrintCategories(_service.GetCategories());
            int categoryId = ReadNumber("Id категорії", 1, int.MaxValue, old.Category.Id);
            decimal amount = ReadAmount("Сума", old.Amount);
            DateTime date = ReadDate("Дата", old.Date);

            string description = ReadLine("Опис (Enter - залишити, \"-\" - очистити)");
            if (description == "")
                description = old.Description;
            else if (description == "-")
                description = "";

            _service.UpdateTransaction(id, categoryId, amount, date, description);
            Console.WriteLine("Операцію змінено.");
        }

        private void DeleteTransaction()
        {
            List<Transaction> transactions = _service.GetTransactions();

            if (transactions.Count == 0)
            {
                Console.WriteLine("Операцій поки немає.");
                return;
            }

            PrintTransactions(transactions);
            int id = ReadNumber("Id операції, яку видалити", 1, int.MaxValue);
            _service.GetTransaction(id);   // якщо Id не існує - тут буде помилка

            if (!Confirm("Видалити операцію?"))
            {
                Console.WriteLine("Скасовано.");
                return;
            }

            _service.DeleteTransaction(id);
            Console.WriteLine("Операцію видалено.");
        }

        private void PrintTransactions(List<Transaction> transactions)
        {
            Console.WriteLine("Id  Дата        Тип      Категорія       Сума        Користувач  Опис");
            foreach (Transaction t in transactions)
            {
                WriteLineColor(
                    $"{t.Id,2}  {t.Date:dd.MM.yyyy}  {TypeToText(t.Type),-8} {t.Category.Name,-15} {Money(t.Amount),10}  {t.User.Name,-10}  {t.Description}",
                    ColorFor(t.Type));
            }
        }

        // ---------- Баланс і збереження ----------

        private void ShowBalance()
        {
            WriteLineColor("Доходи:  " + Money(_service.GetTotalIncome()), IncomeColor);
            WriteLineColor("Витрати: " + Money(_service.GetTotalExpense()), ExpenseColor);
            Console.WriteLine("Баланс:  " + Money(_service.GetBalance()));
        }

        // ---------- Пошук і фільтрація ----------

        private void FindTransactions()
        {
            if (_service.GetTransactions().Count == 0)
            {
                Console.WriteLine("Операцій поки немає.");
                return;
            }

            Console.WriteLine("Умови пошуку. Enter - пропустити умову.");

            if (_service.GetCategories().Count > 0)
                PrintCategories(_service.GetCategories());
            int? categoryId = ReadOptionalNumber("Id категорії");

            if (_service.GetUsers().Count > 0)
                PrintUsers(_service.GetUsers());
            int? userId = ReadOptionalNumber("Id користувача");

            DateTime? dateFrom = ReadOptionalDate("Дата від (включно)");
            DateTime? dateTo = ReadOptionalDate("Дата до (включно)");

            List<Transaction> found = _service.FindTransactions(categoryId, userId, dateFrom, dateTo);

            if (found.Count == 0)
            {
                Console.WriteLine("За цими умовами операцій не знайдено.");
                return;
            }

            PrintTransactions(found);
            Console.WriteLine("Знайдено операцій: " + found.Count);
        }

        // ---------- Звіти і статистика ----------

        private void ShowReport()
        {
            Console.WriteLine("Тип звіту: 1 - день, 2 - тиждень, 3 - місяць, 4 - рік");
            int choice = ReadNumber("Ваш вибір", 1, 4);

            var period = (ReportPeriod)(choice - 1);

            DateTime date = ReadDate("Будь-яка дата з потрібного періоду", DateTime.Today);

            Report report = _reports.BuildReport(period, date);
            PrintReport(report);
        }

        private void PrintReport(Report r)
        {
            Console.WriteLine();
            Console.WriteLine("===== ЗВІТ: " + PeriodToText(r.Period) + " " + r.From.ToString("dd.MM.yyyy")
                + " - " + r.To.ToString("dd.MM.yyyy") + " =====");

            if (r.OperationsCount == 0)
            {
                Console.WriteLine("За цей період операцій немає.");
                Console.WriteLine("Баланс: " + Money(r.OpeningBalance));
                return;
            }

            Console.WriteLine("Кількість операцій: " + r.OperationsCount);
            WriteLineColor("Доходи:             " + Money(r.TotalIncome), IncomeColor);
            WriteLineColor("Витрати:            " + Money(r.TotalExpense), ExpenseColor);
            Console.WriteLine("Різниця за період:  " + Money(r.TotalIncome - r.TotalExpense));
            Console.WriteLine("Баланс на початок:  " + Money(r.OpeningBalance));
            Console.WriteLine("Баланс на кінець:   " + Money(r.ClosingBalance));

            Console.WriteLine();
            WriteLineColor("-- Витрати за категоріями --", ExpenseColor);
            PrintShares(r.ExpenseShares, ExpenseColor);

            Console.WriteLine();
            WriteLineColor("-- Доходи за категоріями --", IncomeColor);
            PrintShares(r.IncomeShares, IncomeColor);

            Console.WriteLine();
            Console.WriteLine(r.Period == ReportPeriod.Year
                ? "-- Динаміка балансу (по місяцях) --"
                : "-- Динаміка балансу (по днях) --");
            Console.Write($"{"Дата",-10} ");
            WriteColor($"{"Доходи",10}", IncomeColor);
            Console.Write(" ");
            WriteColor($"{"Витрати",10}", ExpenseColor);
            Console.WriteLine($" {"Баланс",11}");
            foreach (BalancePoint p in r.BalanceDynamics)
            {
                string date = r.Period == ReportPeriod.Year ? p.Date.ToString("MM.yyyy") : p.Date.ToString("dd.MM.yyyy");
                Console.Write($"{date,-10} ");
                WriteColor($"{Money(p.Income),10}", IncomeColor);
                Console.Write(" ");
                WriteColor($"{Money(p.Expense),10}", ExpenseColor);
                Console.WriteLine($" {Money(p.Balance),11}");
            }

            Console.WriteLine();
            Console.WriteLine("-- Середні витрати (днів у розрахунку: " + r.Days + ") --");
            Console.WriteLine("За день:    " + Money(r.AverageExpensePerDay));
            Console.WriteLine("За тиждень: " + Money(r.AverageExpensePerWeek));

            Console.WriteLine();
            WriteLineColor("-- Найбільші витрати --", ExpenseColor);
            if (r.LargestExpenses.Count == 0)
                Console.WriteLine("Витрат за цей період немає.");
            else
                PrintTransactions(r.LargestExpenses);
        }

        private void PrintShares(List<CategoryShare> shares, ConsoleColor color)
        {
            if (shares.Count == 0)
            {
                Console.WriteLine("Немає.");
                return;
            }

            Console.WriteLine($"{"Категорія",-15} {"Сума",10} {"Частка",7}");
            foreach (CategoryShare s in shares)
                WriteLineColor($"{s.CategoryName,-15} {Money(s.Amount),10} {Percent(s.Percent),7}", color);
        }

        private void AddPlan()
        {
            // План складається лише для витрат, тому показуємо тільки категорії витрат
            List<Category> categories = _service.GetCategories(TransactionType.Expense);

            if (categories.Count == 0)
            {
                Console.WriteLine("Спочатку додайте хоча б одну категорію витрат.");
                return;
            }

            PrintCategories(categories);
            int categoryId = ReadNumber("Id категорії", 1, int.MaxValue);
            int month = ReadNumber("Місяць (1-12)", 1, 12, DateTime.Today.Month);
            int year = ReadNumber("Рік", 2000, 2100, DateTime.Today.Year);
            decimal amount = ReadAmount("Планова сума");

            BudgetPlan plan = _service.AddPlan(categoryId, year, month, amount);
            Console.WriteLine("План додано (Id = " + plan.Id + ").");
        }

        private void ShowPlans()
        {
            List<BudgetPlan> plans = _service.GetPlans();

            if (plans.Count == 0)
            {
                Console.WriteLine("Планів поки немає.");
                return;
            }

            PrintPlans(plans);
        }

        private void EditPlan()
        {
            List<BudgetPlan> plans = _service.GetPlans();

            if (plans.Count == 0)
            {
                Console.WriteLine("Планів поки немає.");
                return;
            }

            PrintPlans(plans);
            int id = ReadNumber("Id плану, який змінити", 1, int.MaxValue);
            BudgetPlan plan = _service.GetPlan(id);

            decimal amount = ReadAmount("Нова планова сума", plan.PlannedAmount);

            _service.UpdatePlanAmount(id, amount);
            Console.WriteLine("Суму плану змінено.");
        }

        private void DeletePlan()
        {
            List<BudgetPlan> plans = _service.GetPlans();

            if (plans.Count == 0)
            {
                Console.WriteLine("Планів поки немає.");
                return;
            }

            PrintPlans(plans);
            int id = ReadNumber("Id плану, який видалити", 1, int.MaxValue);
            _service.GetPlan(id);   // якщо Id не існує - тут буде помилка

            if (!Confirm("Видалити план?"))
            {
                Console.WriteLine("Скасовано.");
                return;
            }

            _service.DeletePlan(id);
            Console.WriteLine("План видалено.");
        }

        private void PrintPlans(List<BudgetPlan> plans)
        {
            Console.WriteLine($"{"Id",2}  {"Місяць",-15} {"Категорія",-15} {"План",10}");

            foreach (BudgetPlan p in plans.OrderBy(p => p.Year).ThenBy(p => p.Month).ThenBy(p => p.Id))
                Console.WriteLine($"{p.Id,2}  {MonthToText(p.Year, p.Month),-15} {p.Category.Name,-15} {Money(p.PlannedAmount),10}");
        }

        private void ShowPlanVsActual()
        {
            int month = ReadNumber("Місяць (1-12)", 1, 12, DateTime.Today.Month);
            int year = ReadNumber("Рік", 2000, 2100, DateTime.Today.Year);

            List<PlanComparison> rows = _reports.ComparePlans(year, month);

            Console.WriteLine();
            Console.WriteLine("===== ПЛАН І ФАКТ: " + MonthToText(year, month) + " =====");

            if (rows.Count == 0)
            {
                Console.WriteLine("На цей місяць планів немає.");
            }
            else
            {
                Console.WriteLine($"{"Категорія",-15} {"План",10} {"Факт",10} {"Різниця",10} {"Використано",12}");
                foreach (PlanComparison c in rows)
                {
                    Console.WriteLine($"{c.Plan.Category.Name,-15} {Money(c.Plan.PlannedAmount),10} {Money(c.Actual),10} "
                        + $"{Money(c.Difference),10} {Percent(c.UsedPercent),12}");
                }

                decimal totalPlan = rows.Sum(c => c.Plan.PlannedAmount);
                decimal totalActual = rows.Sum(c => c.Actual);
                Console.WriteLine($"{"Разом",-15} {Money(totalPlan),10} {Money(totalActual),10} {Money(totalPlan - totalActual),10}");
                Console.WriteLine("Різниця = план - факт. Мінус означає, що ліміт перевищено.");
            }

            Console.WriteLine();
            Console.WriteLine("-- Рекомендації --");
            List<string> recommendations = _reports.GetRecommendations(year, month);

            if (recommendations.Count == 0)
            {
                Console.WriteLine("Зауважень немає.");
                return;
            }

            foreach (string text in recommendations)
                Console.WriteLine("- " + text);
        }


        private void AddGoal()
        {
            string name = ReadText("Назва цілі");
            decimal target = ReadAmount("Сума, яку потрібно накопичити");
            DateTime deadline = ReadDate("Термін", DateTime.Today.AddMonths(6));

            SavingGoal goal = _service.AddSavingGoal(name, target, deadline);
            Console.WriteLine("Ціль додано (Id = " + goal.Id + ").");
        }

        private void ShowGoals()
        {
            List<SavingGoal> goals = _service.GetSavingGoals();

            if (goals.Count == 0)
            {
                Console.WriteLine("Цілей поки немає.");
                return;
            }

            PrintGoals(goals);
        }

        private void AddContribution()
        {
            List<SavingGoal> goals = _service.GetSavingGoals();

            if (goals.Count == 0)
            {
                Console.WriteLine("Цілей поки немає.");
                return;
            }

            PrintGoals(goals);
            int id = ReadNumber("Id цілі", 1, int.MaxValue);
            _service.GetSavingGoal(id);
            decimal amount = ReadAmount("Сума внеску");

            SavingGoal goal = _service.AddContribution(id, amount);

            Console.WriteLine("Внесок додано. Накопичено " + Money(goal.SavedAmount) + " із " + Money(goal.TargetAmount)
                + " (" + Percent(goal.ProgressPercent) + ").");
            if (goal.IsAchieved)
                Console.WriteLine("Ціль досягнуто!");
        }

        private void DeleteGoal()
        {
            List<SavingGoal> goals = _service.GetSavingGoals();

            if (goals.Count == 0)
            {
                Console.WriteLine("Цілей поки немає.");
                return;
            }

            PrintGoals(goals);
            int id = ReadNumber("Id цілі, яку видалити", 1, int.MaxValue);
            SavingGoal goal = _service.GetSavingGoal(id);

            if (!Confirm("Видалити ціль \"" + goal.Name + "\"?"))
            {
                Console.WriteLine("Скасовано.");
                return;
            }

            _service.DeleteSavingGoal(id);
            Console.WriteLine("Ціль видалено.");
        }

        private void PrintGoals(List<SavingGoal> goals)
        {
            Console.WriteLine($"{"Id",2}  {"Назва",-15} {"Ціль",10} {"Накопичено",11} {"Прогрес",8}  {"Термін",-10}  Стан");
            foreach (SavingGoal g in goals)
            {
                Console.WriteLine($"{g.Id,2}  {g.Name,-15} {Money(g.TargetAmount),10} {Money(g.SavedAmount),11} "
                    + $"{Percent(g.ProgressPercent),8}  {g.Deadline:dd.MM.yyyy}  {GoalStatus(g)}");
            }
        }

        private string GoalStatus(SavingGoal goal)
        {
            if (goal.IsAchieved)
                return "досягнуто";

            if (goal.DaysLeft < 0)
                return "термін минув";

            if (goal.DaysLeft == 0)
                return "термін сьогодні";

            return "залишилось " + goal.DaysLeft + " дн.";
        }

        private string MonthToText(int year, int month)
        {
            string[] names = { "січень", "лютий", "березень", "квітень", "травень", "червень",
                               "липень", "серпень", "вересень", "жовтень", "листопад", "грудень" };

            return names[month - 1] + " " + year;
        }

        private string ReadLine(string prompt)
        {
            Console.Write(prompt + ": ");
            string? line = Console.ReadLine();

            if (line == null)
                throw new EndOfStreamException();

            return line.Trim();
        }

        private string ReadText(string prompt)
        {
            string text = ReadLine(prompt);

            while (text == "")
            {
                Console.WriteLine("Поле не може бути порожнім.");
                text = ReadLine(prompt);
            }

            return text;
        }

        private int ReadNumber(string prompt, int min, int max, int? defaultValue = null)
        {
            string fullPrompt = prompt;
            if (defaultValue != null)
                fullPrompt += " (Enter - " + defaultValue + ")";

            while (true)
            {
                string text = ReadLine(fullPrompt);

                if (text == "" && defaultValue != null)
                    return defaultValue.Value;

                if (int.TryParse(text, out int number) && number >= min && number <= max)
                    return number;

                Console.WriteLine("Введіть ціле число" + (max == int.MaxValue ? "" : " від " + min + " до " + max) + ".");
            }
        }

        private decimal ReadAmount(string prompt, decimal? defaultValue = null)
        {
            string fullPrompt = prompt;
            if (defaultValue != null)
                fullPrompt += " (Enter - " + Money(defaultValue.Value) + ")";

            while (true)
            {
                string text = ReadLine(fullPrompt);

                if (text == "" && defaultValue != null)
                    return defaultValue.Value;

                text = text.Replace(',', '.');

                if (decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
                    return amount;

                Console.WriteLine("Введіть додатне число, наприклад 150 або 99.50.");
            }
        }

        private DateTime ReadDate(string prompt, DateTime defaultValue)
        {
            string fullPrompt = prompt + " (дд.мм.рррр, Enter - " + defaultValue.ToString("dd.MM.yyyy") + ")";

            while (true)
            {
                string text = ReadLine(fullPrompt);

                if (text == "")
                    return defaultValue;

                if (DateTime.TryParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;

                Console.WriteLine("Невірна дата. Приклад: 25.03.2026.");
            }
        }

        private int? ReadOptionalNumber(string prompt)
        {
            while (true)
            {
                string text = ReadLine(prompt + " (Enter - пропустити)");

                if (text == "")
                    return null;

                if (int.TryParse(text, out int number) && number >= 1)
                    return number;

                Console.WriteLine("Введіть ціле число більше 0 або натисніть Enter.");
            }
        }

        private DateTime? ReadOptionalDate(string prompt)
        {
            while (true)
            {
                string text = ReadLine(prompt + " (дд.мм.рррр, Enter - пропустити)");

                if (text == "")
                    return null;

                if (DateTime.TryParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;

                Console.WriteLine("Невірна дата. Приклад: 25.03.2026.");
            }
        }

        private TransactionType ReadType()
        {
            Console.Write("Тип: 1 - ");
            WriteColor("дохід", IncomeColor);
            Console.Write(", 2 - ");
            WriteLineColor("витрата", ExpenseColor);
            int choice = ReadNumber("Ваш вибір", 1, 2);

            if (choice == 1)
                return TransactionType.Income;

            return TransactionType.Expense;
        }

        private FamilyRole ReadRole()
        {
            Console.WriteLine("Роль у сім'ї: 1 - батько, 2 - мати, 3 - дитина");
            int choice = ReadNumber("Ваш вибір", 1, 3);

            if (choice == 1)
                return FamilyRole.Father;
            if (choice == 2)
                return FamilyRole.Mother;

            return FamilyRole.Child;
        }

        private bool Confirm(string question)
        {
            string answer = ReadLine(question + " (т/н)").ToLower();
            return answer == "т" || answer == "y" || answer == "так";
        }

        // ---------- Текст для виводу ----------

        private string TypeToText(TransactionType type)
        {
            return type == TransactionType.Income ? "дохід" : "витрата";
        }

        private string RoleToText(FamilyRole role)
        {
            switch (role)
            {
                case FamilyRole.Father: return "батько";
                case FamilyRole.Mother: return "мати";
                default: return "дитина";
            }
        }

        private string PeriodToText(ReportPeriod period)
        {
            switch (period)
            {
                case ReportPeriod.Day: return "день";
                case ReportPeriod.Week: return "тиждень";
                case ReportPeriod.Month: return "місяць";
                default: return "рік";
            }
        }


        private string Percent(decimal percent)
        {
            return percent.ToString("F1", CultureInfo.InvariantCulture) + "%";
        }


        private string Money(decimal amount)
        {
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}
