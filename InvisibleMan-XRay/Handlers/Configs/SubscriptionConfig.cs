using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InvisibleManXRay.Handlers.Configs
{
    using Models;
    using Values;
    using Utilities;

    public class SubscriptionConfig : BaseConfig
    {
        private List<Subscription> groups;

        public SubscriptionConfig() : base()
        {
            this.groups = new List<Subscription>();
        }

        public void Setup(Func<string> getCurrentConfigPath)
        {
            LoadFiles(getCurrentConfigPath.Invoke());
        }

        public void LoadGroups()
        {
            groups.Clear();

            DirectoryInfo directoryInfo = new DirectoryInfo(Directory.CONFIGS);
            if (!directoryInfo.Exists)
                return;

            DirectoryInfo[] directories = directoryInfo.GetDirectories().Where(
                directory => directory.GetFiles().Any(file => file.Name == "info.dat") &&
                    directory.GetFiles().Count() > 1
            ).OrderBy(
                directory => directory.CreationTime
            ).ToArray();

            foreach (DirectoryInfo directory in directories)
            {
                AddDirectoryToList(directory);
            }
        }

        public List<Subscription> GetAllGroups()
        {
            return groups;
        }

        public void CreateSubscription(SubscriptionInfo info)
        {
            string destinationDirectory = $"{Directory.CONFIGS}/{info.Id}";
            var configs = info.Data;

            if (!IsAnyConfigExists())
            {
                return;
            }

            CreateInfoFile();
            foreach (var config in configs)
            {
                CreateConfigFile(
                    config.Id,
                    JsonConvert.SerializeObject(config.Config)
                );
            }

            bool IsAnyConfigExists() => configs.Count > 0;

            void CreateInfoFile()
            {
                string destinationPath = $"{destinationDirectory}/info.dat";
                SaveToDirectory(destinationPath, JsonConvert.SerializeObject(info));
            }

            void CreateConfigFile(string id, string data)
            {
                string destinationPath = $"{destinationDirectory}/{id}.json";
                SaveToDirectory(destinationPath, data);
                SetFileTime(destinationPath);
                AddConfigToList(CreateConfigModel(destinationPath));

                void SetFileTime(string path) => FileUtility.SetFileTimeToNow(path);
            }

            void SaveToDirectory(string destinationPath, string data)
            {
                FileUtility.TryDeleteDirectory(destinationDirectory);
                FileUtility.CreateDirectory(destinationDirectory);
                FileUtility.SetDirectoryTimeToNow(destinationDirectory);
                File.WriteAllText(destinationPath, data);
            }
        }

        public void DeleteSubscription(Subscription subscription)
        {
            System.IO.Directory.Delete(subscription.Directory.FullName, true);
            groups.Remove(subscription);
        }

        public override void LoadFiles(string path)
        {
            LoadGroups();
            configs.Clear();

            if (!IsAnyGroupExists())
            { return; }

            DirectoryInfo directoryInfo = new DirectoryInfo(GetConfigDirectory());

            if (!IsValidDirectory())
                directoryInfo = groups.Last().Directory;

            FileInfo[] files = directoryInfo.GetFiles().Where(
                file => file.Extension != ".dat"
            ).OrderBy(file => file.CreationTime).ToArray();

            foreach (FileInfo file in files)
            {
                AddConfigToList(CreateConfigModel(file.FullName));
            }

            bool IsAnyGroupExists() => groups.Count > 0;

            bool IsValidDirectory() => directoryInfo.Exists && GetConfigDirectory() != GetRootConfigDirectory();

            string GetConfigDirectory()
            {
                string directory = path;
                if (!FileUtility.IsDirectory(path))
                { directory = FileUtility.GetDirectory(path); }

                path = $"{directory}/info.dat";
                if (!FileUtility.IsFileExists(path))
                { directory = Directory.CONFIGS; }

                return FileUtility.GetFullPath(directory);
            }

            string GetRootConfigDirectory()
            {
                return FileUtility.GetFullPath(Directory.CONFIGS);
            }
        }

        public override Config CreateConfigModel(string path)
        {
            var filename = GetFileName(path);
            var directory = GetDirectory(path);
            var name = filename;
            var group = groups.Find(x => x.Directory.FullName == directory);
            if (group != null)
            {
                name = group.Info.Data.Find(x => filename.Contains(x.Id))?.Name;
            }

            return new Config(
                path: $"{directory}/{filename}",
                name,
                type: ConfigType.FILE,
                group: GroupType.SUBSCRIPTION,
                updateTime: GetFileUpdateTime(path)
            );
        }

        private void AddDirectoryToList(DirectoryInfo directory)
        {
            groups.Add(new Subscription(directory, FetchUrlFromDirectory()));

            SubscriptionInfo FetchUrlFromDirectory() => JsonConvert.DeserializeObject<SubscriptionInfo>(File.ReadAllText($"{directory.FullName}/info.dat"));
        }
    }
}