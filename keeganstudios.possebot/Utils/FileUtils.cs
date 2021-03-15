using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public class FileUtils : IFileUtils
    {
        private readonly HttpClient _httpClient;
        public FileUtils(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public string BuildAudioFilePath(ulong guildId)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "files", guildId.ToString());

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public string CleanFileName(string fileName)
        {
            string cleanFileName = string.Empty;
            try
            {
                Regex illegalInFileName = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);
                cleanFileName = illegalInFileName.Replace(fileName, "");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine($"- {ex.StackTrace}");
                throw;
            }

            return cleanFileName;
        }

        public async Task SaveAudioFile(string filePath, string attachmentUrl)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var file = await _httpClient.GetStreamAsync(attachmentUrl))
            using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}
