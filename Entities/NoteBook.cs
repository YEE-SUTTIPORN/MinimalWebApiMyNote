using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiMyNote.Entities
{
    public class NoteBook
    {
        [Key]
        public int NoteId { get; set; }
        public required string NoteTitle { get; set; }
        public required string NoteDescription { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        [ForeignKey("User")]
        public required int UserId { get; set; }
        //public User User { get; set; }
        [ForeignKey("Category")]
        public required int CategoryId { get; set; }
        //public Category Category { get; set; }
    }
}
