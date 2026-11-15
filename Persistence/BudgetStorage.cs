using FamilyBudget.Models;

namespace FamilyBudget.Persistence
{
    public class BudgetStorage
    {
        private readonly List<Category> _categories = new List<Category>();
        private readonly List<User> _users = new List<User>();
        private readonly List<Transaction> _transactions = new List<Transaction>();
        private readonly List<BudgetPlan> _plans = new List<BudgetPlan>();
        private readonly List<SavingGoal> _goals = new List<SavingGoal>();

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

        // ---------- Плани бюджету ----------

        public List<BudgetPlan> GetAllPlans()
        {
            return new List<BudgetPlan>(_plans);
        }

        public void AddPlan(BudgetPlan plan)
        {
            _plans.Add(plan);
        }

        public void RemovePlan(BudgetPlan plan)
        {
            _plans.Remove(plan);
        }

        public int GetNextPlanId()
        {
            if (_plans.Count == 0)
                return 1;

            return _plans.Max(p => p.Id) + 1;
        }

        // ---------- Цілі накопичення ----------

        public List<SavingGoal> GetAllGoals()
        {
            return new List<SavingGoal>(_goals);
        }

        public void AddGoal(SavingGoal goal)
        {
            _goals.Add(goal);
        }

        public void RemoveGoal(SavingGoal goal)
        {
            _goals.Remove(goal);
        }

        public int GetNextGoalId()
        {
            if (_goals.Count == 0)
                return 1;

            return _goals.Max(g => g.Id) + 1;
        }

        // ---------- Очищення ----------

        public void Clear()
        {
            _categories.Clear();
            _users.Clear();
            _transactions.Clear();
            _plans.Clear();
            _goals.Clear();
        }
    }
}
