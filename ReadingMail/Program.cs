using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Xml;
using System.Threading;
using System.Timers;

//29.01.2019 09.47
namespace ReadingMail
{
    public class Program
    {
        #region localvariables

        public static int caseSwitch = 1;
        public static List<string> information = new List<string>();

        #endregion

        /// <summary>
        /// Main Function
        /// Timer operates at the specified time.
        /// </summary>
        /// <param name="args"></param>
        #region MainMethod
        public static void Main(string[] args)
        {
            try
            {
                List<string> allMailInformation = DatabaseConnection.ListConnection();
                Logger.Log("Read all active email boxes");
                MailInformation(allMailInformation);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }

        /// <summary>
        ///  Mail type is determined.
        /// </summary>
        /// <param name="informationMailType"></param>
        /// <returns>1 or 2</returns>
        private static int Mailtype(string informationMailType)
        {
            if (string.Compare(informationMailType, "Outlook") == 0)
                return 1;
            else
                return 2;
        }

        /// <summary>
        /// Run timer.
        /// </summary>
        /// <param name="o"></param>
        private static void TimerCallback(object e)
        {
            try
            {
                // Display the date/time when this method got called.
                Logger.Log("Timer activated");
                Console.WriteLine("Mailler okunuyor " + DateTime.Now);
                List<string> gidenmailler = DatabaseConnection.ListConnection();
                string informationEmail = "";
                string informationEmailPassword = "";
                string informationLogoUsername = "";
                string informationLogoPassword = "";
                string informationMailType = "";
                string informationLogLevel = "";
                string informationHostname = "";
                int informationPort = 0;

                int mailTypeInt = 0;
                int mailI = 0;
                List<UsernameSession> logoUsernameSessions = new List<UsernameSession>();

                while (mailI <= gidenmailler.Count())
                {
                    informationEmail = gidenmailler[mailI];
                    informationEmailPassword = gidenmailler[mailI + 1];
                    informationLogoUsername = gidenmailler[mailI + 2];
                    informationLogoPassword = gidenmailler[mailI + 3];
                    informationMailType = gidenmailler[mailI + 4];
                    informationLogLevel = gidenmailler[mailI + 5];
                    informationHostname = gidenmailler[mailI + 6];
                  //  informationPort = gidenmailler[mailI + 7];
                    Int32.TryParse(gidenmailler[mailI + 7], out informationPort);
                    mailTypeInt = Mailtype(informationMailType);
                    Console.WriteLine("Mailler okunuyor " + informationEmail + " mailI : " + mailI);
                    mailI = mailI + 8;
                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log("Email : " + informationEmail + " , EmailPassword : " + informationEmailPassword + ", LogoCRMUsername : " + informationLogoUsername + ", LogoCRMPassword : " + informationLogoPassword + ", MailDomainType : " + informationMailType + " was read information from the database");
                    string sessionId = String.Empty;
                    var currentUser = logoUsernameSessions.Where(x => x.logoUsername.Equals(informationLogoUsername)).FirstOrDefault();

                    if (currentUser == null)
                    {
                        currentUser = new UsernameSession
                        {
                            logoUsername = informationLogoUsername,
                            sessionId = String.Empty,
                            sessionCreateDate = DateTime.Now,
                            isFirstMail = true
                        };
                        sessionId = GenericMethods.RestLogin(informationLogoUsername, informationLogoPassword, informationLogLevel);
                        if (string.Compare(informationLogLevel, "True") == 0)
                        {
                            if (sessionId == "" && sessionId == null)
                            {
                                Logger.Log("Session Id null");
                            }
                        }
                        currentUser.sessionId = sessionId;
                        logoUsernameSessions.RemoveAll(x => x.logoUsername.Equals(informationLogoUsername));
                        logoUsernameSessions.Add(currentUser);
                    }
                    sessionId = currentUser.sessionId;
                    SelectMail(mailTypeInt, informationEmail, informationEmailPassword, informationLogoUsername, informationLogoPassword, informationMailType, sessionId, informationLogLevel, informationHostname, informationPort);
                }
                // Force a garbage collection to occur for this demo.
                GC.Collect();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }

 

        #endregion

        /// <summary>
        /// Mail type is selected.
        /// Connection information is transferred.
        /// </summary>
        #region SelectMailFunction
        public static void SelectMail(int mailType, string informationEmail, string informationEmailPassword, string informationLogoUsername, string informationLogoPassword, string informationMailType, string sessionId, string informationLogLevel, string hostname, int port)
        {
            try
            {
                if (mailType == 1)
                {
                    Emails outlook = new Emails();
                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log("Mail type is outlook selected");
                    outlook.newGetAllUnSeenMailAsync(mailType, informationEmail, informationEmailPassword, informationLogoUsername, informationLogoPassword, informationMailType, sessionId, hostname, port);
                }
                else if (mailType == 2)
                {
                    Emails gmail = new Emails();
                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log("Mail type is gmail selected");
                    gmail.newGetAllUnSeenMailAsync(mailType, informationEmail, informationEmailPassword, informationLogoUsername, informationLogoPassword, informationMailType, sessionId, hostname, port);
                }
                else
                {
                    if (string.Compare(informationLogLevel, "True") == 0)
                        Logger.Log("Mail type not selected and therefore not read mail");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message);
            }
        }

        public static void MailInformation(List<string> gelenmailler)
        {
            System.Threading.Timer timer = new System.Threading.Timer(TimerCallback, null, 0, 180000);
            Console.ReadKey();
        }
    }
    #endregion
}