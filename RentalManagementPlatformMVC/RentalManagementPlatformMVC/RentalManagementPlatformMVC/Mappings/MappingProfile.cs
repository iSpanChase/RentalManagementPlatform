using AutoMapper;
using Microsoft.Build.Framework.Profiler;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;

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
			CreateMap<EditPlanViewModel, EditPlanDto>();
			CreateMap<HostSubscription, HostSubscriptionDto>()
				.ForMember(dest => dest.HostName, opt => opt.MapFrom(src => src.Host.Name))
				.ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.PlanName));
			CreateMap<HostSubscription, HostSubscriptionDetailDto>()
				.ForMember(dest => dest.HostName, opt => opt.MapFrom(src => src.Host.Name))
				.ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.PlanName))
				.ForMember(dest => dest.Billings, opt => opt.MapFrom(src => src.SubscriptionBillingLogs));
			CreateMap<SubscriptionBillingLog, HostSubscirptionBillingDto>();
			CreateMap<HostSubscriptionDto, HostSubscriptionIndexRowViewModel>();
			CreateMap<HostSubscriptionDetailDto, HostSubscriptionDetailViewModel>()
				.ForMember(dest => dest.Billings, opt => opt.MapFrom(src => src.Billings));
			CreateMap<HostSubscirptionBillingDto, HostSubscriptionBillingViewModel>();
		}
	}
}
