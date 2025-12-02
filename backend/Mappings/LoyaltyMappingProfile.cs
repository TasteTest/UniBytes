using AutoMapper;
using backend.Common.Enums;
using backend.Models;
using backend.DTOs.Loyalty.Request;
using backend.DTOs.Loyalty.Response;

namespace backend.Mappings;

/// <summary>
/// AutoMapper profile for Model-DTO mappings
/// </summary>
public class LoyaltyMappingProfile : Profile
{
    public LoyaltyMappingProfile()
    {
        // LoyaltyAccount mappings
        CreateMap<LoyaltyAccount, LoyaltyAccountResponse>()
            .ForMember(dest => dest.TierName, opt => opt.MapFrom(src => src.Tier.ToString()));

        CreateMap<CreateLoyaltyAccountRequest, LoyaltyAccount>()
            .ForMember(dest => dest.Tier, opt => opt.MapFrom(src => (LoyaltyTier)src.Tier))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LoyaltyTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.LoyaltyRedemptions, opt => opt.Ignore());

        CreateMap<UpdateLoyaltyAccountRequest, LoyaltyAccount>()
            .ForMember(dest => dest.Tier, opt => opt.MapFrom(src => src.Tier.HasValue ? (LoyaltyTier)src.Tier.Value : default))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // LoyaltyTransaction mappings
        CreateMap<LoyaltyTransaction, LoyaltyTransactionResponse>();

        // LoyaltyRedemption mappings
        CreateMap<LoyaltyRedemption, LoyaltyRedemptionResponse>();
    }
}
