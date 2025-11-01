using AutoMapper;
using backend_user.DTOs.Request;
using backend_user.DTOs.Response;
using backend_user.Model;

namespace backend_user.Mappings;

/// <summary>
/// AutoMapper profile for Model-DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserResponse>();
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
            .ForMember(dest => dest.OAuthProviders, opt => opt.Ignore())
            .ForMember(dest => dest.UserAnalytics, opt => opt.Ignore());

        CreateMap<UpdateUserRequest, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // OAuth Provider mappings
        CreateMap<OAuthProvider, OAuthProviderResponse>();
        CreateMap<CreateOAuthProviderRequest, OAuthProvider>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<UpdateOAuthProviderRequest, OAuthProvider>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // User Analytics mappings
        CreateMap<UserAnalytics, UserAnalyticsResponse>()
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress != null ? src.IpAddress.ToString() : null));

        CreateMap<CreateUserAnalyticsRequest, UserAnalytics>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.IpAddress) ? System.Net.IPAddress.Parse(src.IpAddress) : null));
    }
}
