using System;
using System.Text;
using System.Collections.Generic;

namespace InvisibleManXRay.Models.Templates.Subscriptions
{
    using Services;
    using Utilities;

    public abstract class Template
    {
        protected string Data;

        public string Title;

        public string Id;

        protected LocalizationService LocalizationService => ServiceLocator.Get<LocalizationService>();

        public abstract bool IsValid(string link);

        public abstract Status FetchDataFromLink(string link);

        public string GetValidRemark(string remark) => FileUtility.GetValidFileName(remark);

        public List<ConfigData> ConvertToV2RayList(Func<string, Status> convertConfigLinkToV2Ray)
        {
            string data;
            List<ConfigData> v2RayList = new List<ConfigData>();

            TryDecode();
            TryConvert();
            return v2RayList;

            void TryDecode()
            {
                TryDecodeAsBase64();
                if (IsDecodeSucceeded())
                    return;

                TryDecodeAsStringArray();
                if (IsDecodeSucceeded())
                    return;

                void TryDecodeAsBase64()
                {
                    try
                    {
                        data = Encoding.UTF8.GetString(
                            bytes: Convert.FromBase64String(Data)
                        );
                    }
                    catch
                    {
                        data = null;
                    }
                }

                void TryDecodeAsStringArray()
                {
                    data = Data;
                }

                bool IsDecodeSucceeded() => !string.IsNullOrEmpty(data);
            }

            void TryConvert()
            {
                foreach (string link in data.Split("\n"))
                {
                    Status convertingStatus = convertConfigLinkToV2Ray.Invoke(link);
                    if (convertingStatus.Code == Code.SUCCESS)
                    {

                        v2RayList.Add(
                           (ConfigData)convertingStatus.Content
                        );
                    }
                }
            }
        }

        protected bool IsAnyDataExisits() => !string.IsNullOrEmpty(Data);
    }
}