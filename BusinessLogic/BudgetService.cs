using FamilyBudget.Integration;
using FamilyBudget.Models;
using FamilyBudget.Persistence;

namespace FamilyBudget.BusinessLogic
{
    public class BudgetService
    {
        private readonly BudgetStorage _storage;
        private readonly SecurityApiClient _api;

        private const string AdminRole = "Admin";

        // Токен Security зберігається лише в пам'яті на час роботи програми.
        private string? _token;

        public BudgetService(BudgetStorage storage, SecurityApiClient api)
        {
            _storage = storage;
            _api = api;
        }


        public string? CurrentLogin { get; private set; }

        public string? CurrentApiRole { get; private set; }

        public User? CurrentUser { get; private set; }

        public bool IsLoggedIn => _token != null;

        public bool IsAdmin => CurrentApiRole == AdminRole;

        public void Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");
            if (password == "")
                throw new ArgumentException("Пароль не може бути порожнім.");

            LoginResult result = _api.Login(username.Trim(), password);

            _token = result.Token;
            CurrentLogin = username.Trim();
            CurrentApiRole = result.Role;
            CurrentUser = FindLocalUser(CurrentLogin);
        }

        public void Logout()
        {
            _token = null;
            CurrentLogin = null;
            CurrentApiRole = null;
            CurrentUser = null;
        }

        public User CreateLocalUser(FamilyRole familyRole)
        {
            if (CurrentLogin == null)
                throw new InvalidOperationException("Спочатку потрібно увійти в систему.");

            CurrentUser = AddUser(CurrentLogin, familyRole);
            return CurrentUser;
        }

        public List<ApiUser> GetApiUsers()
        {
            return _api.GetUsers(RequireAdminToken("переглядати користувачів сервісу"));
        }

        public User RegisterUser(string username, string password, string role, FamilyRole familyRole)
        {
            string token = RequireAdminToken("створювати користувачів");

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");
            if (password == "")
                throw new ArgumentException("Пароль не може бути порожнім.");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не може бути порожньою.");

            _api.Register(token, username.Trim(), password, role.Trim());

            return FindLocalUser(username.Trim()) ?? AddUser(username.Trim(), familyRole);
        }

        public void SetUserRole(string username, string role)
        {
            string token = RequireAdminToken("змінювати ролі");

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Логін не може бути порожнім.");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Роль не може бути порожньою.");

            _api.SetRole(token, username.Trim(), role.Trim());

            if (string.Equals(username.Trim(), CurrentLogin, StringComparison.OrdinalIgnoreCase))
                CurrentApiRole = role.Trim();
        }

        public void ChangeOwnPassword(string oldPassword, string newPassword)
        {
            if (_token == null)
                throw new InvalidOperationException("Спочатку потрібно увійти в систему.");
            if (oldPassword == "" || newPassword == "")
                throw new ArgumentException("Пароль не може бути порожнім.");

            _api.ChangePassword(_token, oldPassword, newPassword);
        }

        private User? FindLocalUser(string name)
        {
            return _storage.GetAllUsers().FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private string RequireAdminToken(string action)
        {
            if (_token == null)
                throw new InvalidOperationException("Спочатку потрібно увійти в систему.");
            if (!IsAdmin)
                throw new InvalidOperationException("Недостатньо прав: " + action + " може лише користувач з роллю " + AdminRole + ".");

            return _token;
        }


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

        public List<Category> GetCategories(TransactionType type)
        {
            return _storage.GetAllCategories().Where(c => c.Type == type).ToList();
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

            bool hasPlan = _storage.GetAllPlans().Any(p => p.Category.Id == id);
            if (hasPlan)
                throw new InvalidOperationException("Не можна видалити категорію: для неї є план бюджету. Спочатку видаліть план.");

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

        public BudgetPlan AddPlan(int categoryId, int year, int month, decimal plannedAmount)
        {
            Category category = GetCategory(categoryId);

            var plan = new BudgetPlan(_storage.GetNextPlanId(), category, year, month, plannedAmount);

            bool exists = _storage.GetAllPlans().Any(p =>
                p.Category.Id == categoryId && p.Year == year && p.Month == month);
            if (exists)
                throw new InvalidOperationException("План для цієї категорії на цей місяць уже є. Змініть його суму.");

            _storage.AddPlan(plan);
            SaveChanges();
            return plan;
        }

        public List<BudgetPlan> GetPlans()
        {
            return _storage.GetAllPlans();
        }

        public List<BudgetPlan> GetPlans(int year, int month)
        {
            return _storage.GetAllPlans().Where(p => p.Year == year && p.Month == month).ToList();
        }

        public BudgetPlan GetPlan(int id)
        {
            BudgetPlan? plan = _storage.GetAllPlans().FirstOrDefault(p => p.Id == id);

            if (plan == null)
                throw new InvalidOperationException("План з Id " + id + " не знайдено.");

            return plan;
        }

        public void UpdatePlanAmount(int id, decimal newAmount)
        {
            BudgetPlan plan = GetPlan(id);
            plan.PlannedAmount = newAmount;
            SaveChanges();
        }

        public void DeletePlan(int id)
        {
            BudgetPlan plan = GetPlan(id);
            _storage.RemovePlan(plan);
            SaveChanges();
        }


        public SavingGoal AddSavingGoal(string name, decimal targetAmount, DateTime deadline)
        {
            if (deadline.Date < DateTime.Today)
                throw new ArgumentException("Термін цілі не може бути в минулому.");

            var goal = new SavingGoal(_storage.GetNextGoalId(), name, targetAmount, deadline, 0);
            _storage.AddGoal(goal);
            SaveChanges();
            return goal;
        }

        public List<SavingGoal> GetSavingGoals()
        {
            return _storage.GetAllGoals();
        }

        public SavingGoal GetSavingGoal(int id)
        {
            SavingGoal? goal = _storage.GetAllGoals().FirstOrDefault(g => g.Id == id);

            if (goal == null)
                throw new InvalidOperationException("Ціль з Id " + id + " не знайдено.");

            return goal;
        }

        public SavingGoal AddContribution(int goalId, decimal amount)
        {
            SavingGoal goal = GetSavingGoal(goalId);

            if (amount <= 0)
                throw new ArgumentException("Внесок має бути більшим за 0.");

            goal.SavedAmount = goal.SavedAmount + amount;
            SaveChanges();
            return goal;
        }

        public void DeleteSavingGoal(int id)
        {
            SavingGoal goal = GetSavingGoal(id);
            _storage.RemoveGoal(goal);
            SaveChanges();
        }

        public List<Transaction> FindTransactions(int? categoryId, int? userId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom != null && dateTo != null && dateFrom > dateTo)
                throw new ArgumentException("Початкова дата не може бути пізнішою за кінцеву.");

            List<Transaction> result = _storage.GetAllTransactions();

            if (categoryId != null)
            {
                GetCategory(categoryId.Value);
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


        private void SaveChanges()
        {
            // Усі зміни зберігаються тут; у разі помилки сховище скасовує незбережений стан EF.
            _storage.SaveChanges();
        }
    }
}
