using System.Collections;
using System.Collections.Generic;

namespace Barcode
{
    public class KeyConvertor
    {
        public static Dictionary<int, List<string>> Keys { get; set; }

        public KeyConvertor()
        {
            CreateDictionary();
        }

        public static void CreateDictionary()
        {
            Keys = new Dictionary<int, List<string>>
            {
                { 16, new List<string>() { "a", "A", "" } },
                { 48, new List<string>() { "b", "B", "" } },
                { 46, new List<string>() { "c", "C", "" } },
                { 32, new List<string>() { "d", "D", "" } },
                { 18, new List<string>() { "e", "E", "€" } },
                { 33, new List<string>() { "f", "F", "" } },
                { 34, new List<string>() { "g", "G", "" } },
                { 35, new List<string>() { "h", "H", "" } },
                { 23, new List<string>() { "i", "I", "" } },
                { 36, new List<string>() { "j", "J", "" } },
                { 37, new List<string>() { "k", "K", "" } },
                { 38, new List<string>() { "l", "L", "" } },
                { 39, new List<string>() { "m", "M", "" } },
                { 49, new List<string>() { "n", "N", "" } },
                { 24, new List<string>() { "o", "O", "" } },
                { 25, new List<string>() { "p", "P", "" } },
                { 30, new List<string>() { "q", "Q", "" } },
                { 19, new List<string>() { "r", "R", "" } },
                { 31, new List<string>() { "s", "S", "" } },
                { 20, new List<string>() { "t", "T", "" } },
                { 22, new List<string>() { "u", "U", "" } },
                { 47, new List<string>() { "v", "V", "" } },
                { 44, new List<string>() { "w", "W", "" } },
                { 45, new List<string>() { "x", "X", "" } },
                { 21, new List<string>() { "y", "Y", "" } },
                { 17, new List<string>() { "z", "Z", "" } },
                { 28, new List<string>() { "", "", "" } }, //Return
                { 42, new List<string>() { "1", "1", "" } },  // LShiftKey
                { 56, new List<string>() { "2", "2", "" } }, //Alt Gr 
                { 50, new List<string>() { ",", "?", "" } },
                { 51, new List<string>() { ";", ".", "" } },
                { 52, new List<string>() { ":", "/", "" } },
                { 53, new List<string>() { "!", "§", "" } },
                { 2, new List<string>() { "&", "1", "" } },
                { 3, new List<string>() { "é", "2", "~" } },
                { 4, new List<string>() { "\"", "3", "#" } },
                { 5, new List<string>() { "'", "4", "{" } },
                { 6, new List<string>() { "(", "5", "[" } },
                { 7, new List<string>() { "-", "6", "|" } },
                { 8, new List<string>() { "è", "7", "`" } },
                { 9, new List<string>() { "_", "8", "\\" } },
                { 10, new List<string>() { "ç", "9", "^" } },
                { 11, new List<string>() { "à", "0", "@" } },
                { 12, new List<string>() { ")", "°", "]" } },
                { 13, new List<string>() { "=", "+", "}" } },
                { 26, new List<string>() { "^", "¨", "" } },
                { 57, new List<string>() { "$", "£", "¤" } },
                { 27, new List<string>() { "ù", "%", "" } },
                { 43, new List<string>() { "*", "µ", "" } }
            };

        }
    }
}
