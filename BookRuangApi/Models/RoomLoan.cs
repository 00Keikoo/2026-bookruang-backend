using System;
using System.ComponentModel.DataAnnotations;

namespace BookRuangApi.Models{
	public class RoomLoan{
		public int Id {get; set; }

		[Required]
		public string BorrowerName {get; set; }

		[Required]
		public string RoomName {get; set; }

		[Required]
		public string Purpose {get; set;} 

		public string Status {get; set; } = "Pending";

		public DateTime Date {get; set; } = DateTime.Now;
	}
}
