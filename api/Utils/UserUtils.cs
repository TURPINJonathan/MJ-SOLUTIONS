using AutoMapper;
using api.Models;
using api.DTOs;
using api.Data;
using api.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace api.Utils
{
	public static class UserUtils
	{
		public static async Task FillModelUsersAsync<TModel, TResponse>(
				AppDbContext context,
				IMapper mapper,
				TModel model,
				TResponse response)
				where TModel : IUserTrackable
				where TResponse : class
		{
			// CreatedBy
			if (model.CreatedById != 0)
			{
				var user = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == model.CreatedById);
				if (user != null)
					response.GetType().GetProperty("CreatedBy")?.SetValue(response, mapper.Map<UserResponseDTO>(user));
			}

			// UpdatedBy
			if (model.UpdatedById != null)
			{
				var updatedBy = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == model.UpdatedById);
				if (updatedBy != null)
					response.GetType().GetProperty("UpdatedBy")?.SetValue(response, mapper.Map<UserResponseDTO>(updatedBy));
			}

			// DeletedBy
			if (model.DeletedById != null)
			{
				var deletedBy = await context.Users
						.Include(u => u.Permissions)
						.FirstOrDefaultAsync(u => u.Id == model.DeletedById);
				if (deletedBy != null)
					response.GetType().GetProperty("DeletedBy")?.SetValue(response, mapper.Map<UserResponseDTO>(deletedBy));
			}

			// PublishedBy (optionnel, via réflexion)
			var publishedByProp = typeof(TModel).GetProperty("PublishedById");
			if (publishedByProp != null)
			{
				var publishedById = publishedByProp.GetValue(model) as int?;
				if (publishedById != null)
				{
					var publishedBy = await context.Users
							.Include(u => u.Permissions)
							.FirstOrDefaultAsync(u => u.Id == publishedById);
					if (publishedBy != null)
						response.GetType().GetProperty("PublishedBy")?.SetValue(response, mapper.Map<UserResponseDTO>(publishedBy));
				}
			}

		}

	}
		
}