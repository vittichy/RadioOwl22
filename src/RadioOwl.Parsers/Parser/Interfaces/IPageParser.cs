using RadioOwl.Parsers.Data;
using System.Threading.Tasks;

namespace RadioOwl.Parsers.Parser.Interfaces
{
    /// <summary>
    /// Interface pro parsery 
    /// </summary>
    public interface IPageParser 
    {
        /// <summary>
        /// Umim parsovat zadane url?
        /// </summary>
        bool CanParse(string url);

        /// <summary>
        /// Parsování dat z url odkazu
        /// </summary>
        Task<bool> ParseAsync(RadioData radioData);
    }
}
