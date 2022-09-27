using System;

namespace StartupProject_Asp.NetCore_PostGRE.ViewModels.Api
{
	public class JwtViewModel
	{
		public string Token { get; set; }
		public string RefreshToken { get; set; }
		public DateTime ExpiresAt { get; set; }
		public string Error { get; set; }
	}
}
