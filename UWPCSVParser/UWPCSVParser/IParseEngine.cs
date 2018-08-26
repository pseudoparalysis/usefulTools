
using System.Collections.Generic;

namespace UWPCSVParser
{
    public interface IParseEngine
    {
        //Record => one row, field => one value delimited by delimiter in a row.
        IList<string> ExtractRecords(char recordDelimiter, string csvText);
        IList<string> ExtractFields(char delimiter, char quote, string csvRecord);
        string BuildCsvRecord(Dictionary<string, string> recordRowDict, char delimiter, char recordDelimiter, char quote);
        string BuildCsvText(List<string> csvRowsList, char lineDelimiter);
    }
}
