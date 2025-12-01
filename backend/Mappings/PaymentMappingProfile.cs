using AutoMapper;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;

namespace backend.Mappings;

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

