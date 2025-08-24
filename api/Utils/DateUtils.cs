

namespace api.Utils
{
	public static class DateUtils
	{
		public static DateTime CurrentDateTimeUtils()
		{
			return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));
		}

	}

}