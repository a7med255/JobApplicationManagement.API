namespace JobApplicationManagement.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Careerly";
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

    public string FromEmail
    {
        get => FromAddress;
        set => FromAddress = value;
    }
}
