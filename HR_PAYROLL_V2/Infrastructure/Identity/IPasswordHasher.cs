namespace HR_PAYROLL_V2.Infrastructure.Identity;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
