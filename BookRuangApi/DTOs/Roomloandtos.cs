using System;
using System.ComponentModel.DataAnnotations;

namespace BookRuangApi.DTOs
{
    // DTO untuk create peminjaman
    public class CreateRoomLoanDto
    {
        [Required(ErrorMessage = "Nama peminjam wajib diisi")]
        [StringLength(100)]
        public string BorrowerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama ruangan wajib diisi")]
        [StringLength(100)]
        public string RoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tujuan peminjaman wajib diisi")]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal mulai wajib diisi")]
        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "Tanggal selesai wajib diisi")]
        public DateTime? EndTime { get; set; }
    }

    // DTO untuk update peminjaman
    public class UpdateRoomLoanDto
    {
        [Required]
        [StringLength(100)]
        public string BorrowerName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RoomName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    // DTO untuk approve/reject
    public class UpdateStatusDto
    {
        [Required(ErrorMessage = "Nama approver/rejector wajib diisi")]
        public string UpdatedBy { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // DTO untuk filter
    public class RoomLoanFilterDto
    {
        public string? Status { get; set; }
        public string? RoomName { get; set; }
        public string? BorrowerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Response DTO dengan informasi lengkap
    public class RoomLoanResponseDto
    {
        public int Id { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}