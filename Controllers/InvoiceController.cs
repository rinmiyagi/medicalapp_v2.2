using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using medicalapp.Models;

namespace medicalapp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoiceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Generate Invoice for an appointment
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Generate(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Check if invoice already exists
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

            if (existingInvoice != null)
            {
                return RedirectToAction("Details", new { id = existingInvoice.Id });
            }

            // Generate invoice number
            var invoiceCount = await _context.Invoices.CountAsync() + 1;
            var invoiceNumber = $"INV-{DateTime.Now.Year}-{invoiceCount.ToString("D4")}";

            var invoice = new Invoice
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                InvoiceNumber = invoiceNumber,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                SubTotal = appointment.ConsultationFee,
                TaxAmount = appointment.TaxAmount,
                TotalAmount = appointment.TotalAmount,
                Status = "Unpaid",
                CreatedBy = User.Identity?.Name,
                Notes = $"Consultation with Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}"
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = invoice.Id });
        }

        // GET: Invoice Details
        [Authorize(Roles = "Admin,Receptionist,Patient")]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Mark Invoice as Paid
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> MarkAsPaid(int id, string paymentMethod)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            invoice.Status = "Paid";
            invoice.PaymentDate = DateTime.Now;
            invoice.PaymentMethod = paymentMethod;
            invoice.PaymentReference = $"PAY-{DateTime.Now.Year}-{new Random().Next(10000, 99999)}";

            // Update appointment payment status
            var appointment = await _context.Appointments.FindAsync(invoice.AppointmentId);
            if (appointment != null)
            {
                appointment.IsPaid = true;
                appointment.PaymentDate = DateTime.Now;
                appointment.PaymentMethod = paymentMethod;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Invoice marked as paid!";
            return RedirectToAction("Details", new { id = id });
        }

        // GET: All Invoices (Admin or Receptionist)
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Index(string status = "All", string lhdnStatus = "All")
        {
            var query = _context.Invoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.User)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                .AsQueryable();

            // Filter by payment status
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(i => i.Status == status);
            }

            // Filter by LHDN sync status
            if (!string.IsNullOrEmpty(lhdnStatus) && lhdnStatus != "All")
            {
                if (lhdnStatus == "Synced")
                {
                    query = query.Where(i => i.IsSyncedWithLhdn == true);
                }
                else if (lhdnStatus == "Not Synced")
                {
                    query = query.Where(i => i.IsSyncedWithLhdn == false);
                }
            }

            var invoices = await query
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedLhdnStatus = lhdnStatus;

            return View(invoices);
        }

        // GET: Patient's Invoices
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyInvoices()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var invoices = await _context.Invoices
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(i => i.PatientId == patient.Id)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(invoices);
        }

        // POST: Sync with LHDN (E-Invoice Compliance)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SyncWithLhdn(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            // Simulate LHDN e-invoice submission
            invoice.IsSyncedWithLhdn = true;
            invoice.LhdnReferenceId = $"LHDN-{DateTime.Now.Year}-{new Random().Next(100000, 999999)}";
            invoice.LhdnSyncDate = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Invoice synced with LHDN!";
            return RedirectToAction("Details", new { id = id });
        }
    }
}