using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


[Route("/api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    // private readonly JwtConfig _jwtConfig;
    private readonly IConfiguration _configuration;

    public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
    {
        // Validate Incoming Request
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        // Check If The Email Already Exist
        var user_email = await _userManager.FindByEmailAsync(requestDto.Email);

        if (user_email != null)
        {
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                {
                    "Email already exists"
                }
            });
        }

        // create a user
        var new_user = new IdentityUser()
        {
            Email = requestDto.Email,
            UserName = requestDto.Email,
        };

        var is_created = await _userManager.CreateAsync(new_user, requestDto.Password);
        if (is_created.Errors.Count() != 0)
        {
            foreach (var Error in is_created.Errors)
            {
                Console.WriteLine(Error.Code);
            }
        }

        if (!is_created.Succeeded)
        {
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>() {
                    "Server Error"
                }
            });
        }

        // Generate Token
        var token = GenerateToken(new_user);

        return Ok(new AuthResult()
        {
            Result = true,
            Token = token
        });

    }

    [Route("login")]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserLoginRequestDto requestDto)
    {
        // Validate Request
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid Payload"
                },
                Result = false
            });
        }

        // Check if the user exist
        var existing_user = await _userManager.FindByEmailAsync(requestDto.Email);

        if (existing_user == null)
        {
            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid Email"
                },
                Result = false,
            });
        }

        var isCorrectPassword = await _userManager.CheckPasswordAsync(existing_user, requestDto.Password);

        if (!isCorrectPassword)
        {
            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
                {
                    "Invalid Password"
                }
            });
        }

        var jwtToken = GenerateToken(existing_user);

        return Ok(new AuthResult()
        {
            Result = true,
            Token = jwtToken
        });
    }


    private string GenerateToken(IdentityUser user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value!);

        // Token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
            }),
            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return jwtToken;
    }
}