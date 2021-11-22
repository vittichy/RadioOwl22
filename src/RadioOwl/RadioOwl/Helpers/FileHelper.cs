using Dtc.Common.Extensions;
using RadioOwl.Parsers.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioOwl.Helpers
{
    public class FileHelper
    {
        public void GenerateFilename(RadioDataPart radioDataPart)
        {
            var subPath = radioDataPart.RadioData.SiteDocumentPath?.Trim();
            if (string.IsNullOrEmpty(subPath))
                subPath = radioDataPart.RadioData.ContentId?.Trim();
            if (string.IsNullOrEmpty(subPath))
                subPath = $"RADIOOWL_DOWNLOAD_{Guid.NewGuid()}";
            // nebezpecne znaky z filename + max delka jmena souboru
            //    subPath = subPath.Replace('\\', '/');

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

            //subPath = TrimToMaxLen(RemoveInvalidChars(subPath), 60);

            var filename = radioDataPart.Title?.Trim();
            if (string.IsNullOrEmpty(filename) && radioDataPart.PartNo.HasValue)
                filename = $"{radioDataPart.PartNo:000}";
            if (string.IsNullOrEmpty(filename))
                filename = $"PART_{Guid.NewGuid()}";
            filename = $"{radioDataPart.PartNo:000}-{radioDataPart.RadioData.ContentSerialAllParts}-{filename?.Trim()}";
            filename = RemoveInvalidChars(filename.TrimToMaxLen(80));
            filename += ".mp3";

            radioDataPart.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), subPathFinal, filename);
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
