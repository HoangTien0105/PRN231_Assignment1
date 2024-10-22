using AutoMapper;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Enums;
using UberSytem.Dto.Requests;
using UberSytem.Dto.Responses;

namespace UberSytem.Dto
{
    public class MappingProfileExtension : Profile
    {
        /// <summary>
        /// Mapping
        /// </summary>
        public MappingProfileExtension()
        {
            CreateMap<User, Customer>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => Helper.GenerateRandomLong()));
            CreateMap<User, Driver>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => Helper.GenerateRandomLong()));

            CreateMap<User, UserResponseModel>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<UserResponseModel, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)));              
            
            //CreateMap<Trip, UserResponseModel>()
            //    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            //CreateMap<UserResponseModel, User>()
            //    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)));            
            
            CreateMap<User, CustomerDTO>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<CustomerDTO, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)));

            CreateMap<SignupModel, User>();
            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<Trip, TripDTO>().ReverseMap();


        }
    }
}
