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
    class Comment
    {
        public static async void CommentPostAsync(string objectOid, string comment, string informationLogoUsername, string informationLogoPassword, string sessionId, string informationLogLevel)
        {

            #region JsonData

            JObject payload = new JObject(
          new JProperty("content", comment),
          new JProperty("created", "2019-02-26 14:14:25.545"),
          new JProperty("modified", "2019-02-26 14:14:25.545"),
          new JProperty("moduleName", "MT_Ticket"),
          new JProperty("objectId", objectOid)
          );

            #endregion

            #region TicketCommentPost

            HttpClient httpClient = new HttpClient();
            var httpContent = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage responseTicket = await httpClient.PostAsync("*****************************" + sessionId, httpContent);
            responseTicket.EnsureSuccessStatusCode();
            string responseBody = await responseTicket.Content.ReadAsStringAsync();
            var responseParse = JObject.Parse(responseBody);
            HttpWebRequest requestsupport = (HttpWebRequest)WebRequest.Create("*************************" + sessionId);
            requestsupport.Method = WebRequestMethods.Http.Post;
            StreamWriter swJSONPayload = new StreamWriter(requestsupport.GetRequestStream());
            swJSONPayload.Write(payload);

            Logger.Log(objectOid + " comment added");

            #endregion
        }

    }
}
