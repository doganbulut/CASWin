using System;
using System.Data;
using System.Data.Odbc;
using System.Configuration;
using CCSoundTrans;
using System.IO;
using CASWin.Properties;

/// <summary>
/// Summary description for AsteriskData
/// </summary>
public class AsteriskData
{
    private string destination;
    private string source;
    private string sship;
    private string sshpass;
    private string sshuser;


	public AsteriskData()
	{
        destination = Settings.Default.destin;
        source = Settings.Default.source;
        sship = Settings.Default.astserver;
        sshpass = Settings.Default.astpass;
        sshuser = Settings.Default.astuser;
	}

    public string GetSoundFileName(string User,string Number,DateTime dt)
    {
        string fileno = "";
        string filename = "";
        try
        {
            OdbcConnection odbccon = new OdbcConnection(Settings.Default.MySQLConString);
            odbccon.Open();

            DateTime dt1 = dt.AddHours(-1);
            DateTime dt2 = dt.AddHours(1);


            string sqlqry = " SELECT MAX(uniqueid) AS fileno FROM asteriskcdrdb.cdr " +
                            " WHERE (src = '" + User + "') AND (dst LIKE '%" + Number + "%') AND (disposition = 'ANSWERED') " +
                            " AND (calldate between '" + dt1.ToString("yyyy-MM-dd HH:mm:ss") + "' and " +
                            " '"+dt2.ToString("yyyy-MM-dd HH:mm:ss")+"')";

            OdbcCommand cmd = new OdbcCommand(sqlqry, odbccon);

            OdbcDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                fileno = dr[0].ToString();
            }

            dr.Close();
            sqlqry = "SELECT userfield FROM cdr  where uniqueid='"+fileno+"'";
            cmd.CommandText = sqlqry;
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                filename = dr[0].ToString();
            }

            return filename;

        }
        catch (Exception)
        {
            return "";
        }
    }

    public bool SSHTransfer(string DosyaAd)
    {
        string tmpdosya = DosyaAd;


        try
        {
            if (!Directory.Exists(destination + "/temp"))
            {
                Directory.CreateDirectory(destination + "/temp");
            }

            SSHTransfer ssht = new SSHTransfer(sshuser, sshpass, sship);
            if (!ssht.Connect())
            {
                return false;
            }

            try
            {
                if (!ssht.DownloadFile(source + "/" + tmpdosya, destination + "/temp/" + tmpdosya))
                {
                    return false;
                }
            }
            finally
            {
                ssht.DisconnectScp();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ProductCopy(string sndfilename,string user, string telno, string mustadi, string product,bool sold)
    {
        try
        {
            string yil = DateTime.Now.Year.ToString();
            string ay = DateTime.Now.Month.ToString("D2");
            string gun = DateTime.Now.Day.ToString("D2");
            string saat = DateTime.Now.Hour.ToString("D2");
            string strsold = "";
            string dir = "";

            if ((product == "")||(product == "YOK"))
            {
                return false;
            }

            if (sold)
            {
                strsold = "sold";
                dir = destination + "/" + yil + "/" + ay + "/" + strsold + "/" + product;
            }
            else
            {
                strsold = "notsold";
                dir = destination + "/" + yil + "/" + ay + "/" + strsold;
            }

            
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string kaynak = destination + "/temp/" + sndfilename;
            string hedef = dir + "/" + gun + "-" + ay + "-" + yil + "-" + saat + "-" + mustadi + "-" + telno + "-" + user + ".wav";

            File.Copy(kaynak, hedef, true);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }


}
