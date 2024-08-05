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
        public async Task<IActionResult> Register([FromBody] User user, [FromForm] IFormFile? image)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists");
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
                return Ok(new { username = User.Identity.Name });
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
                    u.contacts
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



    }

    public class LoginModel
    {
        
        public required string Username { get; set; }

        public required string Password { get; set; }
    }
}