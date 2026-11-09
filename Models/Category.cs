namespace FamilyBudget.Models
{
    public class Category
    {
        private string _name = "";

        public int Id { get; }

        public TransactionType Type { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва категорії не може бути порожньою.");

                _name = value.Trim();
            }
        }

        public Category(int id, string name, TransactionType type)
        {
            if (!Enum.IsDefined(type))
                throw new ArgumentException("Невідомий тип категорії.");

            Id = id;
            Name = name;
            Type = type;
        }
    }
}
