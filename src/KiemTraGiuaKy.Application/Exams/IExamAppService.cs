using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KiemTraGiuaKy.Exams.Dto;
using Microsoft.AspNetCore.Http;

namespace KiemTraGiuaKy.Exams;

public interface IExamAppService : IApplicationService
{
    Task<PagedResultDto<ExamListDto>> GetAll(PagedAndSortedResultRequestDto input);
    Task<ExamDetailDto> GetForEdit(long id);
    Task<ExamDetailDto> GetForPreview(long id);
    Task Update(UpdateExamDto input);
    Task UpdateQuestion(UpdateQuestionDto input);
    Task<bool> TogglePublish(long id);
    Task Delete(long id);
    Task<ImportExamResultDto> ImportFromDocx(IFormFile file);
    Task<ImportExamResultDto> ImportFromBytes(byte[] fileBytes, string fileName);
}
