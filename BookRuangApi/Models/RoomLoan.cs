using System;
using System.ComponentModel.DataAnnotations;

namespace BookRuangApi.Models
{
	public class RoomLoan
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Nama peminjam wajib diisi")]
		[StringLength(100, ErrorMessage = "Nama peminjam maksimal 100 karakter")]
		public string BorrowerName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Nama ruangan wajib diisi")]
		[StringLength(100, ErrorMessage = "Nama ruangan maksimal 100 karakter")]
		public string RoomName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Tujuan peminjaman wajib diisi")]
		[StringLength(500, ErrorMessage = "Tujuan peminjaman maksimal 500 karakter")]
		public string Purpose { get; set; } = string.Empty;

		[Required]
		public string Status { get; set; } = "Pending";

		public DateTime Date { get; set; } = DateTime.Now;

		// Untuk manajemen status
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		public string? ApprovedBy { get; set; }
		public DateTime? ApprovedAt { get; set; }

		public string? RejectedBy { get; set; }
		public DateTime? RejectedAt { get; set; }

		[StringLength(1000)]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime? UpdatedAt { get; set; }
	}

	// Enum untuk status (bisa digunakan untuk validasi)
	public static class RoomLoanStatus
	{
		public const string Pending = "Pending";
		public const string Approved = "Approved";
		public const string Rejected = "Rejected";
		public const string Cancelled = "Cancelled";

		public static bool IsValid(string status)
		{
			return status == Pending || status == Approved ||
				   status == Rejected || status == Cancelled;
		}
	}
}