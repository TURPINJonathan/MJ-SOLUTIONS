namespace api.DTOs
{
	public class ContactResponseDTO
	{
		public int Id { get; set; }
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Position { get; set; }
		public string? Note { get; set; }
		public ICollection<FileResourceDTO>? Files { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }

	}
}