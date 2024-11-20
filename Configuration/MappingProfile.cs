using AutoMapper;
using TMSWebApi.DTOs;
using TMSWebApi.Entities;
using TMSWebApi.Enums;

namespace TMSWebApi.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WorkTaskDto, WorkTask>()
            .ForMember(dest => dest.Status,
                opt =>
                    opt.MapFrom(src => Enum.Parse<Status>(src.Status)));
        
        CreateMap<WorkTask, WorkTaskDto>()
            .ForMember(dest => dest.Status,
                opt =>
                    opt.MapFrom(src => src.Status.ToString()));
    }
}