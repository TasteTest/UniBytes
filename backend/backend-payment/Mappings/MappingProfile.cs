using AutoMapper;
using backend_payment.DTOs.Request;
using backend_payment.DTOs.Response;
using backend_payment.Model;

namespace backend_payment.Mappings;

/// <summary>
/// AutoMapper profile for entity-to-DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Payment mappings
        CreateMap<Payment, PaymentResponse>();
        CreateMap<CreatePaymentRequest, Payment>();
    }
}

