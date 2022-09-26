using System;

namespace StartupProject_Asp.NetCore_PostGRE.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
