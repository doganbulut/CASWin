using System;
using System.Collections.Generic;
using System.Text;
using Tamir.SharpSsh.jsch;
using Tamir.SharpSsh;
using System.Configuration;

namespace CCSoundTrans
{
    public class SSHTransfer
    {
        Scp SSHScp;
        SshExec SSHdir;

        private string sshuser;
        private string sshpass;
        private string sship;
        

        public SSHTransfer(string username, string password, string server)
        {
            sshuser = username;
            sshpass = password;
            sship = server;

            SSHScp = new Scp(sship, sshuser, sshpass);
            SSHScp.OnTransferStart += new FileTransferEvent(SoundScp_OnTransferStart);
            SSHScp.OnTransferEnd += new FileTransferEvent(SoundScp_OnTransferEnd);
            SSHScp.OnTransferProgress += new FileTransferEvent(SoundScp_OnTransferProgress);

            SSHdir = new SshExec(sship, sshuser, sshpass);
        }

        void SoundScp_OnTransferProgress(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
           //.process 
           
        }

        void SoundScp_OnTransferEnd(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
           //end
        }

        void SoundScp_OnTransferStart(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
            //start...
        }

        public bool Connect()
        {
            try
            {
                SSHScp.Connect();
                SSHdir.Connect();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool DisconnectScp()
        {
            try
            {
                SSHScp.Close();
                SSHdir.Close();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public bool DownloadFile(string srcfile ,string dscfile)
        {
            if (!SSHScp.Connected)
                return false;
            
            try
            {
                //Copy a file from remote SSH server to local machine
                SSHScp.From(srcfile, dscfile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteFile(string srcfile)
        {
            if (!SSHdir.Connected)
                return false;

            try
            {
                string komut = "rm " + srcfile;
                //silme komutu
                SSHdir.RunCommand(komut);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //public static bool DeleteFile()
        //{
        //    try
        //    {

        //        SshExec exec = new SshExec(server, user);
        //        exec.Password = pass;

        //        Console.Write("Connecting...");
        //        exec.Connect();
        //        Console.WriteLine("OK");
        //        string command = "endhcp";
        //        string output = exec.RunCommand(command);
        //        exec.Close();

        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

    }
}
