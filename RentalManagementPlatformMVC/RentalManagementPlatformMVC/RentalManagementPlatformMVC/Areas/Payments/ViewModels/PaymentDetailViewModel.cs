using RentalManagementPlatformMVC.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class PaymentDetailViewModel
	{
		// === Payment 基本資訊 ===
		[Display(Name = "付款編號")]
		public int PaymentId { get; set; }

		[Display(Name = "訂單編號")]
		public int? BookingId { get; set; }

		[Display(Name = "付款金額")]
		[DataType(DataType.Currency)]
		[Range(0, double.MaxValue, ErrorMessage = "金額必須為正數")]
		public decimal? Amount { get; set; }

		[Display(Name = "付款方式")]
		[StringLength(30)]
		public string? Method { get; set; }

		[Display(Name = "付款時間")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "付款參考號")]
		[StringLength(100)]
		public string? PaymentRef { get; set; }

		[Display(Name = "訂單編號快照")]
		[StringLength(50)]
		public string? OrderNumberSnapshot { get; set; }

		[Display(Name = "付款狀態")]
		[StringLength(30)]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
		public DateTime? PaymentCreatedAt { get; set; }

		// === Transaction 集合 ===
		[Display(Name = "交易紀錄")]
		public List<PaymentTransactionDto> Transactions { get; set; } = new();

		// === Booking 額外顯示用 ===
		[Display(Name = "房客姓名")]
		[StringLength(100)]
		public string? GuestName { get; set; }

		[Display(Name = "房間名稱")]
		[StringLength(200)]
		public string? RoomTitle { get; set; }

		// === 顯示用屬性 ===
		[Display(Name = "付款方式")]
		public string DisplayMethod => Method?.ToLower() switch
		{
			"credit_card" => "信用卡",
			_ => Method ?? "其他"
		};

		[Display(Name = "付款狀態")]
		public string DisplayStatus => Status?.ToLower() switch
		{
			"paid" => "已付款",
			"pending" => "付款中",
			"refunded" => "已退款",
			"failed" => "付款失敗",
			_ => "未知"
		};
	}
}