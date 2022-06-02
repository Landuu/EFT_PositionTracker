using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EFT_Posreader
{
    internal class Logger
    {
        private TextBox Element;
        private string Time => DateTime.Now.ToString("T");

        public Logger(TextBox element)
        {
            Element = element;
        }

        public void Log(string msg)
        {
            Element.Dispatcher.BeginInvoke(() => { 
                Element.AppendText($"\n [{Time}] {msg}");
                Element.ScrollToEnd();
            });
        }

        public void LogStart()
        {
            Element.Text = $" [{Time}] Start aplikacji...";
        }
    }
}
