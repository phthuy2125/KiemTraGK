using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KiemTraGiuaKy.Exams.Services;
using Shouldly;
using Xunit;

namespace KiemTraGiuaKy.Tests.Exams;

public class DocxParser_Tests
{
    private readonly DocxParser _parser;

    public DocxParser_Tests()
    {
        _parser = new DocxParser();
    }

    [Fact]
    public void Parse_ShouldReturnFailure_WhenStreamIsEmpty()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var result = _parser.Parse(emptyStream);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void Parse_ShouldReturnFailure_WhenMissingRequiredPartTags()
    {
        // Arrange: Document with title only, no [PART 5], [PART 6], [PART 7]
        using var stream = CreateTestDocx(new[]
        {
            "[EXAM_TITLE]: Test Exam 2026",
            "Random text paragraph"
        });

        // Act
        var result = _parser.Parse(stream);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.Message.Contains("PART")).ShouldBeTrue();
    }

    [Fact]
    public void Parse_ShouldDetect_MissingOptions()
    {
        // Arrange: Question has only options A and B (missing C and D)
        using var stream = CreateTestDocx(new[]
        {
            "[EXAM_TITLE]: Test Exam",
            "[PART 5]",
            "[QUESTION: 101] The company announced that sales increased.",
            "[A] significantly",
            "[B] significant",
            "[ANSWER: A]",
            "[EXPLANATION] Adverb modifies verb."
        });

        // Act
        var result = _parser.Parse(stream);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.Message.Contains("option") || e.Message.Contains("4") || e.Message.Contains("lựa chọn")).ShouldBeTrue();
    }

    [Fact]
    public void Parse_ShouldDetect_MissingAnswerTag()
    {
        // Arrange: Question has 4 options but no [ANSWER: ...] tag
        using var stream = CreateTestDocx(new[]
        {
            "[EXAM_TITLE]: Test Exam",
            "[PART 5]",
            "[QUESTION: 101] The company announced that sales increased.",
            "[A] significantly",
            "[B] significant",
            "[C] significance",
            "[D] signify"
            // Missing [ANSWER: A]
        });

        // Act
        var result = _parser.Parse(stream);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.Message.Contains("ANSWER") || e.Message.Contains("đáp án")).ShouldBeTrue();
    }

    [Fact]
    public void Parse_ShouldDetect_WrongNumberingSequence()
    {
        // Arrange: Question skips from 101 to 103 (missing 102)
        using var stream = CreateTestDocx(new[]
        {
            "[EXAM_TITLE]: Test Exam",
            "[PART 5]",
            "[QUESTION: 101] First question.",
            "[A] a", "[B] b", "[C] c", "[D] d",
            "[ANSWER: A]",
            "[QUESTION: 103] Skipped question 102.",
            "[A] a", "[B] b", "[C] c", "[D] d",
            "[ANSWER: B]"
        });

        // Act
        var result = _parser.Parse(stream);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.Message.Contains("102") || e.Message.Contains("thứ tự") || e.Message.Contains("number")).ShouldBeTrue();
    }

    /// <summary>
    /// Helper to generate in-memory Word .docx file with given paragraphs.
    /// </summary>
    private MemoryStream CreateTestDocx(string[] paragraphs)
    {
        var stream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());

            foreach (var text in paragraphs)
            {
                var p = new Paragraph(new Run(new Text(text)));
                mainPart.Document.Body.AppendChild(p);
            }

            mainPart.Document.Save();
        }

        stream.Position = 0;
        return stream;
    }
}
