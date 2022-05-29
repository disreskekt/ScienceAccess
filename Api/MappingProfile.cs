using System.Collections.Generic;
using Api.Helpers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Enums;
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

            CreateMap<TicketRequest, AllUsersRequestDto>();

            CreateMap<User, UserDto>() //dont forget to .Include() Tickets and Role
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(
                        source => source.Name + " " + source.Lastname))
                .ForMember(
                    dest => dest.RoleName,
                    options => options.MapFrom(
                        source => source.Role.RoleName));
            
            CreateMap<User, GetMyselfDto>()
                .ForMember(
                    dest => dest.FullName,
                    options => options.MapFrom(
                        source => source.Name + " " + source.Lastname))
                .ForMember(
                    dest => dest.RoleName,
                    options => options.MapFrom(
                        source => source.Role.RoleName));
            
            CreateMap<Ticket, TicketDto>() //dont forget to .Include() Task
                .ForMember(
                    dest => dest.ExpirationStatus,
                    options => options.MapFrom(
                        source => source.GetExpirationStatus()))
                .ForMember(
                    dest => dest.UsageStatus,
                    options => options.MapFrom(
                        source => source.GetUsageStatus()))
                .ForMember(
                    dest => dest.TaskStatus,
                    options => options.MapFrom(
                        source => source.Task != null ? source.Task.Status : default(TaskStatuses?)));
        }
    }
}