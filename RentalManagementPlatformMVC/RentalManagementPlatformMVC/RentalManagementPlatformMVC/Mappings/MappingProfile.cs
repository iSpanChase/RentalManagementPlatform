using AutoMapper;
using Microsoft.Build.Framework.Profiler;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;

namespace RentalManagementPlatformMVC.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<SubscriptionPlan, SubscriptionPlanDto>();
			CreateMap<SubscriptionPlanDto, SubscriptionPlanIndexRowViewModel>();
			CreateMap<CreatePlanDto, SubscriptionPlan>();
			CreateMap<CreatePlanViewModel, CreatePlanDto>();
		}
	}
}
