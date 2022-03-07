using System.Collections.Generic;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using AutoMapper;

namespace Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, AllUsersDto>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(
                        source => source.Name + " " + source.Lastname));

            CreateMap<User, UserDto>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(
                        source => source.Name + " " + source.Lastname))
                .ForMember(
                    dest => dest.RoleName,
                    options => options.MapFrom(
                        source => source.Role.RoleName));
            
            CreateMap<Ticket, TicketDto>()
                .ForMember(
                    dest => dest.ExpirationStatus,
                    options => options.MapFrom(
                        source => source.GetExpirationStatus()))
                .ForMember(
                    dest => dest.UsageStatus,
                    options => options.MapFrom(
                        source => source.GetUsageStatus()))
                .ForMember(
                    dest => dest.TaskId,
                    options => options.MapFrom(
                        source => source.Task.Id));
        }
    }
}