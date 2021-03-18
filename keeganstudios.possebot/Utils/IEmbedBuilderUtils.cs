using Discord;

namespace keeganstudios.possebot.Utils
{
    public interface IEmbedBuilderUtils
    {
        EmbedFieldBuilder BuildEmbedField(string name, string value, bool isInline);
    }
}
