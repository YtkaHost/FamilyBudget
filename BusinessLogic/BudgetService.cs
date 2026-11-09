using FamilyBudget.Models;
using FamilyBudget.Persistence;

namespace FamilyBudget.BusinessLogic
{
    public class BudgetService
    {
        private readonly BudgetStorage _storage;

        public BudgetService(BudgetStorage storage)
        {
            _storage = storage;
        }

        public void Load()
        {
            _storage.Load();
        }

        // ---------- Категорії ----------

        public Category AddCategory(string name, TransactionType type)
        {
            var category = new Category(_storage.GetNextCategoryId(), name, type);

            if (CategoryExists(category.Name, type, 0))
                throw new InvalidOperationException("Така категорія вже існує.");

            _storage.AddCategory(category);
            SaveChanges();
            return category;
        }

        public List<Category> GetCategories()
        {
            return _storage.GetAllCategories();
        }

        public Category GetCategory(int id)
        {
            Category? category = _storage.GetAllCategories().FirstOrDefault(c => c.Id == id);

            if (category == null)
                throw new InvalidOperationException("Категорію з Id " + id + " не знайдено.");

            return category;
        }

        public void RenameCategory(int id, string newName)
        {
            Category category = GetCategory(id);

            var check = new Category(category.Id, newName, category.Type);

            if (CategoryExists(check.Name, category.Type, category.Id))
                throw new InvalidOperationException("Така категорія вже існує.");

            category.Name = check.Name;
            SaveChanges();
        }

        public void DeleteCategory(int id)
        {
            Category category = GetCategory(id);

            bool isUsed = _storage.GetAllTransactions().Any(t => t.Category.Id == id);
            if (isUsed)
                throw new InvalidOperationException("Не можна видалити категорію: є операції, що її використовують.");

            _storage.RemoveCategory(category);
            SaveChanges();
        }

        private bool CategoryExists(string name, TransactionType type, int ignoreId)
        {
            return _storage.GetAllCategories().Any(c =>
                c.Id != ignoreId &&
                c.Type == type &&
                string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // ---------- Користувачі ----------

        public User AddUser(string name, FamilyRole role)
        {
            var user = new User(_storage.GetNextUserId(), name, role);
            _storage.AddUser(user);
            SaveChanges();
            return user;
        }

        public List<User> GetUsers()
        {
            return _storage.GetAllUsers();
        }

        public User GetUser(int id)
        {
            User? user = _storage.GetAllUsers().FirstOrDefault(u => u.Id == id);

            if (user == null)
                throw new InvalidOperationException("Користувача з Id " + id + " не знайдено.");

            return user;
        }

        // ---------- Операції ----------

        public Transaction AddTransaction(int categoryId, int userId, decimal amount, DateTime date, string description)
        {
            Category category = GetCategory(categoryId);
            User user = GetUser(userId);

            var transaction = new Transaction(_storage.GetNextTransactionId(), category, user, amount, date, description);
            _storage.AddTransaction(transaction);
            SaveChanges();
            return transaction;
        }

        public List<Transaction> GetTransactions()
        {
            return _storage.GetAllTransactions();
        }

        public Transaction GetTransaction(int id)
        {
            Transaction? transaction = _storage.GetAllTransactions().FirstOrDefault(t => t.Id == id);

            if (transaction == null)
                throw new InvalidOperationException("Операцію з Id " + id + " не знайдено.");

            return transaction;
        }

        public void UpdateTransaction(int id, int categoryId, decimal amount, DateTime date, string description)
        {
            Transaction old = GetTransaction(id);
            Category category = GetCategory(categoryId);

            var updated = new Transaction(old.Id, category, old.User, amount, date, description);
            _storage.ReplaceTransaction(updated);
            SaveChanges();
        }

        public void DeleteTransaction(int id)
        {
            Transaction transaction = GetTransaction(id);
            _storage.RemoveTransaction(transaction);
            SaveChanges();
        }

        public List<Transaction> FindTransactions(int? categoryId, int? userId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom != null && dateTo != null && dateFrom > dateTo)
                throw new ArgumentException("Початкова дата не може бути пізнішою за кінцеву.");

            List<Transaction> result = _storage.GetAllTransactions();

            if (categoryId != null)
            {
                GetCategory(categoryId.Value);   // якщо такої категорії немає - помилка, а не порожній результат
                result = result.Where(t => t.Category.Id == categoryId.Value).ToList();
            }

            if (userId != null)
            {
                GetUser(userId.Value);
                result = result.Where(t => t.User.Id == userId.Value).ToList();
            }

            if (dateFrom != null)
                result = result.Where(t => t.Date >= dateFrom.Value.Date).ToList();

            if (dateTo != null)
                result = result.Where(t => t.Date <= dateTo.Value.Date).ToList();

            return result;
        }

        // ---------- Баланс ----------

        public decimal GetTotalIncome()
        {
            return _storage.GetAllTransactions()
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
        }

        public decimal GetTotalExpense()
        {
            return _storage.GetAllTransactions()
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
        }

        // Поточний баланс = усі доходи - усі витрати
        public decimal GetBalance()
        {
            return GetTotalIncome() - GetTotalExpense();
        }

        // Викликається в кінці кожного методу, що змінює дані
        private void SaveChanges()
        {
            try
            {
                _storage.Save();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Зміну виконано, але зберегти файл не вдалося: " + ex.Message);
            }
        }
    }
}
