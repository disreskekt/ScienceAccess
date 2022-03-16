using System.Collections.Generic;
using System.Text;

namespace Api.Helpers
{
    public static class CharHelper
    {
        private static readonly Dictionary<char, string> mapRules = new Dictionary<char, string>
        {
            {'а', "a"},
            {'б', "b"},
            {'в', "v"},
            {'г', "g"},
            {'д', "d"},
            {'е', "e"},
            {'ё', "yo"},
            {'ж', "zh"},
            {'з', "z"},
            {'и', "i"},
            {'й', "j"},
            {'к', "k"},
            {'л', "l"},
            {'м', "m"},
            {'н', "n"},
            {'о', "o"},
            {'п', "p"},
            {'р', "r"},
            {'с', "s"},
            {'т', "t"},
            {'у', "u"},
            {'ф', "f"},
            {'х', "h"},
            {'ц', "c"},
            {'ч', "ch"},
            {'ш', "sh"},
            {'щ', "sch"},
            {'ъ', "j"},
            {'ы', "i"},
            {'ь', "j"},
            {'э', "e"},
            {'ю', "yu"},
            {'я', "ya"},
            {'А', "A"},
            {'Б', "B"},
            {'В', "V"},
            {'Г', "G"},
            {'Д', "D"},
            {'Е', "E"},
            {'Ё', "Yo"},
            {'Ж', "Zh"},
            {'З', "Z"},
            {'И', "I"},
            {'Й', "J"},
            {'К', "K"},
            {'Л', "L"},
            {'М', "M"},
            {'Н', "N"},
            {'О', "O"},
            {'П', "P"},
            {'Р', "R"},
            {'С', "S"},
            {'Т', "T"},
            {'У', "U"},
            {'Ф', "F"},
            {'Х', "H"},
            {'Ц', "C"},
            {'Ч', "Ch"},
            {'Ш', "Sh"},
            {'Щ', "Sch"},
            {'Ъ', "J"},
            {'Ы', "I"},
            {'Ь', "J"},
            {'Э', "E"},
            {'Ю', "Yu"},
            {'Я', "Ya"},
        };

        public static string CastCyrillicToEnglish(this string cyrillic)
        {
            StringBuilder sb = new StringBuilder(cyrillic.Length * 2);

            for (int i = 0; i < cyrillic.Length; i++)
            {
                if (mapRules.ContainsKey(cyrillic[i]))
                {
                    sb.Append(mapRules[cyrillic[i]]);
                }
                else
                {
                    sb.Append(cyrillic[i]);
                }
            }

            return sb.ToString();
        }
        
        public static bool IsEnglishLower(this char symbol)
        {
            return symbol >= 97 && symbol <= 122;
        }
        
        public static bool IsEnglishUpper(this char symbol)
        {
            return symbol >= 65 && symbol <= 90;
        }
        
        public static bool IsCyrillicLower(this char symbol)
        {
            return symbol >= 1072 && symbol <= 1103 || symbol == 1105;
        }
        
        public static bool IsCyrillicUpper(this char symbol)
        {
            return symbol >= 1040 && symbol <= 1071 || symbol == 1025;
        }
        
        public static bool IsDigit(this char symbol)
        {
            return symbol >= 48 && symbol <= 57;
        }
    }
}