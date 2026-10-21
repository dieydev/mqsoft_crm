#nullable disable
using System;
using System.Collections.Generic;

namespace AI_CRM.Domain.Entities
{
    public enum TicketPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum TicketStatus
    {
        New = 1,
        InProgress = 2,
        Resolved = 3,
        Closed = 4
    }

    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Liên kết Khách hàng y tế (Bệnh viện / Phòng khám)
        public int CustomerId { get; set; }
        public KhachHang Customer { get; set; }

        // Người tạo ticket (Tài khoản người dùng)
        public int? CreatedByUserId { get; set; }
        public NguoiDung CreatedByUser { get; set; }

        // Kỹ thuật viên phụ trách
        public int? AssignedToEmployeeId { get; set; }
        public NhanVienPhuTrach AssignedToEmployee { get; set; }

        // Mức độ ưu tiên & Trạng thái
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public TicketStatus Status { get; set; } = TicketStatus.New;

        // Thời gian & SLA
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }
        public DateTime? SlaDeadline { get; set; }

        // Lịch sử trao đổi trong ticket
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }

    public class TicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int? UserId { get; set; }
        public NguoiDung User { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
