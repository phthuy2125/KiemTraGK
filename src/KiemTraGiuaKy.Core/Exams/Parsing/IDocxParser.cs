using System.IO;

namespace KiemTraGiuaKy.Exams.Parsing;

/// <summary>
/// Domain service interface for parsing a TOEIC Reading exam from a .docx stream.
/// Implementation lives in the Application layer (or Infrastructure) to avoid
/// pulling OpenXML dependencies into Core.
/// Returns a <see cref="DocxParseResult"/> which is always fully populated —
/// all errors collected before returning (all-or-nothing validation).
/// </summary>
public interface IDocxParser
{
    /// <summary>
    /// Parse the given .docx stream and validate its structure.
    /// </summary>
    /// <param name="docxStream">Stream of the uploaded .docx file.</param>
    /// <returns>
    /// A <see cref="DocxParseResult"/> with IsValid=true and a populated ParsedExam,
    /// or IsValid=false with a list of all collected errors.
    /// </returns>
    DocxParseResult Parse(Stream docxStream);
}
