namespace IdentityManager.Models.ViewModels
{
    public class RolesViewModel
    {
        public RolesViewModel()
        {
            Roles = new List<RoleSelection>();
        }

        public ApplicationUser User { get; set; }
        public IList<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
