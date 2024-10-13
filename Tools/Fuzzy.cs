using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ESMetadata.Tools
{
    public static class Fuzzy
    {
        public static string quickRemoveArticlesAndTrim(string input)
        {
            void ProcessWord(StringBuilder r, StringBuilder w)
            {
                string wordStr = w.ToString();
                if (!wordStr.Equals("A", StringComparison.OrdinalIgnoreCase) &&
                    !wordStr.Equals("An", StringComparison.OrdinalIgnoreCase) &&
                    !wordStr.Equals("The", StringComparison.OrdinalIgnoreCase))
                {
                    r.Append(wordStr);
                }
                w.Clear();
            }

            if (string.IsNullOrEmpty(input)) return input;

            StringBuilder result = new StringBuilder();
            StringBuilder word = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (char.IsLetterOrDigit(c))
                {
                    word.Append(c);
                }
                else
                {
                    ProcessWord(result, word);
                    //result.Append(c);
                }
            }

            // Process the last word if there is one
            ProcessWord(result, word);

            return result.ToString();
        }

        private static string quickTrim(string name)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in name.ToCharArray())
            {
                if (char.IsLetterOrDigit(c))
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static string SimplifyName(string name, bool ignoreArticles)
        {
            int len = name.IndexOfAny(new char[] { '(', '[' });
            string cutted = (len > 0 ? name.Substring(0, len) : name).ToLower().Trim();
            return ignoreArticles ? quickRemoveArticlesAndTrim(cutted) : quickTrim(cutted);
        }

        public static int LevenshteinDistance(string source, string target, int maxDistance)
        {
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;
                int minInRow = v1[0];

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                    minInRow = Math.Min(minInRow, v1[j + 1]);
                }
                if (minInRow > maxDistance)
                    return minInRow; // Early exit

                Array.Copy(v1, v0, v0.Length);
            }

            return v1[target.Length];
        }

        public static double Similarity(string first, string second, double minSimilarity)
        {
            if ((first == null) || (first == null)) return 0.0;
            if ((first.Length == 0) || (first.Length == 0)) return 0.0;
            if (first == second) return 1.0;

            int fl = first.Length;
            int sl = second.Length;
            int maxLen = Math.Max(fl, sl);

            double lenSimiratiry = 1.0 - ((double) Math.Abs(fl - sl) / maxLen);

            if (lenSimiratiry < minSimilarity)
            {
                return lenSimiratiry;
            }

            int stepsToSame = LevenshteinDistance(first, second, (int)((1.0 - minSimilarity) * maxLen));
            return 1.0 - ((double)stepsToSame / (double)maxLen);
        }

    };
};