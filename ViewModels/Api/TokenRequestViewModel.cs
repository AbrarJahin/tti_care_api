using System;
using System.ComponentModel.DataAnnotations;

namespace StartupProject_Asp.NetCore_PostGRE.ViewModels.Api
{
	public class TokenRequestViewModel
	{
		[Required]
		public string Token { get; set; }
		[Required]
		public string RefreshToken { get; set; }
	}
}
