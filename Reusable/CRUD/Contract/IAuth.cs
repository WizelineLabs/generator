namespace Reusable.Contract;

public interface IAuthSession
{
    string UserAuthId { get; set; }
    string AuthProvider { get; set; }
    bool IsAuthenticated { get; set; }
    List<string> Permissions { get; set; }
    List<string> Roles { get; set; }
    DateTime CreatedAt { get; set; }
    string Email { get; set; }
    string LastName { get; set; }
    string FirstName { get; set; }
    string DisplayName { get; set; }
    string UserName { get; set; }
    string UserAuthName { get; set; }
    string Sequence { get; set; }
    string Id { get; set; }
    string ReferrerUrl { get; set; }
    string ProfileUrl { get; set; }

    bool IsAuthorized(string provider);
}