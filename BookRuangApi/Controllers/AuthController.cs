using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookRuangApi.Models;
using BookRuangApi.DTOs;
using BookRuangApi.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace BookRuangApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [EnableCors("AllowReact")]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower()))
            {
                return BadRequest(new { message = "Username sudah digunakan" });
            }

            // Check if email exists
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return BadRequest(new { message = "Email sudah terdaftar" });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create user
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                Role = UserRoles.User, // Default role adalah User
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate token
            var token = _tokenService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.Now.AddDays(7)
            };

            return Ok(response);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find user by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == dto.UsernameOrEmail.ToLower() ||
                    u.Email.ToLower() == dto.UsernameOrEmail.ToLower());

            if (user == null)
            {
                return Unauthorized(new { message = "Username/Email atau password salah" });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Username/Email atau password salah" });
            }

            // Check if active
            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Akun Anda tidak aktif" });
            }

            // Update last login
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            // Generate token
            var token = _tokenService.GenerateToken(user);

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.Now.AddDays(7)
            };

            return Ok(response);
        }

        // GET: api/Auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User tidak ditemukan" });

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };

            return Ok(profile);
        }

        // PUT: api/Auth/profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User tidak ditemukan" });

            // Update email if provided and not duplicate
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.Id != userId))
                {
                    return BadRequest(new { message = "Email sudah digunakan" });
                }
                user.Email = dto.Email;
            }

            // Update full name if provided
            if (!string.IsNullOrEmpty(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            await _context.SaveChangesAsync();

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };

            return Ok(profile);
        }

        // PUT: api/Auth/change-password
        [Authorize]
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User tidak ditemukan" });

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Password lama salah" });
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password berhasil diubah" });
        }

        // POST: api/Auth/create-admin (Only for initial setup)
        [HttpPost("create-admin")]
        public async Task<ActionResult> CreateAdmin([FromBody] RegisterDto dto)
        {
            // Check if any admin exists
            if (await _context.Users.AnyAsync(u => u.Role == UserRoles.Admin))
            {
                return BadRequest(new { message = "Admin sudah ada. Endpoint ini hanya untuk setup awal." });
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var admin = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                Role = UserRoles.Admin,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin berhasil dibuat", username = admin.Username });
        }
    }
}