using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace keeganstudios.possebot.Utils
{
    public class FileUtils : IFileUtils
    {
        private readonly ILogger<FileUtils> _logger;
        private readonly HttpClient _httpClient;
        public FileUtils(ILogger<FileUtils> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        public string BuildAudioFilePath(ulong guildId)
        {
            var path = string.Empty;
            try
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), "files", guildId.ToString());

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to build audio file path for Guild Id: {guildId}", guildId);
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
                _logger.LogError(ex, "Unable to clean file name {fileName}", fileName);
                throw;
            }

            return cleanFileName;
        }

        public async Task SaveAudioFile(string filePath, string streamUrl)
        {
            try
            {
                _logger.LogInformation("Saving stream from: {streamUrl} to file: {audioPath}", streamUrl, filePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (var file = await _httpClient.GetStreamAsync(streamUrl))
                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    await file.CopyToAsync(fileStream);
                }
                _logger.LogInformation("Saved stream from: {streamUrl} to file: {audioPath}", streamUrl, filePath);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to save audio file from: {streamUrl} to file: {audioPath}", streamUrl, filePath);
                throw;
            }
        }
    }
}
