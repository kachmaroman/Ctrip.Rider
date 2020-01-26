using Ctrip.Rider.Dtos;

namespace Ctrip.Rider.Helpers
{
	public static class Validator
	{
		public static ValidationResult Validate(UserLoginDto user)
		{
			ValidationResult result = new ValidationResult();

			if (!IsEmailValid(user.Email))
			{
				result.ErorMessage = "Please provide a valid email";
			}
			else if (!IsPasswordValid(user.Password))
			{
				result.ErorMessage = "Please provide a valid password";
			}

			result.IsValid = string.IsNullOrEmpty(result.ErorMessage);

			return result;
		}

		public static ValidationResult Validate(UserRegisterDto user)
		{
			ValidationResult result = new ValidationResult();

			if (!IsFullnameValid(user.Fullname))
			{
				result.ErorMessage = "Please enter a valid name";
			}
			else if (!IsPhoneValid(user.Phone))
			{
				result.ErorMessage = "Please enter a valid phone number";
			}
			else if (!IsEmailValid(user.Email))
			{
				result.ErorMessage = "Please provide a valid email";
			}
			else if (!IsPasswordValid(user.Password))
			{
				result.ErorMessage = "Please provide a valid password";
			}

			result.IsValid = string.IsNullOrEmpty(result.ErorMessage);

			return result;
		}

		private static bool IsEmailValid(string email)
		{
			return email.Contains("@");
		}

		private static bool IsPasswordValid(string password)
		{
			return password.Length >= 8;
		}

		private static bool IsFullnameValid(string fullname)
		{
			return fullname.Length > 3;
		}

		private static bool IsPhoneValid(string phone)
		{
			return phone.Length >= 9;
		}
	}

	public class ValidationResult
	{
		public bool IsValid { get; set; }

		public string ErorMessage { get; set; }
	}
}