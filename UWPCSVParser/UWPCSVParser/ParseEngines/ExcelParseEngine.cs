using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace UWPCSVParser.ParseEngines
{
    public sealed class ExcelParseEngine : IParseEngine
    {
        //NOTE: this parser parse the csv file generated from excel, or csv file that follows the rule of excel.
        //Input delimiters and quote will be ignored and overwritten to comform to excel rules.
        private const char _QUOTE = '"';
        private const string _ESC_QUOTE = "\"\"";
        private const string _SURR_QUOTE_PATTERN = "(\"([^\"]|\"\")*\")";
        private const string _CHAR_TO_ESC = "[\",]";
        private const string _LF = "\n";
        private const string _CR = "\r";
        private const char _DELIMITER = ',';
        private const string _NULL_STRING = "NULL";

        public IList<string> ExtractRecords(char recordDelimiter, string csvText)
        {
            String[] csvRecords = csvText.Split(recordDelimiter);

            //ToList will create a new Generic List object.
            //new List<string>(csvRecords) will do the same thing as well.
            List<string> recordsList = csvRecords.ToList();
            return recordsList;
        }

        public IList<string> ExtractFields(char delimiter, char quote, string csvRecord)
        {
            List<string> fieldValues = new List<string>();

            if (csvRecord.Replace(_LF, String.Empty).Replace(_CR, String.Empty) != String.Empty)
            {
                //Allow parsing of inside escaped quotes
                this.AnalyzeCsvField(fieldValues, csvRecord);
            }

            /*fieldValues will be modified by AnalyzeField as 
                * Objects are reference types and therefore are passed by reference.
                */
            List<string> fieldValuesList = fieldValues.ToList();
            return fieldValuesList;
        }

        public string BuildCsvRecord(Dictionary<string, string> recordRowDict, char delimiter, char recordDelimiter, char quote)
        {
            StringBuilder csvRecordRow = new StringBuilder();
            int i = 0;
            foreach(string val in recordRowDict.Values)
            {
                string valueStr = this.AnalyzeDictField(val);

                csvRecordRow.Append(valueStr);
                if(recordRowDict.Values.Count - 1 != i)
                {
                    csvRecordRow.Append(_DELIMITER);
                }

                i++;
            }

            return csvRecordRow.ToString();
        }

        public string BuildCsvText(List<string> csvRowsList, char lineDelimiter)
        {
            return String.Join(_LF, csvRowsList.Select(v => v).Where(v => !String.IsNullOrEmpty(v)));
        }

        private void AnalyzeCsvField(List<string> fieldValuesList, string csvRecord)
        {
            /* This version does not touch the data, therefore, do not run the risk of data corruption
             * The idea:
             * To capture all the values wrapped around double quotes. e.g. "[value]"
             * For excel, if there are any illegal values, like comma or double quote, the value will definitely be wrapped by double quotes.
             *      e.g. "illegal ""value"" right here, cool."
             * When we split by the delimiter, each part that has double quotes will not be touched, since they are definitely wrapped by double quotes.
             *      Because double quotes are illegal values
             * When a part does not have double quote at all, it is a legal value, hence, it is not wrapped by double quotes.
             *      In this case, since we want to get values wrpped by double quotes, we wrapped the legal value in double quotes.
             * Once wrapping is done, join back using delimiter again, then use regex to extract.
             * 
             * Note that empty fields will be an empty string, hence it will not have double quotes, and will have them added.
             *      Using the regex, an empty string will be captured.
             */
            string[] parts = csvRecord.Split(_DELIMITER);
            for(int i = 0, max = parts.Length; i < max; i++)
            {
                if (parts[i].IndexOf(_QUOTE) == -1)
                {
                    parts[i] = _QUOTE + parts[i] + _QUOTE;
                }
            }

            string recordWithAllValuesQuoted = String.Join(_DELIMITER.ToString(), parts);
            foreach (Match val in Regex.Matches(recordWithAllValuesQuoted, _SURR_QUOTE_PATTERN))
            {
                string value = val.ToString()
                    .Substring(1, val.ToString().LastIndexOf(_QUOTE) - 1)
                    .Replace(_ESC_QUOTE, _QUOTE.ToString())
                    .Replace(_LF, String.Empty)
                    .Replace(_CR, String.Empty);
                fieldValuesList.Add(value);
            }

        }

        private string AnalyzeDictField(string value)
        {
            string valueStr = String.Empty;
            if(value == null)
            {
                valueStr = _NULL_STRING;
            } else
            {
                if (value.IndexOf(_QUOTE) != -1)
                {
                    valueStr = value.Replace(_QUOTE.ToString(), _ESC_QUOTE);
                } else
                {
                    valueStr = value;
                }

                if (Regex.IsMatch(valueStr, _CHAR_TO_ESC))
                {
                    valueStr = _QUOTE + valueStr + _QUOTE;
                }
            }


            return valueStr;
        }
    }
}
