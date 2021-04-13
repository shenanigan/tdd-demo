using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDD
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public String RoomType { get; set; }

        public int CurrentCapacity { get; set; }

        public int MaxCapacity { get; set; }
    }
}