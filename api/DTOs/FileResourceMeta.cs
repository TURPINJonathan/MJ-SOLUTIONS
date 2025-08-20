using System.Text.Json.Serialization;

namespace api.DTOs
{
	public class FileResourceMeta
	{
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