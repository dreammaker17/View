using System.Text.RegularExpressions;

namespace SedWin.Launcher.Utils.Ftp.RegexUtils
{
    public class RegexFactory : IRegexFactory
    {
        public Regex Create(string pattern, RegexOptions options) => new Regex(pattern, options);
    }
}
