using System.Text;
using FamilyBudget.BusinessLogic;
using FamilyBudget.Persistence;
using FamilyBudget.Presentation;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

const string FilePath = "budget.json";
var storage = new BudgetStorage();
var fileStorage = new JsonFileStorage(FilePath);
var service = new BudgetService(storage, fileStorage);
var reports = new ReportService(service);
var menu = new ConsoleMenu(service, reports);

try
{
    service.Load();
}
catch (Exception ex)
{
    Console.WriteLine("Не вдалося прочитати файл " + Path.GetFullPath(FilePath));
    Console.WriteLine("Причина: " + ex.Message);
    Console.WriteLine("Програму зупинено, щоб не втратити дані. Виправте файл або видаліть його.");
    return;
}

Console.WriteLine("Файл даних: " + Path.GetFullPath(FilePath));
Console.WriteLine("Завантажено: категорій - " + service.GetCategories().Count
    + ", користувачів - " + service.GetUsers().Count
    + ", операцій - " + service.GetTransactions().Count
    + ", планів - " + service.GetPlans().Count
    + ", цілей - " + service.GetSavingGoals().Count);

menu.Run();
