using AutoMapper;
using api.Models;
using api.DTOs;
using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Utils
{
	public static class UserUtils
	{
		public static async Task FillProjectUsersAsync(AppDbContext context, IMapper mapper, Project project, ProjectResponseDTO response)
		{
			if (project.CreatedById != 0)
			{
				var user = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == project.CreatedById);
				if (user != null)
					response.CreatedBy = mapper.Map<UserResponseDTO>(user);
			}
			if (project.UpdatedById != null)
			{
				var updatedBy = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == project.UpdatedById);
				if (updatedBy != null)
					response.UpdatedBy = mapper.Map<UserResponseDTO>(updatedBy);
			}
			if (project.DeletedById != null)
			{
				var deletedBy = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == project.DeletedById);
				if (deletedBy != null)
					response.DeletedBy = mapper.Map<UserResponseDTO>(deletedBy);
			}
			if (project.PublishedById != null)
			{
				var publishedBy = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == project.PublishedById);
				if (publishedBy != null)
					response.PublishedBy = mapper.Map<UserResponseDTO>(publishedBy);
			}
		}

	}
		
}