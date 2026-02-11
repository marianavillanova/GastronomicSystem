using AutoMapper;
using RestaurantAPI.Models;
using RestaurantAPI.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Shift, ShiftDto>();
    }
}
