using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_11
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Note> UserNotes { get; } = new List<Note>();
    }
    public class Note
    {
        [Key]
        public int Id { get; set; }
        public required string Text { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; } = null!;
    }
}