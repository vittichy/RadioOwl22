using Dtc.Http;
using Dtc.Http.Http;
using RadioOwl.Parsers.Data;
using RadioOwl.Parsers.Parser.Interfaces;
using System.Threading.Tasks;

namespace RadioOwl.Parsers.Parser.Base
{
    /// <summary>
    /// Zaklad pro parsery stranek + http utility
    /// </summary>
    public abstract class PageParserBase : IPageParser
    {
        public virtual int Version { get { return 0; } }

        public abstract bool CanParse(string url);

        public abstract Task<bool> ParseAsync(RadioData radioData);

        /// <summary>
        /// Jednoduche stazeni url do stringu
        /// </summary>
        protected async Task<string> DownloadHtmlAsync(string url)
        {
            var asyncDownloader = new AsyncDownloader();
            var downloaderOutput = await asyncDownloader.GetString(url);
            return downloaderOutput.DownloadOk ? downloaderOutput.Output : null;
        }

        /// <summary>
        /// Jednoduche stazeni url do stringu
        /// </summary>
        protected async Task DownloadHtmlAsync(RadioData radioData)
        {
            radioData.HtmlPage = await DownloadHtmlAsync(radioData.Url);
        }
    }
}
