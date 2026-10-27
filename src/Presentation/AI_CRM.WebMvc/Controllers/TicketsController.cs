using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.WebMvc.Models;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Manager,NhanVien")]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<AI_CRM.WebMvc.Hubs.NotificationHub> _hubContext;

        public TicketsController(
            ITicketService ticketService,
            ICustomerService customerService,
            IEmployeeService employeeService,
            ICurrentUserService currentUserService,
            Microsoft.AspNetCore.SignalR.IHubContext<AI_CRM.WebMvc.Hubs.NotificationHub> hubContext)
        {
            _ticketService = ticketService;
            _customerService = customerService;
            _employeeService = employeeService;
            _currentUserService = currentUserService;
            _hubContext = hubContext;
        }

        // GET: /Tickets
        public async Task<IActionResult> Index(string searchString, TicketStatus? status, TicketPriority? priority, int page = 1)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPriority = priority;

            var tickets = await _ticketService.GetTicketsAsync(searchString, status, priority);

            // Gắn thông tin tính toán SLA cho view
            var ticketList = tickets.ToList();
            ViewBag.TotalTickets = ticketList.Count;
            ViewBag.NewCount = ticketList.Count(t => t.Status == TicketStatus.New);
            ViewBag.InProgressCount = ticketList.Count(t => t.Status == TicketStatus.InProgress);
            ViewBag.CriticalCount = ticketList.Count(t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);

            const int pageSize = 10;
            var paginatedTickets = PaginatedList<Ticket>.Create(ticketList, page, pageSize);

            return View(paginatedTickets);
        }

        // GET: /Tickets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var employees = await _employeeService.GetEmployeesAsync(null);
            ViewBag.EmployeeList = new SelectList(employees.Where(e => e.IsActive), "EmployeeId", "FullName", ticket.AssignedToEmployeeId);
            ViewBag.SlaStatus = _ticketService.GetSlaStatus(ticket);

            return View(ticket);
        }

        // GET: /Tickets/Create
        public async Task<IActionResult> Create()
        {
            var customers = await _customerService.GetCustomersAsync(null, null);
            var employees = await _employeeService.GetEmployeesAsync(null);

            ViewBag.CustomerId = new SelectList(customers, "CustomerId", "CompanyName");
            ViewBag.AssignedToEmployeeId = new SelectList(employees.Where(e => e.IsActive), "EmployeeId", "FullName");

            return View(new Ticket { Priority = TicketPriority.Medium });
        }

        // POST: /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                int? userId = _currentUserService.UserId;
                var createdTicket = await _ticketService.CreateTicketAsync(ticket, userId);

                // Gửi thông báo real-time qua SignalR
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                    {
                        title = "🚨 Sự cố Y tế mới!",
                        message = $"Ticket #{createdTicket.Id}: {createdTicket.Title}",
                        type = createdTicket.Priority == TicketPriority.Critical ? "danger" : "warning",
                        timestamp = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
                catch { /* Ignore SignalR broadcast error */ }

                TempData["SuccessMessage"] = "Đã tiếp nhận sự cố và tạo Ticket hỗ trợ thành công!";
                return RedirectToAction(nameof(Index));
            }

            var customers = await _customerService.GetCustomersAsync(null, null);
            var employees = await _employeeService.GetEmployeesAsync(null);
            ViewBag.CustomerId = new SelectList(customers, "CustomerId", "CompanyName", ticket.CustomerId);
            ViewBag.AssignedToEmployeeId = new SelectList(employees.Where(e => e.IsActive), "EmployeeId", "FullName", ticket.AssignedToEmployeeId);

            return View(ticket);
        }

        // GET: /Tickets/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var customers = await _customerService.GetCustomersAsync(null, null);
            var employees = await _employeeService.GetEmployeesAsync(null);

            ViewBag.CustomerId = new SelectList(customers, "CustomerId", "CompanyName", ticket.CustomerId);
            ViewBag.AssignedToEmployeeId = new SelectList(employees.Where(e => e.IsActive), "EmployeeId", "FullName", ticket.AssignedToEmployeeId);

            return View(ticket);
        }

        // POST: /Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _ticketService.UpdateTicketAsync(ticket);
                TempData["SuccessMessage"] = "Cập nhật thông tin Ticket thành công!";
                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }

            var customers = await _customerService.GetCustomersAsync(null, null);
            var employees = await _employeeService.GetEmployeesAsync(null);
            ViewBag.CustomerId = new SelectList(customers, "CustomerId", "CompanyName", ticket.CustomerId);
            ViewBag.AssignedToEmployeeId = new SelectList(employees.Where(e => e.IsActive), "EmployeeId", "FullName", ticket.AssignedToEmployeeId);

            return View(ticket);
        }

        // POST: /Tickets/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ticketService.DeleteTicketAsync(id);
            TempData["SuccessMessage"] = "Đã xóa Ticket sự cố thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToAction(nameof(Details), new { id = ticketId });
            }

            int? userId = _currentUserService.UserId;
            await _ticketService.AddCommentAsync(ticketId, userId, content.Trim());
            TempData["SuccessMessage"] = "Đã gửi phản hồi thành công!";

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // POST: /Tickets/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int ticketId, TicketStatus status)
        {
            await _ticketService.UpdateStatusAsync(ticketId, status);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái ticket sang '{status}'!";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        // POST: /Tickets/AssignTechnician
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTechnician(int ticketId, int employeeId)
        {
            await _ticketService.AssignTechnicianAsync(ticketId, employeeId);
            TempData["SuccessMessage"] = "Đã phân công kỹ thuật viên phụ trách thành công!";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        [HttpPost]
        public async Task<IActionResult> GetAiSolution(int id)
        {
            try
            {
                var solution = await _ticketService.GetTicketAiSolutionAsync(id);
                return Json(new { success = true, solution });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lấy chẩn đoán AI: " + ex.Message });
            }
        }
    }
}
