using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Areas.Payments.ViewModels;
using RentalManagementPlatformMVC.Services;

namespace RentalManagementPlatformMVC.Areas.Payments.Controllers
{
	public class PaymentsHomeController : Controller
	{
		private readonly IPaymentService _paymentService;

		public PaymentsHomeController(IPaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		[Area("Payments")]
		public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 20)
		{
			var pagedPayments = await _paymentService.GetPagedPaymentsAsync(pageIndex, pageSize);

			var vm = new PaymentIndexViewModel
			{
				Payments = pagedPayments.Items.Select(p => new PaymentIndexRowViewModel
				{
					PaymentId = p.PaymentId,
					OrderNumberSnapshot = p.OrderNumberSnapshot,
					Amount = p.Amount,
					PaymentRef = p.PaymentRef,
					Method = p.Method,
					PaidAt = p.PaidAt,
					Status = p.Status,
					CreatedAt = p.CreatedAt,
				}).ToList(),

				PageIndex = pagedPayments.PageIndex,
				PageSize = pageSize,
				TotalPages = pagedPayments.TotalPages,
				TotalCount = pagedPayments.TotalCount,
			};

			return View(vm);
		}
	}
}
