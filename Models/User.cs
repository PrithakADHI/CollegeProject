namespace CollegeProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string AuthorName { get; set; }
        public required string Description { get; set; }
        public required int totalPosts { get; set; }
        public string category { get; set; }

        public string contacts { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
