using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

namespace ReadingMail
{
    public class Rest
    {
        /// <summary>
        /// Create support record
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="subject"></param>
        /// <param name="messageBody"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        /// <returns></returns>
        public static async Task<string> RestAsync(string receiver, string subject, string messageBody, string informationLogoUsername, string informationLogoPassword, string sessionId, string informationLogLevel)
        {
            #region Variables

            string supportId = "";
            Guid responseParseOid = Guid.Empty;
            Guid responseParseTicketContactOid = Guid.Empty;
            Guid responseParseTicketContactFirmOid = Guid.Empty;
            DateTime ticketStartDate = DateTime.Now;
            string notes = messageBody;
            string RecordCreateInfoAndRecordLastUpdateInfo = DateTime.Now.ToString("dd-MM-yyyy ") + DateTime.Now.Hour + ":" + DateTime.Now.Minute;

            #endregion

            if (sessionId != "" && sessionId != null)
            {
                #region TicketFirmOid
                string strResponseValueTicketFirmOid = string.Empty;
                HttpWebRequest requestTicketFirmOid = (HttpWebRequest)WebRequest.Create("****************************" + sessionId + "&fields=FiOid,EmailAddress1&Filters=EmailAddress1[EQ]=" + receiver + "&isProcessUserAddedItems=false");
                requestTicketFirmOid.Method = "GET";
                HttpWebResponse responseTicketFirmOid = (HttpWebResponse)requestTicketFirmOid.GetResponse();
                Stream responseStreamTicketFirmOid = responseTicketFirmOid.GetResponseStream();
                StreamReader readerTicketFirmOid = new StreamReader(responseStreamTicketFirmOid);
                strResponseValueTicketFirmOid = readerTicketFirmOid.ReadToEnd().ToString();
                dynamic TicketFirmOid = JsonConvert.DeserializeObject<dynamic>(strResponseValueTicketFirmOid);
                var item2 = string.Empty;

                if (TicketFirmOid["Items"] != null)
                {
                    item2 = TicketFirmOid["Items"].ToString();
                }

                var itemconvertjsonTicketFirmOid = String.IsNullOrWhiteSpace(item2) ? String.Empty : item2.Substring(1, item2.Length - 1).Substring(0, item2.Length - 2);

                if (itemconvertjsonTicketFirmOid == "")
                {
                    responseParseOid = Guid.Empty;
                    responseParseTicketContactFirmOid = Guid.Empty;
                }
                else
                {
                    string myString = GenericMethods.JsonParseOid(itemconvertjsonTicketFirmOid);
                    Guid Guid2 = new Guid(myString);
                    responseParseTicketContactFirmOid = Guid2;
                }

                #endregion

                #region  TicketContactOid

                string strResponseValueTicketContactOid = string.Empty;
                HttpWebRequest requestTicketContactOid = (HttpWebRequest)WebRequest.Create("****************************************" + sessionId + "&fields=TicketContact&Filters=EmailAddress1[EQ]=" + receiver + "&isProcessUserAddedItems=false");
                requestTicketContactOid.Method = "GET";
                HttpWebResponse responseTicketContactOid = (HttpWebResponse)requestTicketContactOid.GetResponse();
                Stream responseStreamTicketContactOid = responseTicketContactOid.GetResponseStream();
                StreamReader readerTicketContactOid = new StreamReader(responseStreamTicketContactOid);
                strResponseValueTicketContactOid = readerTicketContactOid.ReadToEnd().ToString();
                dynamic TicketContactOid = JsonConvert.DeserializeObject<dynamic>(strResponseValueTicketContactOid);
                var itemconvertjsonTicketContactOid = String.Empty;

                if (TicketContactOid["Items"] != null)
                {
                    var itemTicketContactOid = TicketContactOid["Items"].ToString();
                    itemconvertjsonTicketContactOid = itemTicketContactOid.Substring(1, itemTicketContactOid.Length - 1).Substring(0, itemTicketContactOid.Length - 2);
                }

                //burada responseParseTicketContactOid alanı boş ise firmanın mail adresini varsa atar
                if (itemconvertjsonTicketContactOid == "")
                {
                    if (responseParseTicketContactFirmOid != null)
                    {
                        responseParseTicketContactOid = responseParseTicketContactFirmOid;
                    }
                    else
                    {
                        responseParseTicketContactOid = Guid.Empty;
                    }
                }
                else
                {
                    string myString2 = GenericMethods.JsonParseOid(itemconvertjsonTicketContactOid);
                    Guid Guid3 = new Guid(myString2);
                    responseParseTicketContactOid = Guid3;
                }

                #endregion

                #region
                //TicketIdGet
                string strResponseValuenewTicket = string.Empty;
                HttpWebRequest requestnewTicketInformation = (HttpWebRequest)WebRequest.Create("http://localhost/LogoCRMRest/api/v1.0/tickets/new?SessionId=" + sessionId);
                requestnewTicketInformation.Method = "GET";
                HttpWebResponse responsenewTicketInformation = (HttpWebResponse)requestnewTicketInformation.GetResponse();
                Stream responseStreamnewTicketInformation = responsenewTicketInformation.GetResponseStream();
                StreamReader readerTicketnewFirm = new StreamReader(responseStreamnewTicketInformation);
                strResponseValuenewTicket = readerTicketnewFirm.ReadToEnd().ToString();
                dynamic newTicketId = JsonConvert.DeserializeObject<dynamic>(strResponseValuenewTicket);
                var itemconvertjsonnewTicketId = String.Empty;
                var itemnewTicketId = newTicketId["Items"] == null ? String.Empty : newTicketId["Items"].ToString();
                itemconvertjsonnewTicketId = String.IsNullOrWhiteSpace(itemnewTicketId) ? String.Empty : itemnewTicketId.Substring(1, itemnewTicketId.Length - 1).Substring(0, itemnewTicketId.Length - 2);
                string newTicketIdInformation = JsonParsenewTicketId(itemconvertjsonnewTicketId);

                #endregion

                #region JsonData
                JObject payload = new JObject(
              new JProperty("TicketId", newTicketIdInformation), // Burası rest tarafından otomatik olarak oluşturulacak 
              new JProperty("TicketDescription", subject), // mail konusu 
              new JProperty("Priority", 2),  //Bu alan normal olarak açılacaktır
              new JProperty("TicketStartDate", ticketStartDate),
              new JProperty("TicketCompletedDate", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss")),
              new JProperty("TicketEstEndDate", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss")),
              new JProperty("IsCompleted", false),

              new JProperty("TicketState",
               new JObject(
                   new JProperty("Oid", "371F96FC-44E1-4209-A9AB-4BA067796F52") // açık durumun oidini yolla
                   )),
               new JProperty("TicketFirm",
                new JObject(
                    new JProperty("Oid", responseParseTicketContactFirmOid)
                   )),
               new JProperty("TicketContact",
                new JObject(
                    new JProperty("Oid", responseParseTicketContactOid)
                   )),
              new JProperty("Notes", notes),
              new JProperty("Tags", null),
              new JProperty("NotifyUsers", null),
              new JProperty("_CreatedDateTime", ticketStartDate),
              new JProperty("_LastModifiedDateTime", ticketStartDate),
              new JProperty("RecordCreateInfo", "System Administrator, " + RecordCreateInfoAndRecordLastUpdateInfo),  //ticket tarihi atılacak 
              new JProperty("RecordLastUpdateInfo", "System Administrator, " + RecordCreateInfoAndRecordLastUpdateInfo), //ticketStartDate tarihi
              new JProperty("IsLoading", false),
              new JProperty("IsDeleted", false),
              new JProperty("Loading", false),
              new JProperty("IsMobile", false),
              new JProperty("IsControllerSaving", false),
              new JProperty("MobileSessionUser", "00000000-0000-0000-0000-000000000000"),
              new JProperty("AvailableSubCategories", ""),// Bu ve alttaki satır değerleri [] idi
              new JProperty("AvailableTicketTypes", ""),
              new JProperty("FirmERPId", ""),
              new JProperty("IsTwitterTicket", false),
              new JProperty("IsMainCase", true)
              );
                #endregion

                #region TicketPost

                HttpClient httpClient = new HttpClient();
                var httpContent = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage responseTicket = await httpClient.PostAsync("***************************************************" + sessionId, httpContent);
                responseTicket.EnsureSuccessStatusCode();
                string responseBody = await responseTicket.Content.ReadAsStringAsync();
                var responseParse = JObject.Parse(responseBody);
                HttpWebRequest requestsupport = (HttpWebRequest)WebRequest.Create("***************************************************" + sessionId);
                requestsupport.Method = WebRequestMethods.Http.Post;
                StreamWriter swJSONPayload = new StreamWriter(requestsupport.GetRequestStream());
                swJSONPayload.Write(payload);

                #endregion

                #region SupportIdReturnMail

                dynamic jperson = JsonConvert.DeserializeObject<dynamic>(responseBody);
                var itemconvertjson = String.Empty;
                if (jperson["Items"] != null)
                {
                    var item = jperson["Items"].ToString();
                    itemconvertjson = item.Substring(1, item.Length - 1).Substring(0, item.Length - 2);

                }
                supportId = JsonParseSupportId(itemconvertjson);

                #endregion

                if (string.Compare(informationLogLevel, "True") == 0)
                    Logger.Log("#" + supportId + "#" + " id with Support record created");
            }

            return supportId;
        }

        /// <summary>
        /// Get TicketId Info
        /// </summary>
        /// <param name="data"></param>
        /// <returns>parseSupportId;</returns>
        #region JsonParse
        public static string JsonParseSupportId(string data)
        {
            string parseSupportId = "";
            if (data == "" || data == null)
                return "";
            JObject json = JObject.Parse(data);
            parseSupportId = json["TicketId"].ToString();
            return parseSupportId;
        }
        #endregion

        /// <summary>
        /// Get new TicketId Info
        /// </summary>
        /// <param name="data"></param>
        /// <returns>parseticketId</returns>
        #region JsonParse
        public static string JsonParsenewTicketId(string data)
        {
            string parsenewTicketId = "";
            if (data == "" || data == null)
                return "";
            JObject json = JObject.Parse(data);
            parsenewTicketId = json["TicketId"].ToString();
            return parsenewTicketId;
        }
        #endregion
    }
}