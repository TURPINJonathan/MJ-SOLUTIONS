using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace api.Utils
{
	public static class StringUtils
	{
		public static string SanitizeString(string input)
		{
			var normalized = input.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder();
			foreach (var c in normalized)
			{
				var uc = CharUnicodeInfo.GetUnicodeCategory(c);
				if (uc != UnicodeCategory.NonSpacingMark)
					sb.Append(c);
			}
			var slug = sb.ToString().Normalize(NormalizationForm.FormC);

			slug = slug.ToLowerInvariant();
			slug = Regex.Replace(slug, @"[^a-z0-9\-]", "-");
			slug = Regex.Replace(slug, @"-+", "-").Trim('-');
			return slug;
		}

	}

}