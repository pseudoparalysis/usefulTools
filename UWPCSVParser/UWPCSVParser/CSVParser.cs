using System;
using System.Collections.Generic;
using System.Linq;

namespace UWPCSVParser
{
    public class CSVParser
    {
        //Default values
        private const char DEFAULT_LINEDELIMITER = '\n';
        private const char DEFAULT_DELIMITER = ',';
        private const char DEFAULT_QUOTE = '"';
        private const bool DEFAULT_HASHEADERROW = true;

        public char LineDelimiter { get; set; }
        public char Delimiter { get; set; }
        public char Quote { get; set; }
        public bool HasHeaderRow { get; set; }
        public IParseEngine ParseEngine { get; private set; }

        public CSVParser(
            char lineDelimiter = DEFAULT_LINEDELIMITER,
            char delimiter = DEFAULT_DELIMITER,
            char quote = DEFAULT_QUOTE,
            bool hasHeaderRow = DEFAULT_HASHEADERROW
            )
        {
            this.InitProperties(lineDelimiter, delimiter, quote, hasHeaderRow);
        }

        public IEnumerable<IDictionary<string, string>> ParseFromCsv(string rawCsvText)
        {

            IEnumerable<IDictionary<string, string>> parsedCsv = this.ParseFromCsvRows(rawCsvText);

            return parsedCsv;
        }

        public Dictionary<string, Dictionary<string,string>> DictParseFromCsv(string rawCsvText, string key)
        {
            /*Returns a dictionary of dictionary, which the key being a specified value.
             * e.g. Given the record { "id" : "abc", "name" : "def"},
             * if the key provided upon invoking this method is "id", then the output will be
             * {"abc" : { "id" : "abc", "name" : "def"}}
             * Doing this allows extremely quick searching.
             * */

            Dictionary<string, Dictionary<string, string>> parsedCsv = this.DictParseFromCsvRows(rawCsvText, key);

            return parsedCsv;
        }

        public string ParseToCsv(IEnumerable<IDictionary<string, string>> valueToParse)
        {

            string headerRow = this.GetHeaderFieldsFromDictEnum(valueToParse);

            List<string> csvRowsList = new List<string>();
            int headerCount = valueToParse.FirstOrDefault().Count;
            csvRowsList.Add(headerRow);

            foreach (Dictionary<string, string> recordRow in valueToParse)
            {
                string csvRecord = this.ParseEngine.BuildCsvRecord(recordRow, this.Delimiter, this.LineDelimiter, this.Quote);
                csvRowsList.Add(csvRecord);

                if(recordRow.Count > headerCount)
                {
                    string newHeader = String.Join(",", recordRow.Keys.Select(key => key)) + "\n";
                    headerCount = recordRow.Count;
                    csvRowsList[0] = newHeader;
                }
            }

            string csvText = this.ParseEngine.BuildCsvText(csvRowsList, this.LineDelimiter);

            return csvText;


        }

        private void InitProperties(char lineDelimiter, char delimiter, char quote, bool hasHeaderRow)
        {
            this.LineDelimiter = lineDelimiter;
            this.Delimiter = delimiter;
            this.Quote = quote;
            this.HasHeaderRow = hasHeaderRow;
            //Using excel parser as default
            this.ParseEngine = new ParseEngines.ExcelParseEngine();
        }

        private IEnumerable<IDictionary<string, string>> ParseFromCsvRows(string rawCsvText)
        {
            List<Dictionary<string, string>> parsedCsv = new List<Dictionary<string, string>>();
            IList<string> recordRows = this.ParseEngine.ExtractRecords(this.LineDelimiter, rawCsvText);

            List<string> headerFields = this.GetHeaderFieldsFromCsv(recordRows);

            for(int i = this.HasHeaderRow ? 1 : 0, max = recordRows.Count; i < max; i++)
            {
                string oneRecordRow = recordRows[i];
                Dictionary<string, string> recordDict = this.ParseCsvRecordIntoDictionary(headerFields, oneRecordRow);

                if(recordDict.Count > 0)
                {
                    parsedCsv.Add(recordDict);
                }
            }

            return parsedCsv;
        }

        private Dictionary<string, Dictionary<string, string>> DictParseFromCsvRows(string rawCsvText, string key)
        {
            Dictionary<string, Dictionary<string, string>> parsedCsv = new Dictionary<string, Dictionary<string, string>>();
            IList<string> recordRows = this.ParseEngine.ExtractRecords(this.LineDelimiter, rawCsvText);

            List<string> headerFields = this.GetHeaderFieldsFromCsv(recordRows);

            for (int i = this.HasHeaderRow ? 1 : 0, max = recordRows.Count; i < max; i++)
            {
                string oneRecordRow = recordRows[i];
                Dictionary<string, string> recordDict = this.ParseCsvRecordIntoDictionary(headerFields, oneRecordRow);

                if (recordDict.Count > 0 && recordDict.ContainsKey(key))
                {
                    if(!parsedCsv.ContainsKey(recordDict[key]))
                    {
                        parsedCsv.Add(recordDict[key], recordDict);
                    }
                }
            }

            return parsedCsv;
        }

        private Dictionary<string,string> ParseCsvRecordIntoDictionary(List<string> headerFields, string oneRecordRow)
        {
            Dictionary<string, string> parsedRecordDict = new Dictionary<string, string>();
            IList<string> fieldValues = this.ParseEngine.ExtractFields(this.Delimiter, this.Quote, oneRecordRow);

            int i = 0;

            if (fieldValues.Count == headerFields.Count)
            {
                foreach (string header in headerFields)
                {
                    parsedRecordDict.Add(header, fieldValues[i]);
                    i++;
                }
            }

            return parsedRecordDict;
        }

        private List<string> GetHeaderFieldsFromCsv(IList<string> recordRows) {

            List<string> headerRow = new List<string>();

            if (this.HasHeaderRow)
            {
                headerRow = recordRows.FirstOrDefault()
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Split(this.Delimiter)
                    .ToList();
            }
            else
            {
                int numOfColumns = recordRows.FirstOrDefault().Split(this.Delimiter).Length;
                headerRow = new List<string>();
                for(int i=0; i < numOfColumns; i++)
                {
                    headerRow.Add(i.ToString());
                }

            }

            return headerRow;
        }

        private string GetHeaderFieldsFromDictEnum(IEnumerable<IDictionary<string, string>> valueToParse)
        {
            if(this.HasHeaderRow)
            {
                return String.Join(",", valueToParse.FirstOrDefault().Keys.Select(key => key));
            } else
            {
                return String.Empty;
            }
        }
    }
}
