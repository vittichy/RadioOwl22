using Dtc.Common.Extensions;
using RadioOwl.Parsers.Data;
using System;
using System.IO;

namespace RadioOwl.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// Jméno pro uložení části pořadu
        /// </summary>
        /// <param name="radioDataPart"></param>
        /// <returns></returns>
        public string GenerateFileName(RadioDataPart radioDataPart)
        {
            var path = GeneratePath(radioDataPart);
            var fileName = GenerateFilename(radioDataPart);
            return Path.Combine(path, fileName);
        }

        /// <summary>
        /// Nageruji vhodné filename dle části pořadu
        /// </summary>
        public string GenerateReadmeFilename(RadioDataPart radioDataPart)
        {
            var path = GeneratePath(radioDataPart);
            var filename = GetReadmeFilename(radioDataPart);
            return Path.Combine(path, filename);
        }

        /// <summary>
        /// Nageruji vhodný adresář pro uložení pořadu
        /// </summary>
        private string GeneratePath(RadioDataPart radioDataPart)
        {
            var subPath = radioDataPart.RadioData.SiteDocumentPath?.Trim();
            if (string.IsNullOrEmpty(subPath))
            {
                subPath = radioDataPart.RadioData.ContentId?.Trim();
                if (string.IsNullOrEmpty(subPath))
                {
                    subPath = $"RADIOOWL_DOWNLOAD_{Guid.NewGuid()}";
                }
            }

            // nebezpecne znaky z filename + max delka jmena souboru
            var paths = subPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = paths[i].RemovePathInvalidChars().TrimToMaxLen(60);
            }
            var subPathFinal = string.Empty;
            for (var i = 0; i < paths.Length; i++)
            {
                subPathFinal = Path.Combine(subPathFinal, paths[i]);
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), subPathFinal);
        }

        /// <summary>
        /// Nageruji vhodné filename dle části pořadu
        /// </summary>
        private string GenerateFilename(RadioDataPart radioDataPart)
        {
            var filename = GetBaseFileName(radioDataPart);

            // skládá se z jednotlivých dílů? přidám tedy do filename
            if (radioDataPart.RadioData.ContentSerialAllParts.HasValue && radioDataPart.RadioData.ContentSerialAllParts.Value > 0)
            {
                string partNo = null;
                if (radioDataPart.PartNo.HasValue && radioDataPart.PartNo.Value > 0)
                {
                    // ideální situace znám vše
                    partNo = radioDataPart.RadioData.ContentSerialAllParts.Value > 99 ? $"{radioDataPart.PartNo:000}" : $"{radioDataPart.PartNo:00}";
                }
                else
                {
                    // divná situace, znám počet částí, ale neznám číslo části 
                    partNo = $"UNKOWN_PART_NO_{Guid.NewGuid()}";
                }
                filename = $"{partNo}-{radioDataPart.RadioData.ContentSerialAllParts}-{filename?.Trim()}";
            }

            filename = RemoveInvalidChars(filename.TrimToMaxLen(80));
            filename += ".mp3";
            return filename;
        }

        private string GetReadmeFilename(RadioDataPart radioDataPart)
        {
            var filename = GetBaseFileName(radioDataPart);
            filename = RemoveInvalidChars(filename.TrimToMaxLen(80));
            filename += ".readme";
            return filename;
        }

        private string GetBaseFileName(RadioDataPart radioDataPart)
        {
            // základní jméno pořadu
            var filename = radioDataPart.Title?.Trim();
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"UNKOWN_NAME_{Guid.NewGuid()}";
            }
            filename = RemoveInvalidChars(filename.TrimToMaxLen(80));
            return filename;
        }

        private string RemoveInvalidChars(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char ch in invalidChars)
            {
                fileName = fileName.Replace(ch.ToString(), "");
            }
            return fileName;
        }

    }
}
