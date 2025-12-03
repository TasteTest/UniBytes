using AutoMapper;
using System.Text.Json;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;
using backend.Models;

namespace backend.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<CreateOrderRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<Order, OrderResponse>();
    }
}