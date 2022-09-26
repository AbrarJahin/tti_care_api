using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StartupProject_Asp.NetCore_PostGRE.ViewModels.Api
{
	public class JwtViewModel
	{
		public string Token { get; set; }
		public string RefreshToken { get; set; }
		public DateTime ExpiresAt { get; set; }
	}
}
