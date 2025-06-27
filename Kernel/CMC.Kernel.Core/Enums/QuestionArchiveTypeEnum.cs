using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Enums
{
    public enum QuestionArchiveTypeEnum
    {
        None = 0,           // Only exclude parent competitions (default behavior)
        TimeBased = 1,      // Exclude questions from last X months
        CompetitionBased = 2, // Exclude questions from specific competitions
        Global = 3,         // Exclude questions from all previous competitions
        DateRange = 4       // Exclude questions from custom date range
    }
}
