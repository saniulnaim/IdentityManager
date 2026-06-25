using Microsoft.AspNetCore.Authorization;

namespace IdentityManager.Helpers
{
    public class PolicyHelver
    {
        public bool AdminRole_CreateEditDeleteClaim_ORSuperAdminRole(AuthorizationHandlerContext context)
        {
            return (context.User.IsInRole(SD.Admin) && context.User.HasClaim(c => c.Type == "Create" && c.Value == "True"
    && context.User.HasClaim(c => c.Type == "Edit" && c.Value == "True"
    && context.User.HasClaim(c => c.Type == "Delete" && c.Value == "True")
    ))
    || context.User.IsInRole(SD.SuperAdmin));
        }
    }
}
