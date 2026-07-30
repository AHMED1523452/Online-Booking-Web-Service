using Application.Features.Users.DTOs;
using AutoMapper;
using Domain.Entities;
namespace Application.Profiles
{
    public class UsersProfile : Profile
    {
        public UsersProfile()
        {
            CreateMap<passenger, UserDetailsResponseDTO>();
        }
    }
}
