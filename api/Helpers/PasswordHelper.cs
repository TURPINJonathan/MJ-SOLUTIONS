namespace api.Helpers
{
	public static class PasswordHelper
	{
		public static bool IsPasswordValid(string password)
		{
			return !string.IsNullOrWhiteSpace(password)
					&& password.Length >= 8
					&& password.Any(char.IsUpper)
					&& password.Any(char.IsLower)
					&& password.Any(char.IsDigit)
					&& password.Any(ch => !char.IsLetterOrDigit(ch));
		}

	}
		
}