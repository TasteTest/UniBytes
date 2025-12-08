using AutoMapper;
using backend.Models;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;

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

