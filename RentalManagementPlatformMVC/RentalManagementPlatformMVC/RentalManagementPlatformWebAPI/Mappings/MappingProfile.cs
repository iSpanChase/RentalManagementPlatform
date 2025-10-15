using AutoMapper;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;
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
		}
	}
}
