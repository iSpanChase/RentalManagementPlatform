using RentalManagementPlatformMVC.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class PaymentDetailViewModel
	{
		// === Payment 基本資訊 ===
		public int PaymentId { get; set; }
		public int? BookingId { get; set; }
		public decimal? Amount { get; set; }
		public string? Method { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? PaymentRef { get; set; }
		public string? OrderNumberSnapshot { get; set; }
		public string? Status { get; set; }
		public DateTime? PaymentCreatedAt { get; set; }

		// === Transaction 集合 ===
		public List<PaymentTransactionDto> Transactions { get; set; } = new();

		// === Booking 額外顯示用 ===
		public string? GuestName { get; set; }
		public string? RoomTitle { get; set; }

		// === 顯示用屬性 ===
		public string DisplayMethod => Method?.ToLower() switch
		{
			"credit_card" => "信用卡",
			_ => Method ?? "其他"
		};

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
