namespace CollegeProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string AuthorName { get; set; }
        public required string Description { get; set; }
        public required string Contact { get; set; }
    }
}
