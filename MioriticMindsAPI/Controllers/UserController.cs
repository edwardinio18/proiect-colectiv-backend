using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MioriticMindsAPI.Models;
using MioriticMindsAPI.Repository;
using MioriticMindsAPI.Validation;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MioriticMindsAPI.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MioriticMindsDbContext _dbContext;
        private readonly JwtSetter _jwtSetter;
        private readonly UserValidator _validator;

        public UserController(MioriticMindsDbContext dbContext, IOptions<JwtSetter> JwtSetter)
        {
            _dbContext = dbContext;
            _jwtSetter = JwtSetter.Value;
            _validator = new UserValidator();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSetter.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static int GetUserFromToken(string token)
        {
            var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
            var val = claims.ElementAt(0).Value;
            return int.Parse(val);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_dbContext.Users == null)
                return NotFound();

            return await _dbContext.Users.ToListAsync();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Login(UserDTO User)
        {
            if (User.Username == null || User.Password == null)
                return BadRequest();

            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.UserName == User.Username && u.Password != null && u.Password == User.Password);

            if (user == null)
                return Unauthorized("!ERROR! Invalid username or password!");

            var token = GenerateJwtToken(user);
            user.Password = null;

            return new
            {
                user,
                token
            };
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Register(UserDTO userDTO)
        {
            var validationResult = _validator.ValidateRegister(userDTO);
            if (validationResult != string.Empty)
                return BadRequest(validationResult);

            if (await _dbContext.Users.AnyAsync(u => u.UserName == userDTO.Username))
                return BadRequest("Username is already taken.");

            var newUser = new User
            {
                UserName = userDTO.Username,
                Password = userDTO.Password,
                HighScoreMixed = 0,
                HighScorePhotos = 0,
                HighScoreText = 0
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            var token = GenerateJwtToken(newUser);
            newUser.Password = null;

            return new
            {
                newUser,
                token
            };
        }
    }
}
