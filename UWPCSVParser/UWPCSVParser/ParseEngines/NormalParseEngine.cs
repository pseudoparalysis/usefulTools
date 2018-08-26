using System;
using System.Collections.Generic;
using System.Linq;

namespace UWPCSVParser.ParseEngines
{
    public sealed class NormalParseEngine : IParseEngine
    {
        private char _delimiter { set; get; }
        private char _quote { set; get; }

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

                this._delimiter = delimiter;
                this._quote = quote;

                if(csvRecord != String.Empty)
                {   
                    //Don't plan to accept inside double quotes in our csv to reduce complexity.
                    this.AnalyzeField(fieldValues, csvRecord, false);
                }

                /*fieldValues will be modified by AnalyzeField as 
                 * Objects are reference types and therefore are passed by reference.
                 */
                List<string> fieldValuesList = fieldValues.ToList();

                return fieldValuesList;
        }

        public string BuildCsvRecord(Dictionary<string, string> recordRowDict, char delimiter, char recordDelimiter, char quote)
        {
            //TODO
            return "";
        }
        public string BuildCsvText(List<string> csvRowsList, char lineDelimiter)
        {
            //TODO
            return "";
        }

        private void AnalyzeField(List<string> fieldValuesList, string csvRecord, bool insideQuotes)
        {
            int endPoint = 0;

            while (csvRecord.Length != 0)
            {
                int delimiterPosition = csvRecord.IndexOf(this._delimiter);
                //.ToString() will convert '"' to "\"". The latter is present in the csvRecord String.
                int quotePosition = csvRecord.IndexOf(this._quote.ToString());

                string fieldValue = String.Empty;

                //No quote, no delimtier, that means its just the value. immediately add to fieldValueList and finish.
                if (delimiterPosition == -1 && quotePosition == -1)
                {
                    fieldValue = csvRecord.Substring(0).Replace("\n", String.Empty).Replace("\r", String.Empty);
                    fieldValuesList.Add(fieldValue);

                    csvRecord = String.Empty;
                }

                if (quotePosition == -1 || quotePosition > delimiterPosition && delimiterPosition != -1 && insideQuotes == false)
                {
                    fieldValue = csvRecord.Substring(0, delimiterPosition);
                    endPoint = delimiterPosition;

                }
                else if (delimiterPosition == -1 || quotePosition < delimiterPosition && insideQuotes == false)
                {
                    if (quotePosition == 0)
                    {
                        int closingQuotePosition = csvRecord.Substring(1).IndexOf(this._quote.ToString());
                        fieldValue = csvRecord.Substring(1, closingQuotePosition - 1);

                        endPoint = closingQuotePosition + 1;
                    }
                    else
                    {
                        fieldValue = csvRecord.Substring(0, quotePosition);

                        endPoint = quotePosition + 1;

                    }
                }

                if (fieldValue != String.Empty)
                {
                    fieldValuesList.Add(fieldValue);
                    csvRecord = csvRecord.Substring(endPoint + 1);

                }

            }
        }
    }
}
