using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Areas.Admin.Services;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: Admin/Payment
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        // GET: Admin/Payment/Invoices
        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            var invoices = await _paymentService.GetAllInvoicesAsync();
            return View(invoices);
        }

        // GET: Admin/Payment/Details/5 (Invoice detail view)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _paymentService.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            return View(invoice);
        }
    }
}
