namespace IdentityManager.Models.ViewModels
{
    public class ClaimsViewModel
    {
        public ClaimsViewModel()
        {
            Claims = new List<ClaimsSelection>();
        }

        public ApplicationUser User { get; set; }
        public IList<ClaimsSelection> Claims { get; set; }
    }

    public class ClaimsSelection
    {
        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }
}
