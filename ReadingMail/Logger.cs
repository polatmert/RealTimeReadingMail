using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingMail
{
    class Logger
    {
        public static void Log(string logText)
        {
            // Create a writer and open the file:
            StreamWriter log;
            if (!File.Exists("logemail.txt"))
            {
                log = new StreamWriter("logemail.txt");
            }
            else
            {
                log = File.AppendText("logemail.txt");
            }
            // Write to the file:
            log.Write(DateTime.Now);
            log.Write(" --- ");
            log.WriteLine(logText);
            // Close the stream:
            log.Close();
        }
    }
}