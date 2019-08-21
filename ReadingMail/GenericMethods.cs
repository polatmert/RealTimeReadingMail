using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReadingMail
{
    class GenericMethods
    {
        /// <summary>
        /// Get SessionId Info
        /// </summary>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        /// <returns>sessionId</returns>
        #region RestLoginMethod
        public static string RestLogin(string informationLogoUsername, string informationLogoPassword, string informationLogLevel)
        {
            try
            {
                string sessionId = "";
                string userAndPassword = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(informationLogoUsername + ":" + informationLogoPassword + ":"));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("***********************************" + userAndPassword);
                string postData = informationLogoUsername + ":" + informationLogoPassword;
                var byteData = ASCIIEncoding.ASCII.GetBytes(postData);
                request.Method = WebRequestMethods.Http.Post;
                var stream = request.GetRequestStream();
                stream.Write(byteData, 0, byteData.Length);
                stream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string source = streamReader.ReadToEnd().ToString();
                sessionId = JsonParseSession(source);
                if (string.Compare(informationLogLevel, "True") == 0)
                    Logger.Log("CRMRest Login");
                return sessionId;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message + " - CrmRest unable to login");
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Get SessionId Info
        /// </summary>
        /// <param name="data"></param>
        /// <returns>parseSession</returns>
        #region JsonParseSession
        public static string JsonParseSession(string data)
        {
            string parseSession = "";
            if (data == "" || data == null)
                return "";
            JObject json = JObject.Parse(data);
            parseSession = json["SessionId"].ToString();
            return parseSession;
        }

        #endregion

        /// <summary>
        ///  Get Oid Info
        /// </summary>
        /// <param name="data"></param>
        /// <returns>parseOid</returns>

        #region JsonParseOid
        public static string JsonParseOid(string data)
        {
            string parseOid = "";
            if (data == "" || data == null)
                return "";
            JObject json = JObject.Parse(data);
            parseOid = json["Oid"].ToString();
            return parseOid;
        }
        #endregion
    }
}