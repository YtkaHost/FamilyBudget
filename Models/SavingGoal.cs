namespace FamilyBudget.Models
{
    // Скільки вже накопичено, користувач повідомляє вручну, вносячи кошти в ціль.
    public class SavingGoal
    {
        private string _name = "";
        private decimal _targetAmount;
        private DateTime _deadline;
        private decimal _savedAmount;

        public int Id { get; private set; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва цілі не може бути порожньою.");

                _name = value.Trim();
            }
        }

        // Скільки потрібно накопичити
        public decimal TargetAmount
        {
            get => _targetAmount;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Сума цілі має бути більшою за 0.");

                if (decimal.Round(value, 2) != value)
                    throw new ArgumentException("Сума цілі може мати не більше двох знаків після коми.");

                _targetAmount = value;
            }
        }

        public DateTime Deadline
        {
            get => _deadline;
            set => _deadline = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
        }

        public decimal SavedAmount
        {
            get => _savedAmount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Накопичена сума не може бути від'ємною.");

                if (decimal.Round(value, 2) != value)
                    throw new ArgumentException("Накопичена сума може мати не більше двох знаків після коми.");

                _savedAmount = value;
            }
        }

        // Прогрес у відсотках, не більше 100. Округляємо вниз для того, щоб не показувати 100%, якщо ще не накопичено всю суму. Наприклад, 99,99% округляємо до 99%.
        public decimal ProgressPercent
        {
            get
            {
                decimal percent = Math.Floor(SavedAmount * 1000 / TargetAmount) / 10;
                return Math.Min(100, percent);
            }
        }

        public bool IsAchieved => SavedAmount >= TargetAmount;

        // Скільки днів лишилось до терміну (від'ємне число - термін уже минув)
        public int DaysLeft => (Deadline - DateTime.Today).Days;

        public SavingGoal(int id, string name, decimal targetAmount, DateTime deadline, decimal savedAmount)
        {
            Id = id;
            Name = name;
            TargetAmount = targetAmount;
            Deadline = deadline;
            SavedAmount = savedAmount;
        }
    }
}
