﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.News
{
    public class NewsHeader : BreadcrumbControlOverlayHeader
    {
        public static LocalisableString FrontPageString => NewsStrings.FrontPageString;
        public static LocalisableString HeaderTitle => NewsStrings.HeaderTitle;
        public static LocalisableString HeaderDescription => NewsStrings.HeaderDescription;

        public Action ShowFrontPage;

        private readonly Bindable<string> article = new Bindable<string>();

        public NewsHeader()
        {
            TabControl.AddItem(FrontPageString);

            article.BindValueChanged(onArticleChanged, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(e =>
            {
                if (e.NewValue == FrontPageString)
                    ShowFrontPage?.Invoke();
            });
        }

        public void SetFrontPage() => article.Value = null;

        public void SetArticle(string slug) => article.Value = slug;

        private void onArticleChanged(ValueChangedEvent<string> e)
        {
            if (e.OldValue != null)
                TabControl.RemoveItem(e.OldValue);

            if (e.NewValue != null)
            {
                TabControl.AddItem(e.NewValue);
                Current.Value = e.NewValue;
            }
            else
            {
                Current.Value = FrontPageString;
            }
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/news");

        protected override OverlayTitle CreateTitle() => new NewsHeaderTitle();

        private class NewsHeaderTitle : OverlayTitle
        {
            public NewsHeaderTitle()
            {
                Title = HeaderTitle;
                Description = HeaderDescription;
                IconTexture = "Icons/Hexacons/news";
            }
        }
    }
}
