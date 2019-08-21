using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using S22.Imap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ReadingMail
{
    public class Emails
    {
        #region LocalVariables

        public string message_subject;
        public string message_author;
        public string tagline;
        public string message_summary;
        public static Int16 tempCounter = 0;
        public static Int16 mailCount = 0;
        public static Task<string> attachmantDocumentOid;

        #endregion

        /// <summary>
        /// Connect with the mail and log in
        /// Get unread mail
        /// </summary>
        /// <param name="mailSelection"></param>
        /// <param name="informationEmail"></param>
        /// <param name="informationEmailPassword"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        #region newGetAllUnSeenMail 
        public async void newGetAllUnSeenMailAsync(int mailSelection, string informationEmail, string informationEmailPassword, string informationLogoUsername, string informationLogoPassword, string informationLogLevel, string sessionId, string hostname, Int32 prte)
        {
            try
            {
                string usernameOutlook = informationEmail;
                string passwordOutlook = informationEmailPassword;
                string usernameGmail = informationEmail;
                string passwordGmail = informationEmailPassword;

                if (mailSelection == 1)
                {
                    if (hostname == null || hostname == "")
                    {
                        hostname = "imap-mail.outlook.com";
                    }
                    if (prte == 0)
                    {
                        prte = 933;
                    }
                    using (ImapClient Client = new ImapClient(hostname, 993, usernameOutlook, passwordOutlook, AuthMethod.Login, true))
                    {
                        IEnumerable<uint> uids = Client.Search(SearchCondition.Unseen());
                        IEnumerable<MailMessage> messages = Client.GetMessages(uids);
                        if (string.Compare(informationLogLevel, "True") == 0)
                            Logger.Log("Connected to Outlook account.");
                        await SendRepliesAsync(messages, usernameOutlook, passwordOutlook, mailSelection, informationLogoUsername, informationLogoPassword, informationLogLevel, sessionId);
                    }
                }

                if (mailSelection == 2)
                {
                    if (hostname == null || hostname == "")
                    {
                        hostname = "imap.gmail.com";
                    }
                    if (prte == 0)
                    {
                        prte = 933;
                    }
                    using (ImapClient Client = new ImapClient(hostname, 993, usernameGmail, passwordGmail, AuthMethod.Login, true))
                    {
                        IEnumerable<uint> uids = Client.Search(SearchCondition.Unseen());
                        IEnumerable<MailMessage> messages = Client.GetMessages(uids);
                        if (string.Compare(informationLogLevel, "True") == 0)
                            Logger.Log("Connected to Gmail account.");
                        await SendRepliesAsync(messages, usernameGmail, passwordGmail, mailSelection, informationLogoUsername, informationLogoPassword, informationLogLevel, sessionId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }

        #endregion       

        /// <summary>
        /// Extract documents and prepare to post
        /// Get the mail's parameters
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="mailSelection"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        /// <returns></returns>

        #region SendReplies
        public static async Task SendRepliesAsync(IEnumerable<MailMessage> messages, string user, string pass, int mailSelection, string informationLogoUsername, string informationLogoPassword, string informationLogLevel, string sessionId)
        {
            try
            {
                var filePath = "";
                Random rast = new Random();
                int attachmentKey = rast.Next(1, 100000);
                int attachmentFolderKey = rast.Next(1, 100000);

                foreach (var message in messages)
                {
                    var attach = message.Attachments;
                    var attachmentList = new List<string>();

                    foreach (var item in attach)
                    {
                        byte[] buffer = new byte[16 * 1024];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = item.ContentStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }

                            FileStream file = new FileStream(System.IO.Path.GetTempPath() + attachmentKey + "-" + item.Name, FileMode.Create, FileAccess.Write);
                            filePath = System.IO.Path.GetTempPath() + attachmentKey + "-" + item.Name;
                            ms.WriteTo(file);
                            file.Close();
                            ms.Close();
                        }
                        //Attachment
                        byte[] imagearray = System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + attachmentKey + "-" + item.Name);
                        string attachmentBase64 = Convert.ToBase64String(imagearray);
                        string attachmentOid = await AttachmentRest.AttachmentLoginAsync(attachmentBase64, attachmentKey + "-" + item.Name, filePath, attachmentKey, attachmentFolderKey, informationLogoUsername, informationLogoPassword, sessionId, informationLogLevel);
                        attachmentList.Add(attachmentOid);
                    }
                    var plainTextMessageBody = ConvertToPlainText(message.Body);
                    if (string.Compare(informationLogLevel, "True") == 0)
                    {
                        Logger.Log("Mail content edited and Unnecessary information was deleted");
                    }
                    NewSendMail(message.From.Address, message.Subject, user, pass, mailSelection, plainTextMessageBody, attachmentList, informationLogoUsername, informationLogoPassword, sessionId, informationLogLevel);
                }
                var messageCount = messages.Count();
                if (messageCount == 0)
                {
                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log("All mails read");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message + " Mail could not be read");
            }
        }

        #endregion

        /// <summary>
        /// Send mail to user
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="subject"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="mailSelection"></param>
        /// <param name="messageBody"></param>
        /// <param name="attachmantDocumentOid"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>

        #region NewSendMail
        private static async void NewSendMail(string receiver, string subject, string user, string pass, int mailSelection, string messageBody, List<string> attachmantDocumentOid, string informationLogoUsername, string informationLogoPassword, string sessionId, string informationLogLevel)
        {
            try
            {
                string firstThreeLetters = null;
                string subjectControl = "Re:"; //  Mail reply olması durumunda kontrol edilen key 
                string subjectControl2 = "RE:";//
                bool containsAny = false;
                bool containsAny2 = false;
                if (subject.Length > 3)
                {
                    firstThreeLetters = subject.Substring(0, 3);
                    containsAny = subjectControl.Contains(firstThreeLetters);
                    containsAny2 = subjectControl2.Contains(firstThreeLetters);
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(user); // Bu adresten(logosupport) mail gidecek
                mail.To.Add(receiver);//(bu adres üzerinden mail gelmiş)
                SmtpClient smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential(user, pass);
                var returnSupportId = "";
                smtp.Port = 587;

                if (mailSelection == 1)
                    smtp.Host = "smtp.live.com";
                else
                    smtp.Host = "smtp.gmail.com";

                smtp.EnableSsl = true;

                if (containsAny || containsAny2)
                {
                    mail.Subject = subject;
                    string[] subjectParse = subject.Split('#');
                    string objectOid = subjectParse[1];
                    var parsedMail = ParseBody(messageBody , mailSelection);
                    mail.Body = "Mailiniz yorum olarak destek kaydınıza eklenmiştir.";
                    Comment.CommentPostAsync(objectOid, parsedMail, informationLogoUsername, informationLogoPassword, sessionId, informationLogLevel);
                    // mail içeriğini parametrik yap                   
                    smtp.Send(mail);
                }
                else
                {
                    mail.Body = "Destek kaydınız oluşturulmuştur"; // mail içeriğini parametrik yap
                    returnSupportId = await Rest.RestAsync(receiver, subject, messageBody, informationLogoUsername, informationLogoPassword, sessionId, informationLogLevel);
                    mail.Subject = "#" + returnSupportId + "#" + " nolu Destek Kaydı";
                    smtp.Send(mail);
                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log("#" + returnSupportId + "#" + " id  " + receiver + " mail sent to the user");
                    string TicketOid = TicketOidGet(returnSupportId, informationLogoUsername, informationLogoPassword, sessionId);
                    RelatedDocumentRest.ReletadDocumentPostAsync(TicketOid, attachmantDocumentOid, informationLogoUsername, informationLogoPassword, sessionId, informationLogLevel);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// Converts HTML to plain text / strips tags.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>

        #region ConvertToPlainText
        public static string ConvertToPlainText(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        #endregion

        /// <summary>
        /// Count the words.
        /// The content has to be converted to plain text before (using ConvertToPlainText).
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        /// 

        #region CountWords
        public static int CountWords(string plainText)
        {
            return !String.IsNullOrEmpty(plainText) ? plainText.Split(' ', '\n').Length : 0;
        }
        #endregion

        #region Cut
        public static string Cut(string text, int length)
        {
            if (!String.IsNullOrEmpty(text) && text.Length > length)
            {
                text = text.Substring(0, length - 4) + " ...";
            }
            return text;
        }

        #endregion

        #region ConvertContentTo
        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            try
            {
                foreach (HtmlNode subnode in node.ChildNodes)
                {
                    ConvertTo(subnode, outText);
                }
            }

            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }

        #endregion

        #region ConvertTo
        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            try
            {
                string html;
                switch (node.NodeType)
                {
                    case HtmlNodeType.Comment:
                        // don't output comments

                        break;

                    case HtmlNodeType.Document:
                        ConvertContentTo(node, outText);

                        break;

                    case HtmlNodeType.Text:
                        // script and style must not be output
                        string parentName = node.ParentNode.Name;

                        if ((parentName == "script") || (parentName == "style"))

                            break;

                        // get text
                        html = ((HtmlTextNode)node).Text;

                        // is it in fact a special closing node output as text?
                        if (HtmlNode.IsOverlappedClosingElement(html))

                            break;

                        // check the text is meaningful and not a bunch of whitespaces
                        if (html.Trim().Length > 0)
                        {
                            outText.Write(HtmlEntity.DeEntitize(html));
                        }

                        break;

                    case HtmlNodeType.Element:

                        switch (node.Name)
                        {
                            case "p":
                                // treat paragraphs as crlf
                                outText.Write("\r\n");

                                break;

                            case "br":
                                outText.Write("\r\n");

                                break;
                        }

                        if (node.HasChildNodes)
                        {
                            ConvertContentTo(node, outText);
                        }

                        break;
                }
            }

            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// Receive Ticket oid information
        /// </summary>
        /// <param name="supportId"></param>
        /// <param name="informationLogoUsername"></param>
        /// <param name="informationLogoPassword"></param>
        /// <returns></returns>

        #region TicketOidGet

        private static string TicketOidGet(string supportId, string informationLogoUsername, string informationLogoPassword, string sessionId)
        {
            string userAndPassword = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(informationLogoUsername + ":" + informationLogoPassword + ":"));

            if (sessionId != "" && sessionId != null)
            {
                string strResponseValueTicketFirmOid = string.Empty;
                HttpWebRequest requestTicketFirmOid = (HttpWebRequest)WebRequest.Create("http://localhost/LogoCrmRest//api/v1.0/tickets?SessionId=" + sessionId + "&Filters=TicketId[EQ]=" + supportId + "&isProcessUserAddedItems=false&fields=Oid");
                requestTicketFirmOid.Method = "GET";
                HttpWebResponse responseTicketFirmOid = (HttpWebResponse)requestTicketFirmOid.GetResponse();
                Stream responseStreamTicketFirmOid = responseTicketFirmOid.GetResponseStream();
                StreamReader readerTicketFirmOid = new StreamReader(responseStreamTicketFirmOid);
                strResponseValueTicketFirmOid = readerTicketFirmOid.ReadToEnd().ToString();
                dynamic TicketFirmOid = JsonConvert.DeserializeObject<dynamic>(strResponseValueTicketFirmOid);
                var item2 = TicketFirmOid["Items"] == null ? String.Empty : TicketFirmOid["Items"].ToString();
                var itemconvertjsonTicketFirmOid = String.IsNullOrWhiteSpace(item2) ? String.Empty : item2.Substring(1, item2.Length - 1).Substring(0, item2.Length - 2);
                var Oid = GenericMethods.JsonParseOid(itemconvertjsonTicketFirmOid);
                return Oid;
            }
            else
            {
                return "";
            }
        }

        public static string ParseBody (string mailBody , int mailSelection)
        {
            if(mailSelection == 1)
            {
                string[] message = mailBody.Split(new string[] { "-----Original" }, 2, StringSplitOptions.None);
                return message[0];
            }
            else
            {

                string[] message = mailBody.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                int firtReplayedIconIndex = message.Select((s, i) => new { i, s })
                            .Where(t => t.s.Contains(">"))
                            .Select(t => t.i)
                            .FirstOrDefault();

                var bodyText = message.Select((s, i) => new { i, s })
                           .Where(t => t.i < firtReplayedIconIndex - 2)
                           .Select(t => t.s).ToArray();

               return string.Join(" ", bodyText);
            }
        }
        #endregion
    }
}