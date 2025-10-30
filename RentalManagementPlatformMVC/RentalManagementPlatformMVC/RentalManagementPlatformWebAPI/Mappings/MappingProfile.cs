using AutoMapper;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.DTO.RoomList;
using RentalManagementPlatformWebAPI.Models;
using System.Runtime;

namespace RentalManagementPlatformWebAPI.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Booking, BookingDto>()
				.ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest.Name))
				.ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room.Title));
			CreateMap<Payment, PaymentsDto>()
				.ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Booking.BookingId));
			CreateMap<Payment, CreatePaymentDto>();

            CreateMap<Review, ReviewDto>();
            CreateMap<CreateReviewDto, Review>();
		}
	}
}
