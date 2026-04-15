using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lab_11
{
    public class Crud
    {
        public static async Task<Note> Create(DataContext db, string text, CancellationToken ct = default)
        {
            var note = new Note
            {
                Text = text,
                CreatedAt = DateTime.Now
            };
            db.Notes.Add(note);
            await db.SaveChangesAsync(ct);
            return note;
        }
        public static async Task<List<Note>> Read(DataContext db, string search, CancellationToken ct = default)
        {
            var result = await db.Notes
                .Where(x => EF.Functions.Like(x.Text, $"%{search}%"))
                .ToListAsync(ct);
            return result;
        }
        public static async Task<Note?> Read(DataContext db, int id, CancellationToken ct = default)
        {
            return await db.Notes.FirstOrDefaultAsync(x => x.Id == id, ct);
        }
        public static async Task Update(DataContext db, Note note, string text, DateTimeOffset createdAt, CancellationToken ct = default)
        {
            note.Text = text;
            note.CreatedAt = createdAt;
            db.Notes.Update(note);
            await db.SaveChangesAsync(ct);
        }
        public static async Task Delete(DataContext db, Note note, CancellationToken ct = default)
        {
            db.Notes.Remove(note);
            await db.SaveChangesAsync(ct);
        }


        public static async Task<User> CreateUser(DataContext db, string name, CancellationToken ct = default)
        {
            var user = new User
            {
                Name = name,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            return user;
        }
        public static async Task<User?> ReadUsers(DataContext db, int id, CancellationToken ct = default)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }
        public static async Task UpdateUser(DataContext db, User user, string newName, CancellationToken ct = default)
        {
            user.Name = newName;
            db.Users.Update(user);
            await db.SaveChangesAsync(ct);
        }
        public static async Task DeleteUser(DataContext db, User user, CancellationToken ct = default)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync(ct);
        }
        public static async Task<List<Note>> GetUserNotes(DataContext db, int userId, CancellationToken ct = default)
        {
            var user = await db.Users
                .Include(u => u.UserNotes)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            return user?.UserNotes.ToList() ?? new List<Note>();
        }
    }
}
