namespace FamilyBudget.Models
{
    // Член сім'ї, який користується бюджетом
    public class User
    {
        private string _name = "";
        private FamilyRole _familyRole;

        public int Id { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ім'я користувача не може бути порожнім.");

                _name = value.Trim();
            }
        }

        // Роль у сім'ї: батько, мати або дитина
        public FamilyRole FamilyRole
        {
            get => _familyRole;
            set
            {
                if (!Enum.IsDefined(value))
                    throw new ArgumentException("Невідома роль у сім'ї.");

                _familyRole = value;
            }
        }

        public User(int id, string name, FamilyRole familyRole)
        {
            Id = id;
            Name = name;
            FamilyRole = familyRole;
        }
    }
}
