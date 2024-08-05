

namespace CollegeProject.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string PostType { get; set; } // "Poem", "Article", "Story"
        public string Content { get; set; }
        public string PostTitle { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; } // Foreign key to User

        public User? User { get; set; } // Navigation property to User
    }
}
