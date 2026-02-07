using Microsoft.EntityFrameworkCore;
using BookRuangApi.Models;

namespace BookRuangApi{
	public class DataContext : DbContext{
		public DataContext(DbContextOptions<DataContext> options) : base(options) {}

		public DbSet<RoomLoan> RoomLoans {get; set; }
	}
}
