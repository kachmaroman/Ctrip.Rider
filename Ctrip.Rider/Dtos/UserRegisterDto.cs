using System;
namespace Ctrip.Rider.Dtos
{
	public class UserRegisterDto
	{
		public string Email { get; set; }

		public string Password { get; set; }

		public string Fullname { get; set; }

		public string Phone { get; set; }
	}
}