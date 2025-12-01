using AutoMapper;
using backend_monolith.DTOs.Request;
using backend_monolith.DTOs.Response;
using backend_monolith.Modelss;

namespace backend_monolith.Mappings;

/// <summary>
/// AutoMapper profile for entity-to-DTO mappings
/// </summary>
public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        // Payment mappings
        CreateMap<Payment, PaymentResponse>();
        CreateMap<CreatePaymentRequest, Payment>();
    }
}

