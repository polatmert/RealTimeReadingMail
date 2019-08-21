using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Xml;

namespace ReadingMail
{
    class DatabaseConnection
    {
        /// <summary>
        /// Make a database connection
        /// Get information of the desired area
        /// </summary>133+
        /// <param name="requestedData"></param>
        /// <returns></returns>
        public static List<string> ListConnection()
        {
            try
            {
                List<string> mail = new List<string>();
                string email = "";
                string emailPassword = "";
                string logoCrmUsername = "";
                string logoCrmPasswoord = "";
                string hostname = "";
                int port = 0;
                int mailDomainType = 0;
                bool logLevel = false;
                string sqlConnection = RegeditConnectionString();
                SqlConnection conn = new SqlConnection(sqlConnection);
                conn.Open();
                SqlCommand cmd = new SqlCommand("select Email,EmailPassword,CrmUsername,CrmPassword,MailDomainType,LogLevel,Hostname,Port from BO_MailEntegrasyon where GCRecord  IS NULL", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["Email"] != DBNull.Value)
                    {
                        email = reader.GetString(0);
                        mail.Add(email);
                    }
                    else
                    {
                        email = "";
                        mail.Add(email);
                    }
                    if (reader["EmailPassword"] != DBNull.Value)
                    {
                        emailPassword = reader.GetString(1);
                        mail.Add(emailPassword);
                    }
                    else
                    {
                        emailPassword = "";
                        mail.Add(emailPassword);
                    }
                    if (reader["CrmUsername"] != DBNull.Value)
                    {
                        logoCrmUsername = reader.GetString(2);
                        mail.Add(logoCrmUsername);
                    }
                    else
                    {
                        logoCrmUsername = "";
                        mail.Add(logoCrmUsername);
                    }
                    if (reader["CrmPassword"] != DBNull.Value)
                    {
                        logoCrmPasswoord = reader.GetString(3);
                        mail.Add(logoCrmPasswoord);
                    }
                    else
                    {
                        logoCrmPasswoord = "";
                        mail.Add(logoCrmPasswoord);
                    }
                    if (reader["mailDomainType"] != DBNull.Value)
                    {
                        mailDomainType = reader.GetInt32(4);

                        if (mailDomainType == 0)
                            mail.Add("Gmail");
                        else
                            mail.Add("Outlook");
                    }
                    else
                    {
                        mailDomainType = -1;
                        mail.Add(mailDomainType.ToString());
                    }
                    if (reader["LogLevel"] != DBNull.Value)
                    {
                        logLevel = reader.GetBoolean(5);
                        mail.Add(logLevel.ToString());
                    }
                    else
                    {
                        logLevel = false;
                        mail.Add(logLevel.ToString());
                    }
                    if (reader["Hostname"] != DBNull.Value)
                    {
                        hostname = reader.GetString(6);
                        mail.Add(hostname);
                    }
                    else
                    {
                        hostname = "";
                        mail.Add(hostname);
                    }
                    if (reader["Port"] != DBNull.Value)
                    {
                       
                        port = reader.GetInt32(7);
                        mail.Add(port.ToString());
                    }
                    else
                    {
                        port = 0;
                        mail.Add(port.ToString());
                    }
                }
                return mail;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.StackTrace);
                Logger.Log(ex.Message + " - Email , EmailPassword , CrmUsername ,CrmPassword , Hostname , Port  and mail type please check");
                return null;
            }
        }
        public static string RegeditConnectionString()
        {

            //LOGO_CRM_Service_16028_100450_1_57 her set değişeceği için buranın dinamik hale gelmesi gerekmektedir.
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\ControlSet001\\Services\\LOGO_CRM_Service_16028_100450_1_57");
            string fileLocation = key.GetValue("ImagePath").ToString();
            string parseFileLocation = fileLocation.Substring(0, fileLocation.Length - 21);
            string filePath = parseFileLocation + "\\Web.config";
            StreamReader sr = new StreamReader(parseFileLocation + "\\Web.config");
            String connectionData = sr.ReadToEnd();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(connectionData);
            var connectionStrings = xmlDoc.SelectSingleNode("configuration/connectionStrings/add");
            var connectionStringParse = connectionStrings.Attributes["connectionString"].Value;
            return connectionStringParse;
        }
    }
}