namespace api.Utils
{
	public static class DateUtils
	{
		public static DateTime CurrentDateTimeUtils(string timeZoneId = "Europe/Paris")
		{
			return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
		}

	}
		
}