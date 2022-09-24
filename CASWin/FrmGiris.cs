using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CASWin
{
    public partial class FrmGiris : Form
    {
        public FrmPreviewCall FrmPreviewCallP;

        public FrmGiris()
        {
            InitializeComponent();
        }

        protected string GetLoginUser(string Userid, string Pass)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[3];
            string LoginName = "";

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@USER_ID", SqlDbType.VarChar, Userid, false);
                dh.SetPrmArrayByPrm(prmArray, 1, "@USER_PASS", SqlDbType.VarChar, Pass, false);
                dh.SetPrmArrayByPrm(prmArray, 2, "@RESUSER_NAME", SqlDbType.VarChar, 50, LoginName, true);
                dh.GetDataTable(prmArray, "GETUSERID");
                LoginName = prmArray[2].Value.ToString();
                return LoginName;
            }
            catch (Exception)
            {
                return "";
            }

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string loginname;

            loginname = GetLoginUser(txtUser.Text.Trim(), txtPass.Text);

            if (loginname != "")
            {
                FrmPreviewCallP = new FrmPreviewCall(this,txtUser.Text,loginname,txtExten.Text);
                FrmPreviewCallP.Text = loginname;
                FrmPreviewCallP.Show();
            }  

        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            txtExten.Text = txtUser.Text;
        }
    }
}