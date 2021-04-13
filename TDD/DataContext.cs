using Microsoft.EntityFrameworkCore;

namespace TDD
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        // For storing the list of patients and their state
        public DbSet<Patient> Patient { get; set; }

        // For the storying the rooms along with their types and capacity
        public DbSet<Room> Room { get; set; }

        // For logging which patients are currently admitted to which room
        public DbSet<RoomPatient> RoomPatient { get; set; }

    }
}