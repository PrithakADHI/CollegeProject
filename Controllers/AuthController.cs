using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CollegeProject.Data;
using CollegeProject.Models;
using CollegeProject.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] User user)
{
    if (await _context.Users.AnyAsync(u => u.Username == user.Username))
    {
        return BadRequest("Username already exists");
    }

    // Handle image assignment
    var imageUrls = new List<string>
    {
        "random-link-not-to-be-used.com",
        "https://images.pexels.com/photos/12889460/pexels-photo-12889460.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/7811599/pexels-photo-7811599.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/11640148/pexels-photo-11640148.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/774909/pexels-photo-774909.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/1239291/pexels-photo-1239291.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/1222271/pexels-photo-1222271.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/733872/pexels-photo-733872.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/91227/pexels-photo-91227.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1",
        "https://images.pexels.com/photos/1181519/pexels-photo-1181519.jpeg?auto=compress&cs=tinysrgb&w=600",
        "https://images.pexels.com/photos/1933873/pexels-photo-1933873.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
        // Add more image URLs here
    };

    if (user.ImageNo != 0)
    {
        int imageNo = 1;
        if (user.ImageNo != null) {
            imageNo = (int) user.ImageNo;
        }
        user.ImageUrl = imageUrls[imageNo % imageUrls.Count]; // Get the corresponding URL
    }
    else
    {
        // No image chosen
        var random = new Random();
        var randomImageNo = random.Next(imageUrls.Count);
        user.ImageNo = randomImageNo;
        user.ImageUrl = imageUrls[randomImageNo];
    }

    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return Ok(new { message = "Successfully registered" });
}


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
{
    var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == login.Username && u.Password == login.Password);

    if (user == null)
    {
        return Unauthorized("Invalid credentials");
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Use the same claim type here
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

    return Ok(new { message = "Successfully logged in" });
}


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Successfully logged out" });
        }

        [HttpGet("user")]
public IActionResult GetUser()
{
    if (User.Identity.IsAuthenticated)
    {
        var Id = User.FindFirstValue(ClaimTypes.NameIdentifier); // Extract the user ID from claims
        var username = User.Identity.Name;
        
        return Ok(new { Id, username });
    }
    return Unauthorized();
}


    }

    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase {

        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        [HttpGet("authors")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new 
                { 
                    u.Id, 
                    u.Username, 
                    u.AuthorName, 
                    u.Description, 
                    u.totalPosts,
                    u.category,
                    u.contacts,
                    u.ImageNo,
                    u.ImageUrl
                })
                .ToListAsync();
  
            return Ok(users);
        }

        [HttpGet("authors/{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var user = await _context.Users
        .Include(u => u.Posts)
        .Where(u => u.Id == id)
        .Select(u => new
        {
            u.Id,
            u.Username,
            u.AuthorName,
            u.Description,
            u.totalPosts,
            u.category,
            u.contacts,
            u.ImageNo,
            u.ImageUrl,
            Posts = u.Posts.Select(p => new
            {
                p.Id,
                p.PostType,
                p.Content,
                p.PostTitle,
                p.Date
            })
        })
        .FirstOrDefaultAsync();

    if (user == null)
    {
        return NotFound();
    }

    return Ok(user);
}


[HttpPut("authors/{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
{
    if (updateDto == null)
    {
        return BadRequest("Invalid update data.");
    }

    var user = await _context.Users.FindAsync(id);
    if (user == null)
    {
        return NotFound();
    }

    // Update the user entity with the values from the DTO
    if (updateDto.totalPosts.HasValue)
    {
        user.totalPosts = updateDto.totalPosts.Value;
    }
    if (!string.IsNullOrEmpty(updateDto.Username))
    {
        user.Username = updateDto.Username;
    }
    if (!string.IsNullOrEmpty(updateDto.AuthorName))
    {
        user.AuthorName = updateDto.AuthorName;
    }
    if (!string.IsNullOrEmpty(updateDto.Description))
    {
        user.Description = updateDto.Description;
    }
    if (!string.IsNullOrEmpty(updateDto.category))
    {
        user.category = updateDto.category;
    }
    if (!string.IsNullOrEmpty(updateDto.contacts))
    {
        user.contacts = updateDto.contacts;
    }

    // Validate the updated user entity
    if (!TryValidateModel(user))
    {
        return ValidationProblem(ModelState);
    }

    await _context.SaveChangesAsync();

    return Ok(new { message = "Author updated successfully." });
}


    }

    public class LoginModel
    {
        
        public required string Username { get; set; }

        public required string Password { get; set; }
    }
}