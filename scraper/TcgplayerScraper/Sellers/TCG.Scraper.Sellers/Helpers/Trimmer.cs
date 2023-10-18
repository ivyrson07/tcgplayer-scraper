using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCG.Scraper.Sellers.Helpers
{
    public class Trimmer
    {
        public static string GetRemainingString(string word, string keyword)
        {
            var startIndex = word.IndexOf(keyword) + keyword.Length;
            var remainingWord = word.Substring(startIndex);

            return remainingWord;
        }
    }
}
