using System.ComponentModel.DataAnnotations;

namespace KiemTraGiuaKy.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}