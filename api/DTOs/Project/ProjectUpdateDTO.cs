using api.Enums;

namespace api.DTOs
{
	public class ProjectUpdateDTO
	{
		public string? Name { get; set; }
		public string? Overview { get; set; }
		public string? Description { get; set; }
		public string? Slug { get; set; }
		public string? Url { get; set; }
		public string? GithubUrl { get; set; }
		public DeveloperRoleEnum? DeveloperRole { get; set; }
		public StatusEnum? Status { get; set; }
		public bool? IsOnline { get; set; }
	}
		
}