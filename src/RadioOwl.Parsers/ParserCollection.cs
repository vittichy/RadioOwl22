using RadioOwl.Parsers.Parser;
using RadioOwl.Parsers.Parser.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace RadioOwl.Parsers
{
    /// <summary>
    /// Seznam dostupných parserů
    /// </summary>
    public class ParserCollection
    {
        /// <summary>
        /// Seznam dostupných parserů
        /// </summary>
        public readonly List<IPageParser> Parsers
                                = new List<IPageParser>()
                                {
                                    new MujRozhlas2020Parser()
                                    //Chain = /*new RozhlasPrehrat2018PageParser(
                                    //            new RozhlasPrehrat2017PageParser(*/
                                    //                new MujRozhlas2020Parser();
                                };


        /// <summary>
        /// Vrací parser vhodný pro url
        /// </summary>
        public IPageParser FindParser(string url)
        {
            return Parsers.FirstOrDefault(p => p.CanParse(url));
        }

        /// <summary>
        /// Existuje parser pro url?
        /// </summary>
        public bool ExistsParser(string url)
        {
            return FindParser(url) != null;
        }
    }
}   