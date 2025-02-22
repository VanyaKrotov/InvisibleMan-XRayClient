using System;
using System.Net;

namespace InvisibleManXRay.Models.Templates.Subscriptions
{
    using System.Text;
    using Values;

    public class Simple : Template
    {
        public override bool IsValid(string link)
        {
            return true;
        }

        public override Status FetchDataFromLink(string link)
        {
            try
            {
                WebClient webClient = new WebClient();

                webClient.Headers.Add("User-Agent", "InvisibleMan(win64)");
                webClient.Headers.Add("Content-Type", "application/json");
                webClient.Headers.Add("accept", "*/*");

                Data = webClient.DownloadString(link);
                var title = webClient.ResponseHeaders.Get("profile-title");
                if (title.StartsWith("base64:"))
                {
                    Title = Encoding.UTF8.GetString(Convert.FromBase64String(title.Substring(7)));
                }
                else
                {
                    Title = title;
                }

                var test = webClient.ResponseHeaders.Get("Content-Disposition");
                if (!string.IsNullOrEmpty(test))
                {
                    Id = test.Split(" ")[1].Split("=")[1].Replace("\"", "");
                }

                if (!IsAnyDataExisits())
                {
                    throw new Exception();
                }

                return new Status(Code.SUCCESS, SubCode.SUCCESS, null);
            }
            catch
            {
                return new Status(
                    code: Code.ERROR,
                    subCode: SubCode.UNSUPPORTED_LINK,
                    content: LocalizationService.GetTerm(Localization.UNSUPPORTED_SUBSCRIPTION_LINK)
                );
            }
        }
    }
}