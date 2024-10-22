using UberSystem.Domain.Enums;

namespace UberSystem.Domain.Entities;

public partial class User
{
    public long Id { get; set; }

    public UserRole Role { get; set; }

    public string? UserName { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }
    public bool? IsEmailConfirmed { get; set; }

    public string? VerificationCode { get; set; }
    
    public DateTime ExpiryDate { get; set; }

    public virtual ICollection<Customer> Customers { get; } = new List<Customer>();

    public virtual ICollection<Driver> Drivers { get; } = new List<Driver>();
}
