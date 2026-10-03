using Microsoft.AspNetCore.Identity;

namespace TrustRent.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public bool IsVerified { get; set; }
}