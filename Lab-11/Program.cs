using Lab_11;
using static System.Net.Mime.MediaTypeNames;

Console.WriteLine("Hello, World!");

await using var db = new DataContext();

if (File.Exists("app.db"))
{
    File.Delete("app.db");
    Console.WriteLine("Старая БД удалена");
}

await db.Database.EnsureCreatedAsync();
Console.WriteLine("БД создана заново");