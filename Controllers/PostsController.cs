using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CollegeProject.Data;
using CollegeProject.Models;
using CollegeProject.DTOs;

using Microsoft.AspNetCore.Http;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
public async Task<IActionResult> GetPosts()
{
    var posts = await _context.Posts
        .Include(p => p.User) // Eager load the user to handle cycles
        .Select(p => new PostDto
        {
            Id = p.Id,
            PostType = p.PostType,
            Content = p.Content,
            PostTitle = p.PostTitle,
            Date = p.Date,
            User = new UserDto
            {
                Id = p.User.Id,
                username = p.User.Username,
                AuthorName = p.User.AuthorName,
                Description = p.User.Description,
                totalPosts = p.User.totalPosts,
                category = p.User.category,
                contacts = p.User.contacts
                // Map other necessary fields
            }
        })
        .ToListAsync();

    return Ok(posts);
}

[HttpGet("{id}")]
public async Task<IActionResult> GetPostById(int id)
{
    var post = await _context.Posts
        .Include(p => p.User) // Eager load the user to handle cycles
        .Where(p => p.Id == id)
        .Select(p => new PostDto
        {
            Id = p.Id,
            PostType = p.PostType,
            Content = p.Content,
            PostTitle = p.PostTitle,
            Date = p.Date,
            User = new UserDto
            {
                Id = p.User.Id,
                username = p.User.Username,
                AuthorName = p.User.AuthorName,
                Description = p.User.Description,
                totalPosts = p.User.totalPosts,
                category = p.User.category,
                contacts = p.User.contacts

                // Map other necessary fields
            }
        })
        .FirstOrDefaultAsync();

    if (post == null)
    {
        return NotFound();
    }
    return Ok(post);
}


[HttpPost]
public async Task<IActionResult> CreatePost([FromBody] Post post)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    var userNameClaim = User.FindFirst(ClaimTypes.Name); // Check other claims if needed

    if (userIdClaim == null)
    {
        // Log claims for debugging
        Console.WriteLine("UserIdClaim: Not found");
        return Unauthorized("User not authenticated.");
    }

    Console.WriteLine($"UserIdClaim: {userIdClaim.Value}");
    Console.WriteLine($"UserNameClaim: {userNameClaim?.Value}");

    if (!int.TryParse(userIdClaim.Value, out int userId))
    {
        return BadRequest("Invalid user ID.");
    }

    post.UserId = userId; // Set the UserId from the claim
    post.Date = DateTime.UtcNow;

    _context.Posts.Add(post);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
}



        [HttpPut("{id}")]
public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto updatePostDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var post = await _context.Posts.FindAsync(id);
    if (post == null)
    {
        return NotFound();
    }

    // Update only the fields that are not null
    if (updatePostDto.PostType != null)
        post.PostType = updatePostDto.PostType;
    if (updatePostDto.Content != null)
        post.Content = updatePostDto.Content;
    if (updatePostDto.PostTitle != null)
        post.PostTitle = updatePostDto.PostTitle;
    if (updatePostDto.Date.HasValue)
        post.Date = updatePostDto.Date.Value;

    _context.Posts.Update(post);
    await _context.SaveChangesAsync();

    return Ok(post);
}


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
