namespace Petly.Business.Services;

public class PasswordResetOptions
{
    public int CodeLifetimeMinutes { get; set; } = 10;
}
