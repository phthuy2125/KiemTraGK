using System.Collections.Generic;
using KiemTraGiuaKy.Exams.Enums;

namespace KiemTraGiuaKy.Exams.Parsing;

// ─── Parsed model DTOs (intermediate, not DB entities) ───────────────────────

/// <summary>Parsed representation of a complete exam from the .docx file.</summary>
public class ParsedExam
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int DurationMinutes { get; set; } = 75;
    public List<ParsedGroup> Groups { get; set; } = new();
    public List<ParsedQuestion> StandaloneQuestions { get; set; } = new(); // Part 5
}

/// <summary>Parsed representation of a question group (Part 6 or Part 7).</summary>
public class ParsedGroup
{
    public int PartNumber { get; set; }
    public int FromQuestionNumber { get; set; }
    public int ToQuestionNumber { get; set; }
    public string Instruction { get; set; }
    public int OrderIndex { get; set; }
    public List<ParsedPassageItem> PassageItems { get; set; } = new();
    public List<ParsedQuestion> Questions { get; set; } = new();
}

/// <summary>Parsed passage component (text block or image).</summary>
public class ParsedPassageItem
{
    public int OrderIndex { get; set; }
    public PassageItemType Type { get; set; }

    /// <summary>Original text for text items; the image relationship guid for images.</summary>
    public string TextContent { get; set; }

    /// <summary>
    /// Raw image bytes extracted from the .docx package.
    /// Persisted as file; URL assigned after saving.
    /// </summary>
    public byte[] ImageBytes { get; set; }

    /// <summary>File extension (e.g., "png", "jpeg").</summary>
    public string ImageExtension { get; set; }

    /// <summary>Final URL assigned after saving to wwwroot.</summary>
    public string SavedImageUrl { get; set; }
}

/// <summary>Parsed representation of a single question.</summary>
public class ParsedQuestion
{
    public int PartNumber { get; set; }
    public int QuestionNumber { get; set; }
    public string Content { get; set; }
    public List<ParsedOption> Options { get; set; } = new();
    public AnswerOption CorrectOption { get; set; }
    public string Explanation { get; set; }

    /// <summary>1-based paragraph index in the source document (for error reporting).</summary>
    public int SourceParagraphIndex { get; set; }
}

/// <summary>Parsed option A/B/C/D.</summary>
public class ParsedOption
{
    public AnswerOption Label { get; set; }
    public string Content { get; set; }
}

// ─── Parser result ───────────────────────────────────────────────────────────

/// <summary>
/// Result returned by IDocxParser.Parse().
/// If IsValid = false, ParsedExam is null and Errors contains all collected issues.
/// The parser always collects ALL errors before returning (all-or-nothing).
/// </summary>
public class DocxParseResult
{
    public bool IsValid { get; set; }
    public List<ParseError> Errors { get; set; } = new();
    public ParsedExam ParsedExam { get; set; }

    public static DocxParseResult Failure(List<ParseError> errors) =>
        new() { IsValid = false, Errors = errors };

    public static DocxParseResult Success(ParsedExam exam) =>
        new() { IsValid = true, ParsedExam = exam };
}

/// <summary>Describes a single parsing/validation error.</summary>
public class ParseError
{
    /// <summary>1-based paragraph number in the Word document.</summary>
    public int ParagraphIndex { get; set; }

    /// <summary>Human-readable error description (Vietnamese or English).</summary>
    public string Message { get; set; }

    public ParseError(int paragraphIndex, string message)
    {
        ParagraphIndex = paragraphIndex;
        Message = message;
    }

    public override string ToString() =>
        $"Đoạn {ParagraphIndex}: {Message}";
}
