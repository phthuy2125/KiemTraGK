using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using KiemTraGiuaKy.Exams;
using KiemTraGiuaKy.Exams.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KiemTraGiuaKy.Web.Host.Controllers
{
    /// <summary>
    /// RESTful API Controller for Mobile Learners.
    /// Exposed at /api/mobile/...
    /// </summary>
    [Route("api/mobile")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [AbpAllowAnonymous]
    public class MobileExamController : AbpController
    {
        private readonly IExamAttemptAppService _attemptAppService;

        public MobileExamController(IExamAttemptAppService attemptAppService)
        {
            _attemptAppService = attemptAppService;
        }

        /// <summary>
        /// GET /api/mobile/exams - Get list of published exams available to take.
        /// </summary>
        [HttpGet("exams")]
        public async Task<ActionResult<List<ExamListDto>>> GetExams()
        {
            var result = await _attemptAppService.GetAvailableExams();
            return Ok(result);
        }

        /// <summary>
        /// GET /api/mobile/exams/{id} - Get exam content for taking the test.
        /// Strict: Correct answers and explanations are omitted.
        /// </summary>
        [HttpGet("exams/{id:long}")]
        public async Task<ActionResult<ExamForTakingDto>> GetExamForTaking(long id)
        {
            var result = await _attemptAppService.GetExamForTaking(id);
            return Ok(result);
        }

        /// <summary>
        /// POST /api/mobile/attempts/start - Start a new exam attempt.
        /// Server computes start and deadline times.
        /// </summary>
        [HttpPost("attempts/start")]
        public async Task<ActionResult<StartAttemptResultDto>> StartAttempt([FromBody] StartAttemptInput input)
        {
            var result = await _attemptAppService.StartAttempt(input);
            return Ok(result);
        }

        /// <summary>
        /// POST /api/mobile/attempts/answer - Save an answer in real time.
        /// </summary>
        [HttpPost("attempts/answer")]
        public async Task<ActionResult<SubmitAnswerResultDto>> SubmitAnswer([FromBody] SubmitAnswerInput input)
        {
            var result = await _attemptAppService.SubmitAnswer(input);
            return Ok(result);
        }

        /// <summary>
        /// POST /api/mobile/attempts/finish - Complete and submit attempt for grading.
        /// </summary>
        [HttpPost("attempts/finish")]
        public async Task<ActionResult<AttemptResultDetailDto>> FinishAttempt([FromBody] FinishAttemptInput input)
        {
            var result = await _attemptAppService.FinishAttempt(input);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/mobile/attempts/{id}/result - Get detailed result and question review after submission.
        /// </summary>
        [HttpGet("attempts/{id:long}/result")]
        public async Task<ActionResult<AttemptResultDetailDto>> GetResult(long id)
        {
            var result = await _attemptAppService.GetResult(id);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/mobile/attempts/my - Get history of past attempts for the current user.
        /// </summary>
        [HttpGet("attempts/my")]
        public async Task<ActionResult<List<AttemptSummaryDto>>> GetMyAttempts()
        {
            var result = await _attemptAppService.GetMyAttempts();
            return Ok(result);
        }
    }
}
