using System.Text.RegularExpressions;

namespace SedWin.Launcher.Utils.Ftp.RegexUtils
{
    public interface IRegexFactory
    {
        Regex Create(string pattern, RegexOptions options);
    }
}
