using StartupProject_Asp.NetCore_PostGRE.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StartupProject_Asp.NetCore_PostGRE.Data.Models.AppData
{
	public class DoctorAssignment : BaseModel
	{
		public Guid? DoctorId { get; set; }
		[ForeignKey("DoctorId")]
		public virtual User Doctor { get; set; }

		public Guid? PatientId { get; set; }
		[ForeignKey("PatientId")]
		public virtual User Patient { get; set; }
	}
}
