using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class FileResourceDTO
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }
		[JsonPropertyName("fileName")]
		public string? FileName { get; set; }
		[JsonPropertyName("filePath")]
		public string? FilePath { get; set; }
		[JsonPropertyName("isBanner")]
		public bool? IsBanner { get; set; }
		[JsonPropertyName("isLogo")]
		public bool? IsLogo { get; set; }
		[JsonPropertyName("isMaster")]
		public bool? IsMaster { get; set; }
		[JsonPropertyName("description")]
		public string? Description { get; set; }
	}

}