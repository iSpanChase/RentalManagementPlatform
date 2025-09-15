using AutoMapper;
using Microsoft.Build.Framework.Profiler;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Areas.PointRules.ViewModels;

namespace RentalManagementPlatformMVC.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// Subscription Plan mappings
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

			// Point Rule mappings
			CreateMap<PointRule, PointRuleDto>();
			CreateMap<PointRuleDto, PointRuleIndexRowViewModel>();
			CreateMap<CreatePointRuleDto, PointRule>()
				.ForMember(dest => dest.RuleId, opt => opt.Ignore())
				.ForMember(dest => dest.EarnRatePerNtd, opt => opt.MapFrom(src => (decimal?)src.EarnRatePerNtd))
				.ForMember(dest => dest.RedeemRateNtdPerPt, opt => opt.MapFrom(src => (decimal?)src.RedeemRateNtdPerPt));
			CreateMap<CreatePointRuleViewModel, CreatePointRuleDto>();
			CreateMap<EditPointRuleViewModel, EditPointRuleDto>();

			// Point Ledger mappings
			CreateMap<PointLedger, PointLedgerDto>()
				.ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.Guest != null ? src.Guest.Name : null));
			CreateMap<PointLedgerDto, PointLedgerIndexRowViewModel>();
			CreateMap<PointLedgerSearchCriteriaViewModel, PointLedgerSearchCriteriaDto>();
		}
	}
}
