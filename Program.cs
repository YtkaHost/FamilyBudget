using System.Text;
using FamilyBudget.BusinessLogic;
using FamilyBudget.Integration;
using FamilyBudget.Persistence;
using FamilyBudget.Presentation;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Шлях відносний до каталогу, з якого запущено програму.
const string DatabasePath = "budget.db";

using var db = new BudgetDbContext(DatabasePath);
var storage = new BudgetStorage(db);
// Віддалений API Security 
string apiUrl = Environment.GetEnvironmentVariable("SECURITY_API_URL") ?? SecurityApiClient.DefaultUrl;
SecurityApiClient apiClient;

try
{
    apiClient = new SecurityApiClient(apiUrl);
}
catch (UriFormatException)
{
    Console.WriteLine("Некоректна адреса сервісу Security: " + apiUrl);
    return;
}

var service = new BudgetService(storage, apiClient);
var reports = new ReportService(service);
var menu = new ConsoleMenu(service, reports);

try
{
    db.Database.EnsureCreated();

    Console.WriteLine("База даних: " + Path.GetFullPath(DatabasePath));
    Console.WriteLine("Сервіс Security: " + apiClient.BaseUrl);
    Console.WriteLine("Записів у базі: категорій - " + service.GetCategories().Count
        + ", користувачів - " + service.GetUsers().Count
        + ", операцій - " + service.GetTransactions().Count
        + ", планів - " + service.GetPlans().Count
        + ", цілей - " + service.GetSavingGoals().Count);
}
catch (Exception ex)
{
    Console.WriteLine("Не вдалося відкрити базу даних " + Path.GetFullPath(DatabasePath));
    Console.WriteLine("Причина: " + ex.Message);
    Console.WriteLine("Програму зупинено. Перевірте файл бази або видаліть його: програма створить нову порожню базу.");
    return;
}

menu.Run();
