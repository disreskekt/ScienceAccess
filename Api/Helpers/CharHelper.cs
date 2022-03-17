using System.Collections.Generic;
using System.Text;

namespace Api.Helpers
{
    public static class CharHelper
    {
        public static bool IsEnglishLower(this char symbol)
        {
            return symbol >= 97 && symbol <= 122;
        }
        
        public static bool IsEnglishUpper(this char symbol)
        {
            return symbol >= 65 && symbol <= 90;
        }
        
        public static bool IsCyrillicLower(this char symbol) //todo remove when there will be any language
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