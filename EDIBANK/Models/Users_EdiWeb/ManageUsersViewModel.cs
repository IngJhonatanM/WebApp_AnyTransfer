namespace EDIBANK.Models.Users_EdiWeb;

public class ManageUsersViewModel
{
    public required string? Success { get; init; }

    public required string? Failure { get; init; }

    public required AppUser AppUser { get; init; }

    public required IList<AppUser> AdminRole { get; init; }

    public required IList<AppUser> UserRole { get; init; }
}
