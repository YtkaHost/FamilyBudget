namespace FamilyBudget.Models
{
    public class Transaction
    {
        private Category _category = null!;
        private User _user = null!;
        private decimal _amount;
        private DateTime _date;
        private string _description = "";

        public int Id { get; }

        public Category Category
        {
            get => _category;
            set
            {
                if (value == null)
                    throw new ArgumentException("Категорія не може бути порожньою.");

                _category = value;
            }
        }

        // Хто з членів сім'ї додав операцію
        public User User
        {
            get => _user;
            set
            {
                if (value == null)
                    throw new ArgumentException("Користувач не може бути порожнім.");

                _user = value;
            }
        }

        // Тип операції (дохід чи витрата) беремо з її категорії
        public TransactionType Type => Category.Type;

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Сума має бути більшою за 0.");

                // Гроші мають не більше двох знаків після коми (копійки)
                if (decimal.Round(value, 2) != value)
                    throw new ArgumentException("Сума може мати не більше двох знаків після коми.");

                _amount = value;
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (value.Date > DateTime.Today)
                    throw new ArgumentException("Дата операції не може бути в майбутньому.");

                _date = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
            }
        }

        // Опис необов'язковий: якщо його немає, зберігаємо порожній рядок
        public string Description
        {
            get => _description;
            set
            {
                if (value == null)
                    _description = "";
                else
                    _description = value.Trim();
            }
        }

        public Transaction(int id, Category category, User user, decimal amount, DateTime date, string description)
        {
            Id = id;
            Category = category;
            User = user;
            Amount = amount;
            Date = date;
            Description = description;
        }
    }
}
