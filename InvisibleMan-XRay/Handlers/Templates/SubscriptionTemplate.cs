using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InvisibleManXRay.Handlers.Templates
{
    using Services;
    using Models;
    using Models.Templates.Subscriptions;
    using Values;

    public class SubscriptionTemplate : ITemplate
    {
        private List<Type> templates;
        private Func<string, Status> convertConfigLinkToV2Ray;

        private LocalizationService LocalizationService => ServiceLocator.Get<LocalizationService>();

        public SubscriptionTemplate()
        {
            this.templates = new List<Type>();
        }

        public void Setup(Func<string, Status> convertConfigLinkToV2Ray)
        {
            this.convertConfigLinkToV2Ray = convertConfigLinkToV2Ray;
        }

        public void RegisterTemplates()
        {
            templates.Add(typeof(Jwt));
            templates.Add(typeof(Simple));
        }

        public Status ConvertLinkToSubscription(string link)
        {
            Template template = FindTemplate();
            if (template == null)
                return new Status(
                    code: Code.ERROR,
                    subCode: SubCode.UNSUPPORTED_LINK,
                    content: LocalizationService.GetTerm(Localization.UNSUPPORTED_SUBSCRIPTION_LINK)
                );

            Status fetchingStatus = template.FetchDataFromLink(link);
            if (fetchingStatus.Code == Code.ERROR)
                return fetchingStatus;

            List<ConfigData> v2RayList = template.ConvertToV2RayList(convertConfigLinkToV2Ray);
            if (Isv2RayListEmpty())
                return new Status(
                    code: Code.ERROR,
                    subCode: SubCode.INVALID_CONFIG,
                    content: LocalizationService.GetTerm(Localization.INVALID_SUBSCRIPTION)
                );

            return new Status(
                code: Code.SUCCESS,
                subCode: SubCode.SUCCESS,
                content: new SubscriptionInfo()
                {
                    Id = template.Id,
                    Name = template.Title,
                    Url = link,
                    Data = v2RayList
                }
            );

            Template FindTemplate()
            {
                foreach (Type type in templates)
                {
                    Template template = Activator.CreateInstance(type) as Template;
                    if (template.IsValid(link))
                        return template;
                }

                return null;
            }

            bool Isv2RayListEmpty() => v2RayList.Count == 0;
        }
    }
}