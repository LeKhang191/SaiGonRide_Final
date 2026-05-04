namespace SaigonRide.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = "Local";
        public string IdNumber { get; set; } = string.Empty;
    }
}