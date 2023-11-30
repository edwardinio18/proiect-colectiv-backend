using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("checkAnswer")]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromBody] Answer answer)
        {
            if (answer == null)
            {
                return BadRequest("Answer is null.");
            }

            int userId = answer.UserId;
            
            return Ok();
        }
    }
}