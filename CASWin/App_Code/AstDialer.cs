using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using CASWin.Properties;

namespace Astautodialer
{
    public class PhoneDialer
    {

        private string serverAddr = "";
        private string mgtUser = "";
        private string mgtPass = "";

        public PhoneDialer(string mgtUsername, string mgtPassword)
        {
            serverAddr = Settings.Default.astserver;
            mgtUser = mgtUsername;
            mgtPass = mgtPassword;
        }

        public bool DialPhone(int usersExtension, string numberToDial)
        {
            try
            {

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
                {
                    IPAddress svrAddress = IPAddress.Parse(serverAddr);
                    IPEndPoint svrEndPoint = new IPEndPoint(svrAddress, 5038);
                    socket.Connect(svrEndPoint);

                    byte[] byteData = new byte[128];
                    byte[] byteResp = new byte[1024];
                    string cmdString = "Action: Login\r\n";
                    cmdString += string.Format("Username: {0}\r\n", mgtUser);
                    cmdString += string.Format("Secret: {0}\r\n", mgtPass);
                    cmdString += "Event: off\r\n\r\n";

                    socket.Send(Encoding.ASCII.GetBytes(cmdString));
                    socket.Receive(byteData);

                    cmdString = "Action: Originate\r\n";
                    cmdString += string.Format("Channel: SIP/sipone/{0}\r\n", numberToDial);
                    cmdString += string.Format("Exten: {0}\r\n", usersExtension);
                    cmdString += ("Variable: _SIPADDHEADER55=Alert-Info: Ring Answer\r\n");
                    cmdString += "Context: from-internal\r\n";
                    cmdString += string.Format("CallerId: \"Dialer\" <{0}>\r\n", numberToDial);
                    cmdString += "Priority: 1\r\n";
                    cmdString += "Timeout:10000\r\n";
                    cmdString += "Async: true\r\n\r\n";

                    socket.Send(Encoding.ASCII.GetBytes(cmdString));
                    socket.Receive(byteData);

                    socket.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }
    }
}
