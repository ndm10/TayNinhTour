namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string? TOtpSecret { get; set; }
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
    }
}
