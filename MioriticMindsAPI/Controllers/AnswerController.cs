using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MioriticMindsAPI.Models;
using MioriticMindsAPI.Repository;

namespace MioriticMindsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerController : ControllerBase
    {
        private readonly MioriticMindsDbContext _dbContext;

        public AnswerController(MioriticMindsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnswers()
        {
            if (_dbContext.Answers == null)
                return NotFound();

            return await _dbContext.Answers.ToListAsync();
        }

        [HttpPost("addAnswer")]
        public async Task<IActionResult> AddAnswer([FromBody] Answer answer)
        {
            if (answer == null)
            {
                return BadRequest("Answer is null.");
            }
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            int userId = UserController.GetUserFromToken(token);

            answer.UserId = userId;
            await _dbContext.Answers.AddAsync(answer);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("getCorrectAnswersInARow")]
        public async Task<IActionResult> GetCorrectAnswersInARow()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            int userId = UserController.GetUserFromToken(token);
            var answers = await _dbContext.Answers.Where(a => a.UserId == userId).ToListAsync();
            int correctAnswersInARow = 0;
            foreach (Answer answer in answers.Reverse<Answer>())
            {
                if (answer.IsCorrect)
                {
                    correctAnswersInARow++;
                    continue;
                }
                break;
            }
            return Ok(correctAnswersInARow);
        }
    }
}