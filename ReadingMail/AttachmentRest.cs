using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;

namespace ReadingMail
{
    class AttachmentRest
    {
        /// <summary>
        /// Send documents
        /// </summary>
        /// <param name="attachmentBase64"></param>
        /// <param name="attachmentName"></param>
        /// <param name="filePath"></param>
        /// <param name="attachmentKey"></param>
        /// <param name="attachmentFolderKey"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        /// <returns>parseAttachmentDocumentOid</returns>
        public static async Task<string> AttachmentLoginAsync(string attachmentBase64, string attachmentName, string filePath, int attachmentKey, int attachmentFolderKey, string informationLogoUsername, string informationLogoPassword, string sessionId, string informationLogLevel)
        {
            try
            {
                DateTime _CreatedDateTimeAnd_LastModifiedDateTime = DateTime.Now;
                var parseAttachmentDocumentOid = "";

                if (string.Compare(informationLogLevel, "True") == 0)
                {
                    if (sessionId == "" || sessionId != null)
                    {
                        Logger.Log("SessionId not found");
                    }
                }

                if (sessionId != "" && sessionId != null)
                {
                    #region JsonData
                    JObject payload = new JObject(
                  new JProperty("FilePath", attachmentName), // mail konusu 
                  new JProperty("Tags", filePath),
                  new JProperty("NotifyUsers", null),
                  new JProperty("ContentBase64", attachmentBase64),
                  new JProperty("FileName", attachmentName), //dosya adını yazdır
                  new JProperty("SaveDocumentType", 2),
                  new JProperty("SharedFilePath", attachmentFolderKey + "-" + "EmailAttacment\\" + attachmentName),
                  new JProperty("_CreatedDateTime", _CreatedDateTimeAnd_LastModifiedDateTime),
                  new JProperty("_LastModifiedDateTime", _CreatedDateTimeAnd_LastModifiedDateTime),
                  new JProperty("IsLoading", false),
                  new JProperty("IsDeleted", false),
                  new JProperty("Loading", false),
                  new JProperty("IsMobile", false),
                  new JProperty("IsControllerSaving", false),
                  new JProperty("MobileSessionUser", "00000000-0000-0000-0000-000000000000")
                   );

                    #endregion

                    #region TicketAttachmentPost

                    HttpClient httpClient = new HttpClient();
                    var httpContent = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                    HttpResponseMessage responseTicket = await httpClient.PostAsync("http://localhost/LogoCrmRest//api/v1.0/documents?SessionId=" + sessionId, httpContent);
                    responseTicket.EnsureSuccessStatusCode();
                    string responseBody = await responseTicket.Content.ReadAsStringAsync();
                    var responseParse = JObject.Parse(responseBody);
                    dynamic jperson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    var item = jperson["Items"].ToString();
                    var itemconvertjsonTicketFirmOid = item.Substring(1, item.Length - 1).Substring(0, item.Length - 2);
                    parseAttachmentDocumentOid = JsonParseAttachmentDocumentOid(itemconvertjsonTicketFirmOid);

                    #endregion

                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log(attachmentName + " - attachment posted");
                    return parseAttachmentDocumentOid;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
                return null;
            }
        }
        public static string JsonParseAttachmentDocumentOid(string data)
        {
            string parseSession = "";
            if (data == "" || data == null)
                return "";
            JObject json = JObject.Parse(data);
            parseSession = json["Oid"].ToString();
            return parseSession;
        }
    }
}