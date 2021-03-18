using Discord;
using Microsoft.Extensions.Logging;
using System;

namespace keeganstudios.possebot.Utils
{
    public class EmbedBuilderUtils : IEmbedBuilderUtils
    {
        private readonly ILogger<EmbedBuilderUtils> _logger;

        public EmbedBuilderUtils(ILogger<EmbedBuilderUtils> logger)
        {
            _logger = logger;
        }
        public EmbedFieldBuilder BuildEmbedField(string name, string value, bool isInline)
        {
            EmbedFieldBuilder field = null;
            try
            {
                field = new EmbedFieldBuilder
                {
                    Name = name,
                    Value = value,
                    IsInline = isInline
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to build embeded field with name: {fieldName}, value: {fieldValue}, isInline: {fieldIsInline}", name, value, isInline);
                throw;
            }

            return field;
        }
    }
}
