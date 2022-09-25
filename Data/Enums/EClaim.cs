using System.ComponentModel;

namespace StartupProject_Asp.NetCore_PostGRE.Data.Enums
{
    public enum EClaim
    {
        //[Display(Name = "Role-Claim Policy")]
        [Description("SuperAdmin.All")]
        SuperAdmin_All = 0,
        [Description("Role.Create")]
        Role_Create,
        [Description("Role.Read")]
        Role_Read,
        [Description("Role.Update")]
        Role_Update,
        [Description("Role.Delete")]
        Role_Delete,
        [Description("Claim.Create")]
        Claim_Create,
        [Description("UsersApi.Get")]
        UsersApi_Get,
        [Description("UsersApi.GetAll")]
        UsersApi_GetAll,
        [Description("UsersApi.Update")]
        UsersApi_Update,
        [Description("UsersApi.Create")]
        UsersApi_Create,
        [Description("UsersApi.Delete")]
        UsersApi_Delete
    }
}
