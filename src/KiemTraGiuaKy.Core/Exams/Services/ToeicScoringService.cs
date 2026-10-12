using Abp.Dependency;
using System;

namespace KiemTraGiuaKy.Exams.Services;

/// <summary>
/// Domain service interface for calculating official TOEIC Reading scaled scores (5 - 495).
/// </summary>
public interface IToeicScoringService
{
    /// <summary>
    /// Converts the raw number of correct answers (0 - 100) to the TOEIC Reading scaled score (5 - 495).
    /// </summary>
    int CalculateReadingScore(int correctCount);
}

/// <summary>
/// Implements standard ETS TOEIC Reading scaled score conversion.
/// </summary>
public class ToeicScoringService : IToeicScoringService, ITransientDependency
{
    // Standard ETS conversion table for Reading (100 questions)
    private static readonly int[] ReadingScoreTable = new int[]
    {
        5,   // 0
        5,   // 1
        5,   // 2
        5,   // 3
        5,   // 4
        5,   // 5
        5,   // 6
        5,   // 7
        5,   // 8
        5,   // 9
        10,  // 10
        15,  // 11
        20,  // 12
        25,  // 13
        30,  // 14
        35,  // 15
        40,  // 16
        45,  // 17
        50,  // 18
        55,  // 19
        60,  // 20
        65,  // 21
        70,  // 22
        75,  // 23
        80,  // 24
        85,  // 25
        90,  // 26
        95,  // 27
        100, // 28
        105, // 29
        110, // 30
        115, // 31
        120, // 32
        125, // 33
        130, // 34
        135, // 35
        140, // 36
        145, // 37
        150, // 38
        155, // 39
        160, // 40
        165, // 41
        170, // 42
        175, // 43
        180, // 44
        185, // 45
        190, // 46
        195, // 47
        200, // 48
        205, // 49
        210, // 50
        215, // 51
        220, // 52
        225, // 53
        230, // 54
        235, // 55
        240, // 56
        245, // 57
        250, // 58
        255, // 59
        260, // 60
        265, // 61
        270, // 62
        275, // 63
        280, // 64
        285, // 65
        290, // 66
        295, // 67
        300, // 68
        305, // 69
        310, // 70
        315, // 71
        320, // 72
        325, // 73
        330, // 74
        335, // 75
        340, // 76
        345, // 77
        350, // 78
        355, // 79
        360, // 80
        365, // 81
        370, // 82
        375, // 83
        380, // 84
        385, // 85
        390, // 86
        395, // 87
        400, // 88
        405, // 89
        415, // 90
        420, // 91
        430, // 92
        440, // 93
        450, // 94
        460, // 95
        470, // 96
        480, // 97
        490, // 98
        495, // 99
        495  // 100
    };

    public int CalculateReadingScore(int correctCount)
    {
        if (correctCount <= 0) return 5;
        if (correctCount >= 100) return 495;
        return ReadingScoreTable[correctCount];
    }
}
