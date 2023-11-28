using System.ComponentModel.DataAnnotations;

namespace WebApiMyNote.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
