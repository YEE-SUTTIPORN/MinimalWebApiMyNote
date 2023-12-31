﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiMyNote.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public string CategoryDescription { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        [ForeignKey("User")]
        public required int UserId { get; set; }
        //public User User { get; set; }
    }
}
