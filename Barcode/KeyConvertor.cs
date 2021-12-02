using System.Collections.Generic;

namespace Barcode
{
    public class KeyConvertor
    {
        Dictionary<int, List<string>> dictionary;

        public KeyConvertor()
        {
            EngDictionary();
        }

        public void EngDictionary()
        {
            this.dictionary = new Dictionary<int, List<string>>();
            this.dictionary.Add(16, new List<string>() { "a", "A", "" });
            this.dictionary.Add(48, new List<string>() { "b", "B", "" });
            this.dictionary.Add(46, new List<string>() { "c", "C", "" });
            this.dictionary.Add(32, new List<string>() { "d", "D", "" });
            this.dictionary.Add(18, new List<string>() { "e", "E", "€"});
            this.dictionary.Add(33, new List<string>() { "f", "F", "" });
            this.dictionary.Add(34, new List<string>() { "g", "G", "" });
            this.dictionary.Add(35, new List<string>() { "h", "H", "" });
            this.dictionary.Add(23, new List<string>() { "i", "I", "" });
            this.dictionary.Add(36, new List<string>() { "j", "J", "" });
            this.dictionary.Add(37, new List<string>() { "k", "K", "" });
            this.dictionary.Add(38, new List<string>() { "l", "L", "" });
            this.dictionary.Add(39, new List<string>() { "m", "M", "" });
            this.dictionary.Add(49, new List<string>() { "n", "N", "" });
            this.dictionary.Add(24, new List<string>() { "o", "O", "" });
            this.dictionary.Add(25, new List<string>() { "p", "P", "" });
            this.dictionary.Add(30, new List<string>() { "q", "Q", "" });
            this.dictionary.Add(19, new List<string>() { "r", "R", "" });
            this.dictionary.Add(31, new List<string>() { "s", "S", "" });
            this.dictionary.Add(20, new List<string>() { "t", "T", "" });
            this.dictionary.Add(22, new List<string>() { "u", "U", "" });
            this.dictionary.Add(47, new List<string>() { "v", "V", "" });
            this.dictionary.Add(44, new List<string>() { "w", "W", "" });
            this.dictionary.Add(45, new List<string>() { "x", "X", "" });
            this.dictionary.Add(21, new List<string>() { "y", "Y", "" });
            this.dictionary.Add(17, new List<string>() { "z", "Z", "" });
            this.dictionary.Add(28, new List<string>() { "", "", "" }); //Return
            this.dictionary.Add(42, new List<string>() { "1", "1", "" });  // LShiftKey
            this.dictionary.Add(56, new List<string>() { "2", "2", "" }); //Alt Gr 
            this.dictionary.Add(50, new List<string>() { ",", "?", "" });
            this.dictionary.Add(51, new List<string>() { ";", ".", "" });
            this.dictionary.Add(52, new List<string>() { ":", "/", "" });
            this.dictionary.Add(53, new List<string>() { "!", "§", "" });
            this.dictionary.Add(2, new List<string>() { "&", "1", "" });
            this.dictionary.Add(3, new List<string>() { "é", "2", "~" });
            this.dictionary.Add(4, new List<string>() { "\"", "3", "#" });
            this.dictionary.Add(5, new List<string>() { "'", "4", "{" });
            this.dictionary.Add(6, new List<string>() { "(", "5", "[" });
            this.dictionary.Add(7, new List<string>() { "-", "6", "|" });
            this.dictionary.Add(8, new List<string>() { "è", "7", "`" });
            this.dictionary.Add(9, new List<string>() { "_", "8", "\\" });
            this.dictionary.Add(10, new List<string>() { "ç", "9", "^" });
            this.dictionary.Add(11, new List<string>() { "à", "0", "@"});
            this.dictionary.Add(12, new List<string>() { ")", "°", "]" });
            this.dictionary.Add(13, new List<string>() { "=", "+", "}" });
            this.dictionary.Add(26, new List<string>() { "^", "¨", "" });
            this.dictionary.Add(57, new List<string>() { "$", "£", "¤" });
            this.dictionary.Add(27, new List<string>() { "ù", "%", "" });
            this.dictionary.Add(43, new List<string>() { "*", "µ", "" });

        }

        public Dictionary<int, List<string>> getDictionary()
        {
            return this.dictionary;
        }

    }
}
