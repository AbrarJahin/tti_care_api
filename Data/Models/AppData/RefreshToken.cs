using StartupProject_Asp.NetCore_PostGRE.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupProject_Asp.NetCore_PostGRE.Data.Models.AppData
{
	public class RefreshToken : BaseModel
	{
		public string Token { get; set; }
		public string JwtId { get; set; }
		public bool isRevoked { get; set; } = false;
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
		public DateTime DateExpire { get; set; }
		public Guid? UserId { get; set; }
		[ForeignKey("UserId")]
		public virtual User User { get; set; }
	}
}
