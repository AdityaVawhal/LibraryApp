namespace LibraryApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User"; // or Admin
        public string LibraryId => $"LIB-{Id:0000}";
    }
}
