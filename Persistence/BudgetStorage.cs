using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilyBudget.Models;

namespace FamilyBudget.Persistence
{
    public class BudgetStorage
    {
        private static readonly JsonSerializerOptions _jsonOptions = CreateJsonOptions();

        private readonly string _filePath;
        private readonly List<Category> _categories = new List<Category>();
        private readonly List<User> _users = new List<User>();
        private readonly List<Transaction> _transactions = new List<Transaction>();

        public BudgetStorage(string filePath)
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

        // ---------- Категорії ----------

        public List<Category> GetAllCategories()
        {
            return new List<Category>(_categories);
        }

        public void AddCategory(Category category)
        {
            _categories.Add(category);
        }

        public void RemoveCategory(Category category)
        {
            _categories.Remove(category);
        }

        public int GetNextCategoryId()
        {
            if (_categories.Count == 0)
                return 1;

            return _categories.Max(c => c.Id) + 1;
        }

        // ---------- Користувачі ----------

        public List<User> GetAllUsers()
        {
            return new List<User>(_users);
        }

        public void AddUser(User user)
        {
            _users.Add(user);
        }

        public int GetNextUserId()
        {
            if (_users.Count == 0)
                return 1;

            return _users.Max(u => u.Id) + 1;
        }

        // ---------- Операції ----------

        public List<Transaction> GetAllTransactions()
        {
            return new List<Transaction>(_transactions);
        }

        public void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }

        public void RemoveTransaction(Transaction transaction)
        {
            _transactions.Remove(transaction);
        }

        // Замінює операцію на нову версію з тим самим Id
        public void ReplaceTransaction(Transaction updated)
        {
            int index = _transactions.FindIndex(t => t.Id == updated.Id);
            if (index < 0)
                throw new InvalidOperationException("Операцію з Id " + updated.Id + " не знайдено.");

            _transactions[index] = updated;
        }

        public int GetNextTransactionId()
        {
            if (_transactions.Count == 0)
                return 1;

            return _transactions.Max(t => t.Id) + 1;
        }

        // ---------- Збереження у файл ----------

        // Перетворюємо моделі на прості дані (FileData.cs) і записуємо їх у JSON
        public void Save()
        {
            var data = new BudgetFileData();

            foreach (Category c in _categories)
                data.Categories.Add(new CategoryData { Id = c.Id, Name = c.Name, Type = c.Type });

            foreach (User u in _users)
                data.Users.Add(new UserData { Id = u.Id, Name = u.Name, FamilyRole = u.FamilyRole });

            foreach (Transaction t in _transactions)
            {
                data.Transactions.Add(new TransactionData
                {
                    Id = t.Id,
                    CategoryId = t.Category.Id,   // замість цілого об'єкта категорії пишемо лише її Id
                    UserId = t.User.Id,
                    Amount = t.Amount,
                    Date = t.Date,
                    Description = t.Description
                });
            }

            string json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        // ---------- Завантаження з файлу ----------

        public void Load()
        {
            // Файлу ще немає (перший запуск) - це нормально, починаємо з порожніх списків
            if (!File.Exists(_filePath))
                return;

            string json = File.ReadAllText(_filePath);
            BudgetFileData? data = JsonSerializer.Deserialize<BudgetFileData>(json, _jsonOptions);

            if (data == null || data.Categories == null || data.Users == null || data.Transactions == null)
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

            _categories.Clear();
            _categories.AddRange(categories);
            _users.Clear();
            _users.AddRange(users);
            _transactions.Clear();
            _transactions.AddRange(transactions);
        }
    }
}
