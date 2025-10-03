using System;

namespace Launcher.ServiceLib.Contracts
{
    public class NewsItem
    {
        public class TextItem { public string Text { get; set; } }
        public TextItem Title { get; set; }
        public TextItem Summary { get; set; }
        public DateTimeOffset PublishDate { get; set; }
    }
}