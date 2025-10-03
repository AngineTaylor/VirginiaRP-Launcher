using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Launcher.ServiceLib.Contracts
{
    [DataContract]
    public class NewsItemDto
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Emoji { get; set; }
        [DataMember] public string Title { get; set; }
        [DataMember] public string Summary { get; set; }
        [DataMember] public string Content { get; set; }
        [DataMember] public DateTime PublishDate { get; set; }
        [DataMember] public string Author { get; set; }
        [DataMember] public int Version { get; set; } = 1;
    }

    [DataContract]
    public class NewsResponse
    {
        [DataMember] public List<NewsItemDto> News { get; set; } = new List<NewsItemDto>();
        [DataMember] public int Version { get; set; } = 1;
    }
}