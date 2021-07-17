// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class NewsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.News";

        /// <summary>
        /// "frontpage"
        /// </summary>
        public static LocalisableString FrontPageString => new TranslatableString(getKey(@"front_page"), @"frontpage");

        /// <summary>
        /// "news"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"news");

        /// <summary>
        /// "join the real-time discussion"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"get up-to-date on community happenings");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
