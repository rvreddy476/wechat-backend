using System.Text.RegularExpressions;

namespace AuthService.Api.Extensions
{
    public static class Utility
    {
        public static readonly Regex EmailRegex = new Regex(
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
    }
}
