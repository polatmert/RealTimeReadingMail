using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadingMail
{
    class RelatedDocumentRest
    {
        /// <summary>
        /// The documents in the mail are posted.
        /// </summary>
        /// <param name="ticketOid"></param>
        /// <param name="documentOid"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        public static async void ReletadDocumentPostAsync(string ticketOid, List<string> documentOid, string informationLogoUsername, string informationLogoPassword, string sessionId, string informationLogLevel)
        {
            try
            {
                #region TicketDocumentRelationship
                if (sessionId != "" && sessionId != null)
                {
                    for (int i = 0; i < documentOid.Count; i++)
                    {
                        HttpClient httpClient = new HttpClient();
                        var httpContent = new StringContent("application/json");
                        HttpResponseMessage responseTicket = await httpClient.PostAsync("***********************************" + ticketOid + "/DocumentOid/" + documentOid[i] + "?SessionId=" + sessionId, httpContent);
                        responseTicket.EnsureSuccessStatusCode();
                        string responseBody = await responseTicket.Content.ReadAsStringAsync();
                        var responseParse = JObject.Parse(responseBody);
                        if (string.Compare(informationLogLevel, "True") == 0)
                            Logger.Log("Ticket and attachment relationship established. TicketOid = " + ticketOid + "AttachmentOid = " + documentOid[i]);
                    }
                }
                #endregion              
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }
    }
}