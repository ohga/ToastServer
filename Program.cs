using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;

namespace ToastServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 20008;

            System.Net.Sockets.TcpListener listener =
                new TcpListener(System.Net.IPAddress.Any, port);

            listener.Start();
            while (true == true)
            {
                TcpClient client = listener.AcceptTcpClient();

                NetworkStream ns = client.GetStream();
                ns.ReadTimeout = ns.WriteTimeout = 10000;
                try
                {
                    System.Text.Encoding enc = System.Text.Encoding.UTF8;
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    byte[] bytes = new byte[1024];
                    int size = 0;
                    do
                    {
                        size = ns.Read(bytes, 0, bytes.Length);
                        if (size == 0)
                            break;
                        ms.Write(bytes, 0, size);
                    } while (ns.DataAvailable || bytes[size - 1] != '\n');

                    string str = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                    ms.Close();
                    str = str.TrimEnd(new char[] { '\n', '\r' });

                    String[] arr = str.Split('\t');
                    String title, body;
                    title = "通知を受信しました";

                    if (arr.Length == 1)
                        body = arr[0];
                    else
                    {
                        body = arr[0];
                        title = arr[1];
                    }
                    if (body.Equals("_exit_")) break;

                    Debug.WriteLine("title:" + title);
                    Debug.WriteLine("body:" + body);
                    if (title.Equals("_LOCK_"))
                    {
                        Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                    }
                    else if (title.IndexOf("_EXEC_") == 0)
                    {
                        string cmd = title.Substring(6);
                        Process.Start("cmd", "/c " + cmd);
//                        cmd = "\"C:\\\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe\" http://www.google.co.jp";
//                        Process.Start("cmd", "/c " + cmd );
                    }
                    else if (title.IndexOf("_OPEN_") == 0)
                    {
                        Process.Start(title.Substring(6), body);
                    }
                    else
                    {
                        toast(title, body);
                    }
                }
                catch
                {
                    Debug.WriteLine("error..");

                }
                ns.Close();
                client.Close();

            }
            listener.Stop();
            toast("Toast Server", "終了します");
        }

        private static void toast(String title, String body)
        {
            var tmpl = ToastTemplateType.ToastImageAndText02;
            var xml = ToastNotificationManager.GetTemplateContent(tmpl);

            Debug.WriteLine(xml.GetXml());
            /* ToastImageAndText02の場合
            <toast>
                <visual>
                    <binding template="ToastImageAndText02">
                        <image id="1" src=""/>
                        <text id="1"></text>
                        <text id="2"></text>
                    </binding>
                </visual>
            </toast>
            */

            var images = xml.GetElementsByTagName("image");
            var src = images[0].Attributes.GetNamedItem("src");
            src.InnerText = "file:///" + Path.GetFullPath("img\\icon.png");

            Debug.WriteLine(src.InnerText);

            var texts = xml.GetElementsByTagName("text");
            texts[0].AppendChild(xml.CreateTextNode(title));
            texts[1].AppendChild(xml.CreateTextNode(body));
            var toast = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier("NotificationTest").Show(toast);

        }

    }
}
