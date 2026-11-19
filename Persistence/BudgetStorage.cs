using FamilyBudget.Integration;
using FamilyBudget.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Persistence
{
    public class BudgetStorage
    {
        private readonly BudgetDbContext _db;

        public BudgetStorage(BudgetDbContext db)
        {
            _db = db;
        }


        public List<Category> GetAllCategories()
        {
            return _db.Categories.ToList();
        }

        public void AddCategory(Category category)
        {
            _db.Categories.Add(category);
        }

        public void RemoveCategory(Category category)
        {
            _db.Categories.Remove(category);
        }

        public int GetNextCategoryId()
        {
            if (!_db.Categories.Any())
                return 1;

            return _db.Categories.Max(c => c.Id) + 1;
        }


        public List<User> GetAllUsers()
        {
            return _db.Users.ToList();
        }

        public void AddUser(User user)
        {
            _db.Users.Add(user);
        }

        public int GetNextUserId()
        {
            if (!_db.Users.Any())
                return 1;

            return _db.Users.Max(u => u.Id) + 1;
        }


        public List<Transaction> GetAllTransactions()
        {
            return _db.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .ToList();
        }

        public void AddTransaction(Transaction transaction)
        {
            _db.Transactions.Add(transaction);
        }

        public void RemoveTransaction(Transaction transaction)
        {
            _db.Transactions.Remove(transaction);
        }

        public void ReplaceTransaction(Transaction updated)
        {
            Transaction? old = _db.Transactions.Find(updated.Id);
            if (old == null)
                throw new InvalidOperationException("Операцію з Id " + updated.Id + " не знайдено.");

            old.Category = updated.Category;
            old.User = updated.User;
            old.Amount = updated.Amount;
            old.Date = updated.Date;
            old.Description = updated.Description;
        }

        public int GetNextTransactionId()
        {
            if (!_db.Transactions.Any())
                return 1;

            return _db.Transactions.Max(t => t.Id) + 1;
        }


        public List<BudgetPlan> GetAllPlans()
        {
            return _db.BudgetPlans
                .Include(p => p.Category)
                .ToList();
        }

        public void AddPlan(BudgetPlan plan)
        {
            _db.BudgetPlans.Add(plan);
        }

        public void RemovePlan(BudgetPlan plan)
        {
            _db.BudgetPlans.Remove(plan);
        }

        public int GetNextPlanId()
        {
            if (!_db.BudgetPlans.Any())
                return 1;

            return _db.BudgetPlans.Max(p => p.Id) + 1;
        }


        public List<SavingGoal> GetAllGoals()
        {
            return _db.SavingGoals.ToList();
        }

        public void AddGoal(SavingGoal goal)
        {
            _db.SavingGoals.Add(goal);
        }

        public void RemoveGoal(SavingGoal goal)
        {
            _db.SavingGoals.Remove(goal);
        }

        public int GetNextGoalId()
        {
            if (!_db.SavingGoals.Any())
                return 1;

            return _db.SavingGoals.Max(g => g.Id) + 1;
        }


        public void SaveChanges()
        {
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _db.ChangeTracker.Clear();

                string reason = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new InvalidOperationException("Зміну не збережено: не вдалося записати в базу даних (" + reason + ").");
            }
        }
    }
}
