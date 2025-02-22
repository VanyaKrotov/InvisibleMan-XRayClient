using System.Collections.Generic;
using System.IO;

namespace InvisibleManXRay.Models
{
    public class Subscription
    {
        private DirectoryInfo directory;

        public DirectoryInfo Directory => directory;

        public string Name => Info.Name;

        public SubscriptionInfo Info;

        public Subscription(DirectoryInfo directory, SubscriptionInfo info)
        {
            this.directory = directory;
            this.Info = info;
        }
    }

    public class SubscriptionInfo
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Url { get; set; }

        public List<ConfigData> Data { get; set; }
    }
}