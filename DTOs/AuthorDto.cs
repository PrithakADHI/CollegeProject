namespace CollegeProject.DTOs
{
    public class AuthorDto
    {
        public required string AuthorName { get; set; }
        public required string Description { get; set; }
        public required int totalPosts { get; set; }
    }

    public class PostDto
{
    public int Id { get; set; }
    public string PostType { get; set; }
    public string Content { get; set; }
    public string PostTitle { get; set; }
    public DateTime Date { get; set; }
    public UserDto User { get; set; }
}

public class UserDto
{
    public int Id { get; set; }

    public string username {get; set;}
    public string AuthorName { get; set; }

    public string Description { get; set; }

    public int totalPosts { get; set; }

    public string category { get; set; }

    public string contacts {get; set;}
    // Include other fields as needed
}

public class UpdatePostDto
{
    public string? PostType { get; set; }
    public string? Content { get; set; }
    public string? PostTitle { get; set; }
    public DateTime? Date { get; set; }
}




}
