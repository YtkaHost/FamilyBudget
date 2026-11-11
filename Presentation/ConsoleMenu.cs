using System.Globalization;
using FamilyBudget.BusinessLogic;
using FamilyBudget.Models;

namespace FamilyBudget.Presentation
{
    public class ConsoleMenu
    {
        private readonly BudgetService _service;
        private readonly ReportService _reports;

        public ConsoleMenu(BudgetService service, ReportService reports)
        {
            _service = service;
            _reports = reports;
        }

        public void Run()
        {
            bool exit = false;

            while (!exit)
            {
                ShowMenu();

                try
                {
                    int choice = ReadNumber("Ваш вибір", 0, 13);

                    switch (choice)
                    {
                        case 1: AddCategory(); break;
                        case 2: ShowCategories(); break;
                        case 3: RenameCategory(); break;
                        case 4: DeleteCategory(); break;
                        case 5: AddUser(); break;
                        case 6: ShowUsers(); break;
                        case 7: AddTransaction(); break;
                        case 8: ShowTransactions(); break;
                        case 9: EditTransaction(); break;
                        case 10: DeleteTransaction(); break;
                        case 11: ShowBalance(); break;
                        case 12: FindTransactions(); break;
                        case 13: ShowReport(); break;
                        case 0:
                            Console.WriteLine("До побачення!");
                            exit = true;
                            break;
                    }
                }
                catch (EndOfStreamException)
                {
                    // Введення закінчилось (наприклад, Ctrl+Z) - виходимо з програми
                    exit = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка: " + ex.Message);
                }
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== СІМЕЙНИЙ БЮДЖЕТ =====");
            Console.WriteLine("-- Категорії --");
            Console.WriteLine(" 1. Додати категорію");
            Console.WriteLine(" 2. Показати категорії");
            Console.WriteLine(" 3. Змінити назву категорії");
            Console.WriteLine(" 4. Видалити категорію");
            Console.WriteLine("-- Користувачі --");
            Console.WriteLine(" 5. Додати користувача");
            Console.WriteLine(" 6. Показати користувачів");
            Console.WriteLine("-- Операції --");
            Console.WriteLine(" 7. Додати операцію");
            Console.WriteLine(" 8. Показати операції");
            Console.WriteLine(" 9. Змінити операцію");
            Console.WriteLine("10. Видалити операцію");
            Console.WriteLine("-- Інше --");
            Console.WriteLine("11. Показати баланс");
            Console.WriteLine("12. Знайти операції (за категорією, користувачем, періодом)");
            Console.WriteLine("13. Звіт і статистика за період (день, тиждень, місяць, рік)");
            Console.WriteLine(" 0. Вийти");
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
                Console.WriteLine($"{c.Id,2}  {TypeToText(c.Type),-8} {c.Name}");
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
                Console.WriteLine(
                    $"{t.Id,2}  {t.Date:dd.MM.yyyy}  {TypeToText(t.Type),-8} {t.Category.Name,-15} {Money(t.Amount),10}  {t.User.Name,-10}  {t.Description}");
            }
        }

        // ---------- Баланс і збереження ----------

        private void ShowBalance()
        {
            Console.WriteLine("Доходи:  " + Money(_service.GetTotalIncome()));
            Console.WriteLine("Витрати: " + Money(_service.GetTotalExpense()));
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
            Console.WriteLine("Доходи:             " + Money(r.TotalIncome));
            Console.WriteLine("Витрати:            " + Money(r.TotalExpense));
            Console.WriteLine("Різниця за період:  " + Money(r.TotalIncome - r.TotalExpense));
            Console.WriteLine("Баланс на початок:  " + Money(r.OpeningBalance));
            Console.WriteLine("Баланс на кінець:   " + Money(r.ClosingBalance));

            Console.WriteLine();
            Console.WriteLine("-- Витрати за категоріями --");
            PrintShares(r.ExpenseShares);

            Console.WriteLine();
            Console.WriteLine("-- Доходи за категоріями --");
            PrintShares(r.IncomeShares);

            Console.WriteLine();
            Console.WriteLine(r.Period == ReportPeriod.Year
                ? "-- Динаміка балансу (по місяцях) --"
                : "-- Динаміка балансу (по днях) --");
            Console.WriteLine($"{"Дата",-10} {"Доходи",10} {"Витрати",10} {"Баланс",11}");
            foreach (BalancePoint p in r.BalanceDynamics)
            {
                string date = r.Period == ReportPeriod.Year ? p.Date.ToString("MM.yyyy") : p.Date.ToString("dd.MM.yyyy");
                Console.WriteLine($"{date,-10} {Money(p.Income),10} {Money(p.Expense),10} {Money(p.Balance),11}");
            }

            Console.WriteLine();
            Console.WriteLine("-- Середні витрати (днів у розрахунку: " + r.Days + ") --");
            Console.WriteLine("За день:    " + Money(r.AverageExpensePerDay));
            Console.WriteLine("За тиждень: " + Money(r.AverageExpensePerWeek));

            Console.WriteLine();
            Console.WriteLine("-- Найбільші витрати --");
            if (r.LargestExpenses.Count == 0)
                Console.WriteLine("Витрат за цей період немає.");
            else
                PrintTransactions(r.LargestExpenses);
        }

        private void PrintShares(List<CategoryShare> shares)
        {
            if (shares.Count == 0)
            {
                Console.WriteLine("Немає.");
                return;
            }

            Console.WriteLine($"{"Категорія",-15} {"Сума",10} {"Частка",7}");
            foreach (CategoryShare s in shares)
                Console.WriteLine($"{s.CategoryName,-15} {Money(s.Amount),10} {Percent(s.Percent),7}");
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
            Console.WriteLine("Тип: 1 - дохід, 2 - витрата");
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
