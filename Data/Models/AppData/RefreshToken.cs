using StartupProject_Asp.NetCore_PostGRE.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StartupProject_Asp.NetCore_PostGRE.Data.Models.AppData
{
	public class RefreshToken : BaseModel
	{
		public string Token { get; set; }
		public string JwtId { get; set; }
		public bool isRevoked { get; set; } = false;
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
		public DateTime DateExpire { get; set; }
		public virtual User User {get; set; }
	}
}
