using AutoMapper;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs.Bookings;
using RentalManagementPlatformWebAPI.DTOs.Payments;
using RentalManagementPlatformWebAPI.Models;
using System.Runtime;

namespace RentalManagementPlatformWebAPI.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Booking, BookingDto>()
				// 原有的對應
				.ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest.Name))
				.ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room.Title));
			CreateMap<Payment, PaymentsDto>()
				.ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Booking.BookingId));
			CreateMap<Payment, CreatePaymentDto>();
		}
	}
}
