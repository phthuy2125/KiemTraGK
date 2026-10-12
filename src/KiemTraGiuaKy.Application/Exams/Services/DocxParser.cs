using Abp.Dependency;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KiemTraGiuaKy.Exams.Enums;
using KiemTraGiuaKy.Exams.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KiemTraGiuaKy.Exams.Services;

/// <summary>
/// Parser linh hoạt hỗ trợ 2 định dạng file Word (.docx):
/// 1. Định dạng tự nhiên/hiện đại:
///    [EXAM_TITLE] ...
///    [PART 5], [PART 6], [PART 7]
///    [QUESTION: 101] hoặc [Q 101]
///    [ANSWER: B] hoặc [ANS] B
///    [EXPLANATION] ...
///    [PASSAGE: 131-134]
///    A. Option / [A] Option
/// 2. Định dạng thẻ đóng mở gốc:
///    [EXAM] ... [/EXAM]
///    [GROUP 131-134] ... [/GROUP]
/// </summary>
public class DocxParser : IDocxParser, ITransientDependency
{
    private const int PART5_FROM = 101;
    private const int PART5_TO = 130;
    private const int PART6_FROM = 131;
    private const int PART6_TO = 146;
    private const int PART7_FROM = 147;
    private const int PART7_TO = 200;
    private const int TOTAL_QUESTIONS = 100;

    public DocxParseResult Parse(Stream docxStream)
    {
        return ParseInternal(docxStream, extractImages: false);
    }

    public DocxParseResult ParseWithImages(Stream docxStream)
    {
        return ParseInternal(docxStream, extractImages: true);
    }

    private DocxParseResult ParseInternal(Stream docxStream, bool extractImages)
    {
        var errors = new List<ParseError>();
        var paragraphs = new List<(int Index, string Text, bool HasImage, string ImageRelId)>();
        MainDocumentPart mainPart = null;

        try
        {
            using var wordDoc = WordprocessingDocument.Open(docxStream, false);
            mainPart = wordDoc.MainDocumentPart;
            var body = mainPart.Document.Body;
            int paraIdx = 0;

            foreach (var para in body.Elements<Paragraph>())
            {
                paraIdx++;
                var text = GetParagraphText(para).Trim();
                var (hasImage, relId) = HasInlineImage(para, mainPart);
                paragraphs.Add((paraIdx, text, hasImage, relId));
            }

            var result = ExecuteParse(paragraphs, mainPart, extractImages, errors);

            if (errors.Any())
                return DocxParseResult.Failure(errors);

            return DocxParseResult.Success(result);
        }
        catch (Exception ex)
        {
            errors.Add(new ParseError(0, $"Lỗi xử lý file Word: {ex.Message}"));
            return DocxParseResult.Failure(errors);
        }
    }

    private ParsedExam ExecuteParse(
        List<(int Index, string Text, bool HasImage, string ImageRelId)> paragraphs,
        MainDocumentPart mainPart,
        bool extractImages,
        List<ParseError> errors)
    {
        var result = new ParsedExam
        {
            DurationMinutes = 75,
            Title = "TOEIC Reading Exam"
        };

        var allQuestionNumbers = new List<int>();
        int idx = 0;
        int currentPart = 5;
        int groupOrderIndex = 0;

        // 1. Tìm tiêu đề đề thi
        while (idx < paragraphs.Count)
        {
            idx = SkipEmpty(paragraphs, idx);
            if (idx >= paragraphs.Count) break;

            var line = paragraphs[idx].Text;
            if (line.Equals("[EXAM_TITLE]", StringComparison.OrdinalIgnoreCase))
            {
                idx++;
                idx = SkipEmpty(paragraphs, idx);
                if (idx < paragraphs.Count && !paragraphs[idx].Text.StartsWith("["))
                {
                    result.Title = paragraphs[idx].Text.Trim();
                    idx++;
                }
                break;
            }
            else if (line.Equals("[EXAM]", StringComparison.OrdinalIgnoreCase))
            {
                idx++;
                while (idx < paragraphs.Count && !paragraphs[idx].Text.Equals("[/EXAM]", StringComparison.OrdinalIgnoreCase))
                {
                    var subLine = paragraphs[idx].Text;
                    if (subLine.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
                        result.Title = subLine.Substring(6).Trim();
                    else if (subLine.StartsWith("Duration:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(subLine.Substring(9).Trim(), out int dur))
                            result.DurationMinutes = dur;
                    }
                    idx++;
                }
                if (idx < paragraphs.Count) idx++; // consume [/EXAM]
                break;
            }
            else if (line.StartsWith("[PART", StringComparison.OrdinalIgnoreCase))
            {
                // Bắt đầu Part luôn
                break;
            }
            idx++;
        }

        // 2. Duyệt qua toàn bộ văn bản để phân tích Parts, Passages và Questions
        ParsedGroup currentGroup = null;

        while (idx < paragraphs.Count)
        {
            idx = SkipEmpty(paragraphs, idx);
            if (idx >= paragraphs.Count) break;

            var line = paragraphs[idx].Text;

            // Kiểm tra thẻ PART
            if (line.Equals("[PART 5]", StringComparison.OrdinalIgnoreCase))
            {
                currentPart = 5;
                currentGroup = null;
                idx++;
                continue;
            }
            if (line.Equals("[PART 6]", StringComparison.OrdinalIgnoreCase))
            {
                currentPart = 6;
                currentGroup = null;
                idx++;
                continue;
            }
            if (line.Equals("[PART 7]", StringComparison.OrdinalIgnoreCase))
            {
                currentPart = 7;
                currentGroup = null;
                idx++;
                continue;
            }

            // Kiểm tra thẻ PASSAGE hoặc GROUP
            var passageMatch = Regex.Match(line, @"^\[(?:PASSAGE|GROUP)[\s:]*(\d+)\s*[-–]\s*(\d+)\]$", RegexOptions.IgnoreCase);
            if (passageMatch.Success)
            {
                int fromQ = int.Parse(passageMatch.Groups[1].Value);
                int toQ = int.Parse(passageMatch.Groups[2].Value);
                idx++;

                currentGroup = new ParsedGroup
                {
                    PartNumber = currentPart,
                    FromQuestionNumber = fromQ,
                    ToQuestionNumber = toQ,
                    OrderIndex = ++groupOrderIndex
                };
                result.Groups.Add(currentGroup);

                // Đọc đoạn văn bản passage
                var passageLines = new List<string>();
                while (idx < paragraphs.Count)
                {
                    idx = SkipEmpty(paragraphs, idx);
                    if (idx >= paragraphs.Count) break;

                    var pLine = paragraphs[idx].Text;
                    if (IsQuestionStart(pLine) || pLine.StartsWith("[PART", StringComparison.OrdinalIgnoreCase) ||
                        Regex.IsMatch(pLine, @"^\[(?:PASSAGE|GROUP)", RegexOptions.IgnoreCase) ||
                        pLine.Equals("[/GROUP]", StringComparison.OrdinalIgnoreCase) ||
                        pLine.Equals("[/PASSAGE]", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pLine.Equals("[/GROUP]", StringComparison.OrdinalIgnoreCase) || pLine.Equals("[/PASSAGE]", StringComparison.OrdinalIgnoreCase))
                            idx++;
                        break;
                    }

                    // Kiểm tra nếu là hình ảnh
                    if (paragraphs[idx].HasImage)
                    {
                        if (passageLines.Any())
                        {
                            currentGroup.PassageItems.Add(new ParsedPassageItem
                            {
                                OrderIndex = currentGroup.PassageItems.Count + 1,
                                Type = PassageItemType.Text,
                                TextContent = string.Join("\n", passageLines)
                            });
                            passageLines.Clear();
                        }

                        var imgItem = new ParsedPassageItem
                        {
                            OrderIndex = currentGroup.PassageItems.Count + 1,
                            Type = PassageItemType.Image
                        };
                        if (extractImages && mainPart != null && !string.IsNullOrEmpty(paragraphs[idx].ImageRelId))
                        {
                            try
                            {
                                var imgPart = (ImagePart)mainPart.GetPartById(paragraphs[idx].ImageRelId);
                                using var s = imgPart.GetStream();
                                using var ms = new MemoryStream();
                                s.CopyTo(ms);
                                imgItem.ImageBytes = ms.ToArray();
                                imgItem.ImageExtension = imgPart.ContentType.Split('/').LastOrDefault() ?? "png";
                            }
                            catch { }
                        }
                        currentGroup.PassageItems.Add(imgItem);
                    }
                    else
                    {
                        passageLines.Add(pLine);
                    }
                    idx++;
                }

                if (passageLines.Any())
                {
                    currentGroup.PassageItems.Add(new ParsedPassageItem
                    {
                        OrderIndex = currentGroup.PassageItems.Count + 1,
                        Type = PassageItemType.Text,
                        TextContent = string.Join("\n", passageLines)
                    });
                }
                continue;
            }

            // Kiểm tra thẻ QUESTION
            var qMatch = Regex.Match(line, @"^\[(?:QUESTION|Q)[\s:]*(\d+)\]$", RegexOptions.IgnoreCase);
            if (qMatch.Success)
            {
                int qNum = int.Parse(qMatch.Groups[1].Value);
                int qLineIndex = paragraphs[idx].Index;
                idx++;

                var (q, nextIdx) = ParseFlexibleQuestion(paragraphs, idx, qNum, currentPart, qLineIndex, errors);
                idx = nextIdx;

                if (q != null)
                {
                    allQuestionNumbers.Add(q.QuestionNumber);
                    if (currentPart == 5 || currentGroup == null)
                    {
                        result.StandaloneQuestions.Add(q);
                    }
                    else
                    {
                        currentGroup.Questions.Add(q);
                    }
                }
                continue;
            }

            idx++;
        }

        // Validate số lượng câu hỏi và tính liên tục
        ValidateQuestionContinuity(allQuestionNumbers, errors);

        return result;
    }

    private static (ParsedQuestion question, int nextIdx) ParseFlexibleQuestion(
        List<(int Index, string Text, bool HasImage, string ImageRelId)> paragraphs,
        int idx,
        int qNum,
        int partNumber,
        int sourceLine,
        List<ParseError> errors)
    {
        var q = new ParsedQuestion
        {
            QuestionNumber = qNum,
            PartNumber = partNumber,
            SourceParagraphIndex = sourceLine,
            Options = new List<ParsedOption>()
        };

        var questionContentLines = new List<string>();

        // 1. Đọc nội dung câu hỏi cho đến khi gặp đáp án A/B/C/D
        while (idx < paragraphs.Count)
        {
            var line = paragraphs[idx].Text.Trim();
            if (string.IsNullOrWhiteSpace(line)) { idx++; continue; }

            if (IsOptionLine(line, "A") || IsOptionLine(line, "B") ||
                IsOptionLine(line, "C") || IsOptionLine(line, "D") ||
                IsAnswerLine(line) || IsQuestionStart(line) ||
                line.StartsWith("[PART", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            questionContentLines.Add(line);
            idx++;
        }

        q.Content = string.Join("\n", questionContentLines).Trim();

        // 2. Đọc các lựa chọn A, B, C, D
        var labels = new[] { AnswerOption.A, AnswerOption.B, AnswerOption.C, AnswerOption.D };
        foreach (var label in labels)
        {
            idx = SkipEmpty(paragraphs, idx);
            if (idx >= paragraphs.Count)
            {
                errors.Add(new ParseError(sourceLine, $"Câu {qNum}: Thiếu lựa chọn [{label}]."));
                break;
            }

            var line = paragraphs[idx].Text.Trim();
            if (IsOptionLine(line, label.ToString()))
            {
                var optContent = ExtractOptionContent(line, label.ToString());
                q.Options.Add(new ParsedOption
                {
                    Label = label,
                    Content = optContent
                });
                idx++;
            }
            else
            {
                errors.Add(new ParseError(paragraphs[idx].Index, $"Câu {qNum}: Mong đợi lựa chọn {label} nhưng gặp '{line}'."));
            }
        }

        // 3. Đọc đáp án đúng [ANSWER: X] hoặc [ANS] X
        idx = SkipEmpty(paragraphs, idx);
        if (idx < paragraphs.Count && IsAnswerLine(paragraphs[idx].Text))
        {
            var ansStr = ExtractAnswerLetter(paragraphs[idx].Text);
            if (Enum.TryParse<AnswerOption>(ansStr, true, out var correctOpt))
            {
                q.CorrectOption = correctOpt;
            }
            else
            {
                errors.Add(new ParseError(paragraphs[idx].Index, $"Câu {qNum}: Đáp án đúng không hợp lệ '{ansStr}'."));
            }
            idx++;
        }
        else
        {
            errors.Add(new ParseError(sourceLine, $"Câu {qNum}: Thiếu đáp án đúng [ANSWER: ...]."));
        }

        // 4. Đọc giải thích [EXPLANATION] nếu có
        idx = SkipEmpty(paragraphs, idx);
        if (idx < paragraphs.Count && paragraphs[idx].Text.Equals("[EXPLANATION]", StringComparison.OrdinalIgnoreCase))
        {
            idx++;
            var expLines = new List<string>();
            while (idx < paragraphs.Count)
            {
                idx = SkipEmpty(paragraphs, idx);
                if (idx >= paragraphs.Count) break;

                var eLine = paragraphs[idx].Text;
                if (IsQuestionStart(eLine) || eLine.StartsWith("[PART", StringComparison.OrdinalIgnoreCase) ||
                    Regex.IsMatch(eLine, @"^\[(?:PASSAGE|GROUP)", RegexOptions.IgnoreCase) ||
                    eLine.Equals("[/Q]", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                expLines.Add(eLine);
                idx++;
            }
            q.Explanation = string.Join("\n", expLines).Trim();
        }

        // Bỏ qua thẻ đóng [/Q] nếu có
        if (idx < paragraphs.Count && paragraphs[idx].Text.Equals("[/Q]", StringComparison.OrdinalIgnoreCase))
        {
            idx++;
        }

        return (q, idx);
    }

    private static bool IsQuestionStart(string text) =>
        Regex.IsMatch(text, @"^\[(?:QUESTION|Q)[\s:]*\d+\]", RegexOptions.IgnoreCase);

    private static bool IsOptionLine(string line, string letter)
    {
        // Khớp: A. Text, [A] Text, A) Text
        return Regex.IsMatch(line, $@"^(?:\[?{letter}\]?[\.\)]|\\[{letter}\\])\s*", RegexOptions.IgnoreCase);
    }

    private static string ExtractOptionContent(string line, string letter)
    {
        return Regex.Replace(line, $@"^(?:\[?{letter}\]?[\.\)]|\\[{letter}\\])\s*", "", RegexOptions.IgnoreCase).Trim();
    }

    private static bool IsAnswerLine(string line) =>
        Regex.IsMatch(line, @"^\[(?:ANSWER|ANS)[\s:]*[A-D]\]?", RegexOptions.IgnoreCase);

    private static string ExtractAnswerLetter(string line)
    {
        var m = Regex.Match(line, @"[A-D]", RegexOptions.IgnoreCase);
        return m.Success ? m.Value.ToUpperInvariant() : "";
    }

    private static void ValidateQuestionContinuity(List<int> allNums, List<ParseError> errors)
    {
        if (!allNums.Any())
        {
            errors.Add(new ParseError(0, "Không tìm thấy câu hỏi nào trong đề thi."));
            return;
        }

        var sorted = allNums.OrderBy(n => n).ToList();

        // Kiểm tra trùng lặp câu hỏi
        var dupes = sorted.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        foreach (var d in dupes)
            errors.Add(new ParseError(0, $"Số câu hỏi {d} bị trùng lặp."));

        // Kiểm tra phạm vi 101-200
        if (sorted.First() != 101)
            errors.Add(new ParseError(0, $"Câu hỏi đầu tiên phải bắt đầu từ 101, hiện tại là câu {sorted.First()}."));
        if (sorted.Last() != 200)
            errors.Add(new ParseError(0, $"Câu hỏi cuối cùng phải là 200, hiện tại là câu {sorted.Last()}."));

        // Kiểm tra bị ngắt quãng
        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] != sorted[i - 1] + 1)
                errors.Add(new ParseError(0, $"Bị thiếu câu hỏi trong khoảng từ {sorted[i - 1] + 1} đến {sorted[i] - 1}."));
        }

        if (sorted.Distinct().Count() != TOTAL_QUESTIONS)
            errors.Add(new ParseError(0, $"Đề thi phải có đủ đúng {TOTAL_QUESTIONS} câu hỏi (từ 101 đến 200), hiện có {sorted.Distinct().Count()} câu."));
    }

    private static string GetParagraphText(Paragraph para)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var run in para.Elements<Run>())
            foreach (var text in run.Elements<Text>())
                sb.Append(text.Text);
        return sb.ToString();
    }

    private static (bool hasImage, string relId) HasInlineImage(Paragraph para, MainDocumentPart mainPart)
    {
        foreach (var drawing in para.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline>())
        {
            var graphic = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Graphic>().FirstOrDefault();
            if (graphic != null)
            {
                var graphicData = graphic.GraphicData;
                foreach (var pic in graphicData?.Descendants<DocumentFormat.OpenXml.Drawing.Pictures.Picture>() ?? Enumerable.Empty<DocumentFormat.OpenXml.Drawing.Pictures.Picture>())
                {
                    var relId = pic.BlipFill?.Blip?.Embed?.Value;
                    if (!string.IsNullOrEmpty(relId)) return (true, relId);
                }
            }
        }
        return (false, null);
    }

    private static int SkipEmpty(List<(int Index, string Text, bool HasImage, string ImageRelId)> paragraphs, int idx)
    {
        while (idx < paragraphs.Count && string.IsNullOrWhiteSpace(paragraphs[idx].Text) && !paragraphs[idx].HasImage)
            idx++;
        return idx;
    }
}
