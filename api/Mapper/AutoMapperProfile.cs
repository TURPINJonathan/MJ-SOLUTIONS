using AutoMapper;
using api.Models;
using api.DTOs;
using api.Enums;

namespace api.Mapper
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Project, ProjectResponseDTO>()
					.ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
					.ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
					.ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Files))
					.ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => Enum.GetName(typeof(StatusEnum), src.Status)))
					.ForMember(dest => dest.DeveloperRoleName, opt => opt.MapFrom(src => Enum.GetName(typeof(DeveloperRoleEnum), src.DeveloperRole)));

			CreateMap<Skill, SkillResponseDTO>()
					.ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type != null ? Enum.GetName(typeof(SkillTypeEnum), src.Type) : null))
					.ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Files));

			CreateMap<FileResource, FileResourceDTO>();

			CreateMap<User, UserResponseDTO>()
					.ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(p => p.Name)))
					.ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.HasValue ? Enum.GetName(typeof(UserRoleEnum), src.Role.Value) : null));

			CreateMap<Contact, ContactResponseDTO>()
					.ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Files));
		}

	}
		
}