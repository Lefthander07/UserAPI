using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.Core;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Login must contain only Latin letters and digits.")]
    public string Login { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Password must contain only Latin letters and digits.")]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [RegularExpression("^[A-Za-z\u0400-\u04FF ]+$", ErrorMessage = "Name must contain only letters (Latin or Cyrillic).")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; } = Gender.Unknown;

    public DateTime? Birthday { get; set; }

    public bool Admin { get; set; } = false;

    [Required]
    public DateTime CreatedOn { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
    public Guid? ModifiedBy { get; set; }

    public DateTime? RevokedOn { get; set; }
    public Guid? RevokedBy { get; set; }
}
