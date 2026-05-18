using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lab_11.Tests
{
    public class CompleteTests : IDisposable
    {
        private readonly SqliteConnection _connection;

        public CompleteTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        private DataContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(_connection) 
                .Options;

            var context = new DataContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        //note tests
        [Fact]
        public async Task Note_Create_AddsToDatabase()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Пользователь");

            var note = new Note { Text = "клац клац", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.Add(note);
            await db.SaveChangesAsync();

            var fromDb = await db.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("клац клац", fromDb.Text);
        }

        [Fact]
        public async Task Note_ReadBySearch_ReturnsFiltered()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Анечка");

            var note1 = new Note { Text = "емае, я усталь", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            var note2 = new Note { Text = "дорогой дневник...", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.AddRange(note1, note2);
            await db.SaveChangesAsync();

            var result = await Crud.Read(db, "емае");

            Assert.Single(result);
            Assert.Equal("емае, я усталь", result[0].Text);
        }

        [Fact]
        public async Task Note_ReadById_ReturnsCorrect()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Бусинка");

            var note = new Note { Text = "покатилась", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.Add(note);
            await db.SaveChangesAsync();

            var found = await Crud.Read(db, note.Id);

            Assert.NotNull(found);
            Assert.Equal("покатилась", found.Text);
        }

        [Fact]
        public async Task Note_Update_ModifiesExisting()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Паймон");

            var note = new Note { Text = "Лучший проводник!", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.Add(note);
            await db.SaveChangesAsync();

            var newDate = DateTimeOffset.Now;

            await Crud.Update(db, note, "Летающая консерва", newDate);

            var updated = await db.Notes.FindAsync(note.Id);
            Assert.Equal("Летающая консерва", updated.Text);
            Assert.Equal(newDate, updated.CreatedAt);
        }

        [Fact]
        public async Task Note_Delete_RemovesFromDatabase()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Тьма");

            var note = new Note { Text = "тут пусто", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.Add(note);
            await db.SaveChangesAsync();

            await Crud.Delete(db, note);

            var deleted = await db.Notes.FindAsync(note.Id);
            Assert.Null(deleted);
        }

        //user tests
        [Fact]
        public async Task User_Create_AddsToDatabase()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Машенька");

            var fromDb = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("Машенька", fromDb.Name);
        }

        [Fact]
        public async Task User_ReadById_ReturnsCorrect()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Егорчик");

            var found = await Crud.ReadUsers(db, user.Id);

            Assert.NotNull(found);
            Assert.Equal("Егорчик", found.Name);
        }

        [Fact]
        public async Task User_Update_ChangesName()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Егор");

            await Crud.UpdateUser(db, user, "Алефтиния");

            var updated = await db.Users.FindAsync(user.Id);
            Assert.Equal("Алефтиния", updated.Name);
        }

        [Fact]
        public async Task User_Delete_RemovesFromDatabase()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "бу, испугался?");

            await Crud.DeleteUser(db, user);

            var deleted = await db.Users.FindAsync(user.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task User_GetNotes_ReturnsUserNotes()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Машенька");

            var note1 = new Note { Text = "Я круто порисовала", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            var note2 = new Note { Text = "А ещё поиграла в геншин", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.AddRange(note1, note2);
            await db.SaveChangesAsync();

            var notes = await Crud.GetUserNotes(db, user.Id);

            Assert.Equal(2, notes.Count);
            Assert.Contains(notes, n => n.Text == "Я круто порисовала");
            Assert.Contains(notes, n => n.Text == "А ещё поиграла в геншин");
        }

        [Fact]
        public async Task User_GetNotes_UserNotFound_ReturnsEmpty()
        {
            using var db = CreateContext();
            var notes = await Crud.GetUserNotes(db, 999);
            Assert.Empty(notes);
        }

        //rela tests
        [Fact]
        public async Task Relationship_UserCanHaveMultipleNotes()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Анна");

            var note1 = new Note { Text = "Я сдам мат анализ!", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            var note2 = new Note { Text = "Не сдала, плаки плаки", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.AddRange(note1, note2);
            await db.SaveChangesAsync();

            var loadedUser = await db.Users
                .Include(u => u.UserNotes)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.NotNull(loadedUser);
            Assert.Equal(2, loadedUser.UserNotes.Count);
        }

        [Fact]
        public async Task Relationship_NoteBelongsToUser()
        {
            using var db = CreateContext();
            var user = await Crud.CreateUser(db, "Алёна");

            var note = new Note { Text = "сдала все предметы)", CreatedAt = DateTimeOffset.Now, UserId = user.Id };
            db.Notes.Add(note);
            await db.SaveChangesAsync();

            var loadedNote = await db.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == note.Id);

            Assert.NotNull(loadedNote.User);
            Assert.Equal("Алёна", loadedNote.User.Name);
        }
    }
}