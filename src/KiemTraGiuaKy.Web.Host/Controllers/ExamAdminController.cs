using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Authorization;
using KiemTraGiuaKy.Authorization;
using KiemTraGiuaKy.Exams;
using KiemTraGiuaKy.Exams.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KiemTraGiuaKy.Web.Host.Controllers
{
    /// <summary>
    /// Controller for Admin / Teacher Exam Management.
    /// </summary>
    [Route("api/admin/exams")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [AbpAllowAnonymous]
    public class ExamAdminController : AbpController
    {
    private readonly IExamAppService _examAppService;

    public ExamAdminController(IExamAppService examAppService)
    {
        _examAppService = examAppService;
    }

    /// <summary>
    /// GET /api/admin/exams - Get paginated exams list.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ExamListDto>>> GetAll([FromQuery] PagedAndSortedResultRequestDto input)
    {
        var result = await _examAppService.GetAll(input);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/admin/exams/{id} - Get complete exam detail for preview and editing.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ExamDetailDto>> GetDetail(long id)
    {
        var result = await _examAppService.GetForPreview(id);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/admin/exams/upload - Upload and import .docx file.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportExamResultDto>> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file .docx" });
        }

        var result = await _examAppService.ImportFromDocx(file);
        return Ok(result);
    }

    /// <summary>
    /// PUT /api/admin/exams/{id} - Update exam general metadata.
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateExamDto input)
    {
        input.Id = id;
        await _examAppService.Update(input);
        return Ok(new { message = "Cập nhật đề thi thành công" });
    }

    /// <summary>
    /// PUT /api/admin/exams/question - Update question content, correct answer, and explanation.
    /// </summary>
    [HttpPut("question")]
    public async Task<IActionResult> UpdateQuestion([FromBody] UpdateQuestionDto input)
    {
        await _examAppService.UpdateQuestion(input);
        return Ok(new { message = "Cập nhật câu hỏi thành công" });
    }

    /// <summary>
    /// POST /api/admin/exams/{id}/publish - Toggle publish status.
    /// </summary>
    [HttpPost("{id:long}/publish")]
    public async Task<ActionResult<bool>> TogglePublish(long id)
    {
        var isPublished = await _examAppService.TogglePublish(id);
        return Ok(new { isPublished });
    }

    /// <summary>
    /// DELETE /api/admin/exams/{id} - Delete an exam and all related items.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _examAppService.Delete(id);
        return Ok(new { message = "Xóa đề thi thành công" });
    }
}
}
