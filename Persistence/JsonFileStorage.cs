using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilyBudget.Models;

namespace FamilyBudget.Persistence
{
    // Переробив логіку, тепер перейти на базу данних буде простіше: достатньо реалізувати інтерфейс IBudgetStorage і замінити JsonFileStorage на DatabaseStorage.
    public class JsonFileStorage
    {
        private static readonly JsonSerializerOptions _jsonOptions = CreateJsonOptions();

        private readonly string _filePath;

        public JsonFileStorage(string filePath)
        {
            _filePath = filePath;
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        // ---------- Збереження у файл ----------

        public void Save(BudgetStorage storage)
        {
            var data = new BudgetFileData();

            foreach (Category c in storage.GetAllCategories())
                data.Categories.Add(new CategoryData { Id = c.Id, Name = c.Name, Type = c.Type });

            foreach (User u in storage.GetAllUsers())
                data.Users.Add(new UserData { Id = u.Id, Name = u.Name, FamilyRole = u.FamilyRole });

            foreach (Transaction t in storage.GetAllTransactions())
            {
                data.Transactions.Add(new TransactionData
                {
                    Id = t.Id,
                    CategoryId = t.Category.Id,
                    UserId = t.User.Id,
                    Amount = t.Amount,
                    Date = t.Date,
                    Description = t.Description
                });
            }

            foreach (BudgetPlan p in storage.GetAllPlans())
            {
                data.Plans.Add(new BudgetPlanData
                {
                    Id = p.Id,
                    CategoryId = p.Category.Id,
                    Year = p.Year,
                    Month = p.Month,
                    PlannedAmount = p.PlannedAmount
                });
            }

            foreach (SavingGoal g in storage.GetAllGoals())
            {
                data.Goals.Add(new SavingGoalData
                {
                    Id = g.Id,
                    Name = g.Name,
                    TargetAmount = g.TargetAmount,
                    Deadline = g.Deadline,
                    SavedAmount = g.SavedAmount
                });
            }

            string json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        // ---------- Завантаження з файлу ----------

        public void Load(BudgetStorage storage)
        {
            if (!File.Exists(_filePath))
                return;

            string json = File.ReadAllText(_filePath);
            BudgetFileData? data = JsonSerializer.Deserialize<BudgetFileData>(json, _jsonOptions);

            if (data == null || data.Categories == null || data.Users == null || data.Transactions == null
                || data.Plans == null || data.Goals == null)
                throw new InvalidDataException("Файл має неправильну структуру.");

            var categories = new List<Category>();
            foreach (CategoryData d in data.Categories)
            {
                if (categories.Any(c => c.Id == d.Id))
                    throw new InvalidDataException("У файлі повторюється Id категорії: " + d.Id + ".");

                categories.Add(new Category(d.Id, d.Name, d.Type));
            }

            var users = new List<User>();
            foreach (UserData d in data.Users)
            {
                if (users.Any(u => u.Id == d.Id))
                    throw new InvalidDataException("У файлі повторюється Id користувача: " + d.Id + ".");

                users.Add(new User(d.Id, d.Name, d.FamilyRole));
            }

            var transactions = new List<Transaction>();
            foreach (TransactionData d in data.Transactions)
            {
                if (transactions.Any(t => t.Id == d.Id))
                    throw new InvalidDataException("У файлі повторюється Id операції: " + d.Id + ".");

                Category? category = categories.FirstOrDefault(c => c.Id == d.CategoryId);
                if (category == null)
                    throw new InvalidDataException("Операція " + d.Id + ": не знайдено категорію з Id " + d.CategoryId + ".");

                User? user = users.FirstOrDefault(u => u.Id == d.UserId);
                if (user == null)
                    throw new InvalidDataException("Операція " + d.Id + ": не знайдено користувача з Id " + d.UserId + ".");

                transactions.Add(new Transaction(d.Id, category, user, d.Amount, d.Date, d.Description));
            }

            var plans = new List<BudgetPlan>();
            foreach (BudgetPlanData d in data.Plans)
            {
                if (plans.Any(p => p.Id == d.Id))
                    throw new InvalidDataException("У файлі повторюється Id плану: " + d.Id + ".");

                Category? category = categories.FirstOrDefault(c => c.Id == d.CategoryId);
                if (category == null)
                    throw new InvalidDataException("План " + d.Id + ": не знайдено категорію з Id " + d.CategoryId + ".");

                if (plans.Any(p => p.Category.Id == d.CategoryId && p.Year == d.Year && p.Month == d.Month))
                    throw new InvalidDataException("План " + d.Id + ": для цієї категорії на " + d.Month + "." + d.Year + " вже є інший план.");

                plans.Add(new BudgetPlan(d.Id, category, d.Year, d.Month, d.PlannedAmount));
            }

            var goals = new List<SavingGoal>();
            foreach (SavingGoalData d in data.Goals)
            {
                if (goals.Any(g => g.Id == d.Id))
                    throw new InvalidDataException("У файлі повторюється Id цілі: " + d.Id + ".");

                goals.Add(new SavingGoal(d.Id, d.Name, d.TargetAmount, d.Deadline, d.SavedAmount));
            }

            storage.Clear();

            foreach (Category item in categories)
                storage.AddCategory(item);

            foreach (User item in users)
                storage.AddUser(item);

            foreach (Transaction item in transactions)
                storage.AddTransaction(item);

            foreach (BudgetPlan item in plans)
                storage.AddPlan(item);

            foreach (SavingGoal item in goals)
                storage.AddGoal(item);
        }
    }
}
