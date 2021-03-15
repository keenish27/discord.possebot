using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public interface IFileUtils
    {
        string BuildAudioFilePath(ulong guildId);
        string CleanFileName(string fileName);
        Task SaveAudioFile(string filePath, string attachmentUrl);
    }
}
