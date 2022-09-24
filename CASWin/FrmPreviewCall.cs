using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using Astautodialer;
using Asterisk.NET.Manager;
using Asterisk.NET.Manager.Action;
namespace CASWin
{
    public partial class FrmPreviewCall : Form
    {
        string CallStatus = "";
        string User = "";
        string Extension = "";
        string UserName = "";
        int CustRecordID = 0;

        private FrmGiris MainFrm;
        private static ManagerConnection manager;
        private string cCallerID ="";
        private string cState = "";
        private int CallWait = 15000;
        private bool AppWaitExit = false;
        private bool CeelPhoneWait = false;
        private bool CallStart = false;
        private bool OnlyWaitExit = false;

        private void AppWaitAndSave(int WaitTime)
        {
            AppWaitExit = false;
            for (int i = 0; i < WaitTime/100; i++)
            {
                WaitTime = CallWait;
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
                if (AppWaitExit)
                    return;
                lblWait.Text = i.ToString();
            }
            if (cmbStatus.SelectedItem == null)
            {
                cmbStatus.SelectedItem = "OTO_KAPAN";
                CallStatus = "OTO_KAPAN";
            }
            SaveDatabase();
        }

        private void OnlyWait(int WaitTime)
        {
            for (int i = 0; i < WaitTime / 50; i++)
            {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
                if (OnlyWaitExit)
                    return;
                lblWait.Text = i.ToString();
            }
        }


        public FrmPreviewCall(FrmGiris PfrmMain, string pUser,string pUserName,string pExtension)
        {
            User = pUser;
            Extension = pExtension;
            UserName = pUserName;
            MainFrm = PfrmMain;
            InitializeComponent();
            CallWait = Properties.Settings.Default.TmrClose;
            if (InitAsterisk())
            {
                lblAstState.Text = "Santrale Baðlandý.";
                lblAstState.BackColor = Color.LimeGreen;
            }
            else
            {
                lblAstState.Text = "Hata Santrale Baðlanmadý!";
                lblAstState.BackColor = Color.LimeGreen;
            }

        }

        private bool InitAsterisk()
        {
            try
            {
                manager = new ManagerConnection(Properties.Settings.Default.astserver, 5038, 
                    Properties.Settings.Default.astuser,Properties.Settings.Default.astpass);

                manager.OriginateResponse += new OriginateResponseEventHandler(manager_OriginateResponse);
                manager.Dial += new DialEventHandler(manager_Dial);
                manager.NewState += new NewStateEventHandler(manager_NewState);
                manager.Hangup += new HangupEventHandler(manager_Hangup);
                manager.Login();
                return true;
            }
            catch
            {
                return false;
            } 
        }

        void manager_Hangup(object sender, Asterisk.NET.Manager.Event.HangupEvent e)
        {
            try
            {
                bool err = true;
                if (e.CallerIdNum != null)
                    if ((e.CallerIdNum == txtPhone.Text)||(e.CallerIdNum == txtNewPhone.Text))
                    {
                        CallStart = false;
                        updateTelBagText(" Görüþme Bitti.",
                        Color.Red);
                        if (e.Cause == 16)
                        {
                            lblHangup.Text = "Normal Kapanma...";
                            lblHangup.BackColor = Color.LightBlue;
                            
                        }
                        else if (e.Cause == 17)
                        {
                            lblHangup.Text = "Meþgul Verdi...";
                            lblHangup.BackColor = Color.Magenta;
                            CallWait = 4000;
                        }
                        else if (e.Cause == 19)
                        {
                            lblHangup.Text = "Cevap Yok...";
                            lblHangup.BackColor = Color.Fuchsia;
                            CallWait = 4000;
                        }
                        else if (e.Cause == 1)
                        {
                            lblHangup.Text = "Ulaþýlamadý...";
                            lblHangup.BackColor = Color.Fuchsia;
                            CallWait = 4000;
                        }
                        else {

                            lblHangup.Text = "Belirsiz...";
                            lblHangup.BackColor = Color.Fuchsia;
                            CallWait = 4000;
                        }
                        
                        updateAgentCommentText("Çaðrý Sonlandý...",
                        Color.LightGreen);
                        
                        CeelPhoneWait = true;
                        if (!chkRunWork.Checked)
                                AppWaitAndSave(CallWait);
                    }
            }
            catch
            {
                
            
            }
        }

        void manager_NewState(object sender, Asterisk.NET.Manager.Event.NewStateEvent e)
        {
            bool callidok = false;
            bool agentidok = false;

            try
            {
                if (e.CallerIdNum != null)
                    if ((e.CallerIdNum == txtPhone.Text) || (e.CallerIdNum == txtNewPhone.Text))
                        callidok = true;
                    else if (e.CallerIdNum == Extension)
                        agentidok = true;


                if ((!callidok) && ((e.CallerIdNum == txtPhone.Text) || (e.CallerIdNum == txtNewPhone.Text)))
                    callidok = true;
                else if ((!agentidok) && (e.CallerIdNum == Extension))
                    agentidok = true;

                //if (agentidok)
                //{
                //    if (e.ChannelState == "5")
                //    {
                //        updateAgentCommentText("Çaðrýyý Kabul Edin...",
                //        Color.Red);
                //    }
                //    else if (e.ChannelState == "6")
                //    {
                //        updateTelBagText("Açtýnýz...", Color.Lime); ;
                //    }
                //    else if (e.ChannelState == "7")
                //    {
                //        updateTelBagText("Meþgul verdiniz..",Color.Red);
                //    }
                //}

                if (callidok)
                {
                    cCallerID = txtPhone.Text;
                    if (e.ChannelState == "5")
                    {
                        updateTelBagText(e.CallerIdNum + " Çalýyor...",
                        Color.Yellow);
                    }
                    else if (e.ChannelState == "6")
                    {
                        updateAgentCommentText("Çaðrý Baðlandý...",
                        Color.LimeGreen);
                        updateTelBagText(cCallerID + " Açýldý...",
                        Color.LimeGreen);
                    }
                    else if (e.ChannelState == "7")
                    {
                        updateAgentCommentText("",Color.Red);
                        updateTelBagText(cCallerID + " Meþgul...",
                        Color.Pink);
                        
                    }
                }
            }
            catch
            {
               
            }  
        }

        void manager_Dial(object sender, Asterisk.NET.Manager.Event.DialEvent e)
        {
            try
            {
               
            }
            catch
            {
                
               
            }
        }

        void manager_OriginateResponse(object sender, Asterisk.NET.Manager.Event.OriginateResponseEvent e)
        {
            bool orgCall = false;
            string orgExten = "";

            try
            {
                if (e.CallerIdNum != null)
                {
                    //string tmpch = e.Channel.Substring(e.Channel.IndexOf("SIP/") + 4, 3);
                    if ((e.CallerIdNum == txtPhone.Text)||(e.CallerIdNum == txtNewPhone.Text))
                    {
                        if (e.Response == "Failure")
                        {
                            updateTelBagText( orgExten + "Hata Baðlantý Yapýlamadý...",Color.Red);       
                            cCallerID = "";
                            CallWait = 4000;
                            //AppWaitAndSave(5000);
                        }
                        else if (e.Response == "Success")
                        {
                            if (e.Exten != null)
                            {
                                CallStart = true;
                                orgExten = e.Exten;
                                cCallerID = e.CallerIdNum;
                                CallWait = 10000;
                            }
                            updateTelBagText(cCallerID + " baðlandý...", Color.Lime);
                            updateAgentCommentText("Görüþme baþladý...",
                            Color.Lime);
                            AppWaitExit = true;
                        }

                    }
                }
            }
            catch
            {
                updateTelBagText("Originate Eroor...",Color.Red);
            }

        }

        protected SqlDataReader GetCampain()
        {
            DataHelper dh = new DataHelper();
            try
            {
                return dh.GetDataReader("GETCAMPAINS");
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected SqlDataReader GetCampainProduct(string Campain)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[1];

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@CAMPAIN_ID", SqlDbType.VarChar, Campain, false);
                return dh.GetDataReader(prmArray, "GETPRODUCTS");
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected SqlDataReader GetCampainControl(string Campain, string ControlName)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[2];

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@CAMPAIN_ID", SqlDbType.VarChar, Campain, false);
                dh.SetPrmArrayByPrm(prmArray, 1, "@CONTROL", SqlDbType.VarChar, ControlName, false);
                return dh.GetDataReader(prmArray, "GETCONTROLS");
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected SqlDataReader GetCampainSelection(string Campain, string ControlName)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[2];

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@CAMPAIN_ID", SqlDbType.VarChar, Campain, false);
                dh.SetPrmArrayByPrm(prmArray, 1, "@CONTROL", SqlDbType.VarChar, ControlName, false);
                return dh.GetDataReader(prmArray, "GETSELECTION");
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected string GetCampainControls(string Campain, string Control)
        {
            try
            {
                SqlDataReader dr;

                dr = GetCampainControl(cmdCampains.SelectedItem.ToString(), Control);
                if (dr == null)
                    return "";

                while (dr.Read())
                {
                    return dr.GetString(0);
                }
                return dr.GetString(0);
            }
            catch (Exception)
            {
                return "";
            }
        }

        protected string GetCampainTTProducts(string Campain, string Control)
        {
            try
            {
                SqlDataReader dr;

                dr = GetCampainControl(cmdCampains.SelectedItem.ToString(), Control);

                if (dr.Read())
                {
                    if (dr.IsDBNull(2))
                        return "YOK";
                    else
                        return dr.GetString(2);
                }
                else
                    return "YOK";
            }
            catch (Exception)
            {
                return "";
            }
        }



        protected bool GetCampainControlsNull(string Campain, string Control)
        {
            try
            {
                SqlDataReader dr;

                dr = GetCampainControl(cmdCampains.SelectedItem.ToString(), Control);
                if (dr == null)
                    return false;

                while (dr.Read())
                {
                    return dr.GetBoolean(1);
                }
                return dr.GetBoolean(1);
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected void GetCampainSelections(string Campain, string Control, ComboBox cmb)
        {
            try
            {
                SqlDataReader dr;

                cmb.Items.Clear();

                dr = GetCampainSelection(cmdCampains.SelectedItem.ToString(), Control);
                if (dr == null)
                    return;

                while (dr.Read())
                {
                    cmb.Items.Add(dr.GetString(0));
                }
                return;
            }
            catch (Exception)
            {
                return;
            }
        }


        protected bool LoadCampain()
        {
            try
            {
                SqlDataReader dr;

                dr = GetCampain();
                if (dr == null)
                    return false;
                while (dr.Read())
                {
                    cmdCampains.Items.Add(dr.GetString(0));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        protected void LoadProduct(string Campain)
        {
            cmbProduct.Items.Clear();
            try
            {
                SqlDataReader dr;

                dr = GetCampainProduct(cmdCampains.SelectedItem.ToString());
                if (dr == null)
                    return;
                while (dr.Read())
                {
                    cmbProduct.Items.Add(dr.GetString(1));
                }
            }
            catch (Exception)
            {

                return;
            }
        }

        protected void LoadControls(string Campain)
        {
            try
            {
                lblINFO1.Text = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO1") + ":" +
                    GetCampainControls(cmdCampains.SelectedItem.ToString(), "INFO1");
                lblINFO2.Text = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO2") + ":" +
                    GetCampainControls(cmdCampains.SelectedItem.ToString(), "INFO2");
                lblINFO3.Text = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO3") + ":" +
                    GetCampainControls(cmdCampains.SelectedItem.ToString(), "INFO3");
                lblINFO4.Text = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO4") + ":" +
                    GetCampainControls(cmdCampains.SelectedItem.ToString(), "INFO4");
                lblINFO5.Text = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO5") + ":" +
                    GetCampainControls(cmdCampains.SelectedItem.ToString(), "INFO5");
                lblINFO6.Text = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO6") + ":" +
                    GetCampainControls(cmdCampains.SelectedItem.ToString(), "INFO6");

            }
            catch (Exception)
            {
                return;
            }
        }


        protected void LoadSelections(string Campain)
        {
            try
            {
                GetCampainSelections(cmdCampains.SelectedItem.ToString(), "INFO1", txtInfo1);
                GetCampainSelections(cmdCampains.SelectedItem.ToString(), "INFO2", txtInfo2);
                GetCampainSelections(cmdCampains.SelectedItem.ToString(), "INFO3", txtInfo3);
                GetCampainSelections(cmdCampains.SelectedItem.ToString(), "INFO4", txtInfo4);
                GetCampainSelections(cmdCampains.SelectedItem.ToString(), "INFO5", txtInfo5);
                GetCampainSelections(cmdCampains.SelectedItem.ToString(), "INFO6", txtInfo6);
            }
            catch (Exception)
            {
                return;
            }
        }

        protected int GetOutBoundCall(string userid, string campain, out string Phone)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[4];
            int recid = 0;
            string recstr = "";
            string tel = "";

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@CAMPAIN_ID", SqlDbType.VarChar, campain, false);
                dh.SetPrmArrayByPrm(prmArray, 1, "@USER_ID", SqlDbType.Int, userid, false);
                dh.SetPrmArrayByPrm(prmArray, 2, "@TEL", SqlDbType.VarChar, 20, recstr, true);
                dh.SetPrmArrayByPrm(prmArray, 3, "@RECORD_ID", SqlDbType.VarChar, 10, recstr, true);
                dh.GetDataTable(prmArray, "GETRECORD");
                tel = prmArray[2].Value.ToString();
                recstr = prmArray[3].Value.ToString();
                recid = Convert.ToInt32(recstr);
                Phone = tel;
                return recid;

            }
            catch (Exception)
            {
                Phone = "";
                return 0;
            }
        }

        protected int GetOutBoundPhone(string userid, string campain, string Tel)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[4];
            int recid = 0;
            string recstr = "";

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@CAMPAIN_ID", SqlDbType.VarChar, campain, false);
                dh.SetPrmArrayByPrm(prmArray, 1, "@USER_ID", SqlDbType.Int, userid, false);
                dh.SetPrmArrayByPrm(prmArray, 2, "@TEL", SqlDbType.VarChar, Tel, false);
                dh.SetPrmArrayByPrm(prmArray, 3, "@RECORD_ID", SqlDbType.VarChar, 10, recstr, true);
                dh.GetDataTable(prmArray, "GETRECORDTEL");
                recstr = prmArray[3].Value.ToString();
                recid = Convert.ToInt32(recstr);
                return recid;

            }
            catch (Exception)
            {
                return 0;
            }
        }



        protected void ClearRecord()
        {
            txtName.Text = "";
            //txtPhone.Text = "";
            txtAddress1.Text = "";
            txtAddress2.Text = "";
            txtAddress3.Text = "";
            txtCountry.Text = "";
            txtCity.Text = "";
            txtComments.Text = "";
            lblCallCount.Text = "";
            lblEntryDate.Text = "";
            lblNextCall.Text = "";
            lblLastCall.Text = "";
            lblErr.Text = "";
            lblSound.Text = "";
            chkPhone.Checked = false;
            txtNewPhone.Text = "";

            if (cmbProduct.Items.Count != 0)
                cmbProduct.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            CallStatus = "";
            cmbCallbackDay.DateTime = DateTime.Today;
            cmbCallbackHour.Text = "00:00";


            if (txtInfo1.Items.Count != 0)
            {
                txtInfo1.SelectedIndex = 0;
            }
            if (txtInfo2.Items.Count != 0)
            {
                txtInfo2.SelectedIndex = 0;
            }
            if (txtInfo3.Items.Count != 0)
            {
                txtInfo3.SelectedIndex = 0;
            }
            if (txtInfo4.Items.Count != 0)
            {
                txtInfo4.SelectedIndex = 0;
            }
            if (txtInfo5.Items.Count != 0)
            {
                txtInfo5.SelectedIndex = 0;
            }
            if (txtInfo6.Items.Count != 0)
            {
                txtInfo6.SelectedIndex = 0;
            }
        }

        private void ConrolInfo()
        {
            if ((txtInfo1.Items.Count != 0) && (txtInfo1.SelectedIndex == -1))
            {
                txtInfo1.SelectedIndex = 0;
            }
            if ((txtInfo2.Items.Count != 0) && (txtInfo2.SelectedIndex == -1))
            {
                txtInfo2.SelectedIndex = 0;
            }
            if ((txtInfo3.Items.Count != 0) && (txtInfo3.SelectedIndex == -1))
            {
                txtInfo3.SelectedIndex = 0;
            }
            if ((txtInfo4.Items.Count != 0) && (txtInfo4.SelectedIndex == -1))
            {
                txtInfo4.SelectedIndex = 0;
            }
            if ((txtInfo5.Items.Count != 0) && (txtInfo5.SelectedIndex == -1))
            {
                txtInfo5.SelectedIndex = 0;
            }
            if ((txtInfo6.Items.Count != 0) && (txtInfo6.SelectedIndex == -1))
            {
                txtInfo6.SelectedIndex = 0;
            }
        }

        protected bool GetRecordDetail(int id)
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[1];
            SqlDataReader dr;
            string pid = id.ToString();

            try
            {


                dh.SetPrmArrayByPrm(prmArray, 0, "@ID", SqlDbType.Int, pid, false);
                dr = dh.GetDataReader(prmArray, "GETRECORDDETAIL");

                if (dr != null)
                {
                    dr.Read();
                    txtName.Text = dr["NAME"].ToString();
                    txtPhone.Text = dr["PHONE"].ToString();
                    txtAddress1.Text = dr["ADDRESS1"].ToString();
                    txtAddress2.Text = dr["ADDRESS2"].ToString();
                    txtAddress3.Text = dr["ADDRESS3"].ToString();
                    txtCountry.Text = dr["COUNTRY"].ToString();
                    txtCity.Text = dr["CITY"].ToString();
                    txtComments.Text = dr["COMMENTS"].ToString();
                    lblCallCount.Text = dr["CALL_COUNT"].ToString();

                    DateTime dt1 = new DateTime();
                    dt1 = (DateTime)dr["EXPORT_DATE"];
                    lblEntryDate.Text = dt1.ToString("HH:mm:ss dd.MM.yyyy");

                    DateTime dt2 = new DateTime();
                    dt2 = (DateTime)dr["CALLBACK_DATE"];
                    lblNextCall.Text = dt2.ToString("HH:mm:ss dd.MM.yyyy");

                    DateTime dt3 = new DateTime();
                    dt3 = (DateTime)dr["LAST_CALL"];
                    lblLastCall.Text = dt3.ToString("HH:mm:ss dd.MM.yyyy");

                    cmbProduct.SelectedIndex = Convert.ToInt32(dr["PRODUCT"].ToString());
                    cmbStatus.SelectedIndex =
                        cmbStatus.Items.IndexOf(dr["STATUS"].ToString());
                    CallStatus = dr["CALL_STATUS"].ToString();

                    txtInfo1.SelectedIndex =
                        txtInfo1.Items.IndexOf(dr["INFO1"].ToString());
                    txtInfo2.SelectedIndex =
                        txtInfo2.Items.IndexOf(dr["INFO2"].ToString());
                    txtInfo3.SelectedIndex =
                        txtInfo3.Items.IndexOf(dr["INFO3"].ToString());
                    txtInfo4.SelectedIndex =
                        txtInfo4.Items.IndexOf(dr["INFO4"].ToString());
                    txtInfo5.SelectedIndex =
                        txtInfo5.Items.IndexOf(dr["INFO5"].ToString());
                    txtInfo6.SelectedIndex =
                        txtInfo6.Items.IndexOf(dr["INFO6"].ToString());
                    ConrolInfo();
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        protected bool InsertRecord()
        {
            return true;
        }

        protected bool SaveRecordDetail()
        {
            DataHelper dh = new DataHelper();
            SqlParameter[] prmArray = new SqlParameter[16];

            if (chkPhone.Checked)
            {
                txtComments.Text = "TELNO:" + txtNewPhone.Text + ":" + txtComments.Text;
            }

            try
            {
                dh.SetPrmArrayByPrm(prmArray, 0, "@ID", SqlDbType.Int, txtID.Text, false);
                dh.SetPrmArrayByPrm(prmArray, 1, "@CALL_STATUS", SqlDbType.VarChar, CallStatus, false);
                dh.SetPrmArrayByPrm(prmArray, 2, "@STATUS", SqlDbType.VarChar, cmbStatus.SelectedItem.ToString(), false);
                dh.SetPrmArrayByPrm(prmArray, 3, "@ADDRESS2", SqlDbType.VarChar, txtAddress2.Text, false);
                dh.SetPrmArrayByPrm(prmArray, 4, "@ADDRESS3", SqlDbType.VarChar, txtAddress3.Text, false);
                dh.SetPrmArrayByPrm(prmArray, 5, "@COUNTRY", SqlDbType.VarChar, txtCountry.Text, false);
                dh.SetPrmArrayByPrm(prmArray, 6, "@CITY", SqlDbType.VarChar, txtCity.Text, false);
                if (cmbProduct.SelectedItem== null)
                    dh.SetPrmArrayByPrm(prmArray, 7, "@PRODUCT", SqlDbType.VarChar, "0", false);
                else
                    dh.SetPrmArrayByPrm(prmArray, 7, "@PRODUCT", SqlDbType.VarChar, cmbProduct.SelectedItem.ToString(), false);
                if (CallStatus == "TEKRAR_ARA")
                {
                    DateTime scdt = new DateTime();
                    scdt = DateTime.Now;


                    scdt = cmbCallbackDay.DateTime.Date + cmbCallbackHour.Time.TimeOfDay;
                    dh.SetPrmArrayByPrm(prmArray, 8, "@CALLBACK_DATE", SqlDbType.DateTime, scdt.ToString(), false);
                }
                else
                {
                    dh.SetPrmArrayByPrm(prmArray, 8, "@CALLBACK_DATE", SqlDbType.DateTime, "00:00:00 01.01.1900", false);
                }
                string inf1 = "";
                if (txtInfo1.SelectedItem != null)
                    inf1 = txtInfo1.SelectedItem.ToString();
                string inf2 = "";
                if (txtInfo2.SelectedItem != null)
                    inf2 = txtInfo2.SelectedItem.ToString();
                string inf3 = "";
                if (txtInfo3.SelectedItem != null)
                    inf3 = txtInfo3.SelectedItem.ToString();
                string inf4 = "";
                if (txtInfo4.SelectedItem != null)
                    inf4 = txtInfo4.SelectedItem.ToString();
                string inf5 = "";
                if (txtInfo5.SelectedItem != null)
                    inf5 = txtInfo5.SelectedItem.ToString();
                string inf6 = "";
                if (txtInfo6.SelectedItem != null)
                    inf6 = txtInfo6.SelectedItem.ToString();

                dh.SetPrmArrayByPrm(prmArray, 9, "@INFO1", SqlDbType.VarChar, inf1, false);
                dh.SetPrmArrayByPrm(prmArray, 10, "@INFO2", SqlDbType.VarChar, inf2, false);
                dh.SetPrmArrayByPrm(prmArray, 11, "@INFO3", SqlDbType.VarChar, inf3, false);
                dh.SetPrmArrayByPrm(prmArray, 12, "@INFO4", SqlDbType.VarChar, inf4, false);
                dh.SetPrmArrayByPrm(prmArray, 13, "@INFO5", SqlDbType.VarChar, inf5, false);
                dh.SetPrmArrayByPrm(prmArray, 14, "@INFO6", SqlDbType.VarChar, inf6, false);
                dh.SetPrmArrayByPrm(prmArray, 15, "@COMMENTS", SqlDbType.VarChar, txtComments.Text, false);
                dh.ExecuteCommand("UPDATERECORDDETAIL", prmArray);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        protected string AgentState(string Exten)
        {
            try
            {
                Asterisk.NET.Manager.Action.ExtensionStateAction action = new ExtensionStateAction();
                action.ActionId = "1";
                action.Exten = Exten;
                action.Context = "from-internal";

                Asterisk.NET.Manager.Response.ManagerResponse mr =
                    manager.SendAction(action, 15000);
                if (mr != null)
                {
                    Asterisk.NET.Manager.Response.ExtensionStateResponse es = (Asterisk.NET.Manager.Response.ExtensionStateResponse)mr;
                    return es.Status.ToString();
                }else
                    return "";
            }
            catch
            {
                return "";
            }
 
        }

        protected bool SaveSoundDetail(bool sold)
        {
            try
            {
                string telefon = "";
                if (chkPhone.Checked)
                    telefon = txtNewPhone.Text;
                else
                    telefon = txtPhone.Text;

                string sfile = ControlSound(telefon);
                if (sfile == "")
                {
                    lblSound.ForeColor = System.Drawing.Color.Red;
                    lblSound.Text = "Uyarý ses dosyasý bulunamadý !!!";
                    return false;
                }
                else
                {
                    sfile = sfile.Remove(0, 6);
                    lblSound.Text = sfile;

                    if (CopySound(sfile))
                    {
                        if (!ControlTTProducts(telefon, sfile, sold))
                            lblErr.Text = "Ses dosyasý TT Ürün eþleþtirme hatasý ! ";
                        else
                            DeleteTempSound(sfile);

                        lblSound.ForeColor = System.Drawing.Color.Green;
                        lblSound.Text = "Ses Kayýt Entegrasyonu Baþarýlý";
                        return true;
                    }
                    else
                    {
                        lblSound.ForeColor = System.Drawing.Color.Red;
                        lblSound.Text = "Ses dosyasý Asterisk serverdan kopyalanamadý!:Dosya" + sfile;
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected bool ControlData()
        {
            lblErr.Text = "";
            try
            {
                //if (cmbStatus.SelectedItem.ToString() == "SECINIZ")
                //{
                //    lblErr.Text = "ÇAÐRI SONUCU SEÇMELÝSÝNÝZ !";
                //    return TRUE;
                //}

                if ((cmbStatus.SelectedItem.ToString() == "SATILDI") ||
                    (cmbStatus.SelectedItem.ToString() == "ILGILENMIYOR") ||
                    (cmbStatus.SelectedItem.ToString() == "YANLIS_NUMARA") ||
                     (cmbStatus.SelectedItem.ToString() == "BIR_DAHA_ARAMA"))
                    CallStatus = "ARANDI";

                if ((cmbStatus.SelectedItem.ToString() == "CEVAP_YOK") ||
                    (cmbStatus.SelectedItem.ToString() == "TEKRAR_ARA"))
                    CallStatus = "TEKRAR_ARA";

                if (CallStatus == "TEKRAR_ARA")
                    if ((cmbCallbackDay.DateTime.Date == null) && (cmbCallbackHour.Text == null))
                    {
                        lblErr.Text = "TEKRAR ARAMA ÝÇÝN TARÝH SEÇÝNÝZ !";
                        return false;
                    }


                if ((cmbStatus.SelectedItem.ToString() == "SATILDI"))
                {
                    if ((cmbProduct.SelectedItem.ToString() == "0"))
                    {
                        lblErr.Text = "Bir internet paketi veya telefon hizmeti seçiniz !";
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected bool ControlSelection()
        {
            bool result = true;
            try
            {
                if (cmbStatus.SelectedItem.ToString() == "SATILDI")
                {
                    if (GetCampainControlsNull(cmdCampains.SelectedItem.ToString(), "INFO1"))
                        if (txtInfo1.SelectedItem.ToString() == "0")
                        {
                            lblErr.Text = "Boþ geçilemez: " + lblINFO1.Text;
                            result = false;
                        }
                    if (GetCampainControlsNull(cmdCampains.SelectedItem.ToString(), "INFO2"))
                        if (txtInfo2.SelectedItem.ToString() == "0")
                        {
                            lblErr.Text = "Boþ geçilemez: " + lblINFO2.Text;
                            result = false;
                        }
                    if (GetCampainControlsNull(cmdCampains.SelectedItem.ToString(), "INFO3"))
                        if (txtInfo3.SelectedItem.ToString() == "0")
                        {
                            lblErr.Text = "Boþ geçilemez: " + lblINFO3.Text;
                            result = false;
                        }
                    if (GetCampainControlsNull(cmdCampains.SelectedItem.ToString(), "INFO4"))
                        if (txtInfo4.SelectedItem.ToString() == "0")
                        {
                            lblErr.Text = "Boþ geçilemez: " + lblINFO4.Text;
                            result = false;
                        }
                    if (GetCampainControlsNull(cmdCampains.SelectedItem.ToString(), "INFO5"))
                        if (txtInfo5.SelectedValue == "0")
                        {
                            lblErr.Text = "Boþ geçilemez: " + lblINFO5.Text;
                            result = false;
                        }
                    if (GetCampainControlsNull(cmdCampains.SelectedItem.ToString(), "INFO6"))
                        if (txtInfo6.SelectedValue == "0")
                        {
                            lblErr.Text = "Boþ geçilemez: " + lblINFO6.Text;
                            result = false;
                        }
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected bool ControlTTProducts(string tel, string filename, bool solded)
        {
            string sndfile = filename;
            string urun;

            try
            {
                AsteriskData astdt = new AsteriskData();

                if (!solded)//ilgilenmiyorsa kaydet çýk...
                {
                    urun = "notsold";
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                    return true;
                }


                if ((txtCountry.Text != "") && (txtCountry.Text.IndexOf("@") != -1))
                {
                    urun = "EFATURA";
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }

                if (txtInfo1.SelectedIndex > 0)
                {
                    urun = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO1");
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }

                if (txtInfo2.SelectedIndex > 0)
                {
                    urun = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO2");
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }
                if (txtInfo3.SelectedIndex > 0)
                {
                    urun = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO3");
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }
                if (txtInfo4.SelectedIndex > 0)
                {
                    urun = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO4");
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }
                if (txtInfo5.SelectedIndex > 0)
                {
                    urun = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO5");
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }
                if (txtInfo6.SelectedIndex > 0)
                {
                    urun = GetCampainTTProducts(cmdCampains.SelectedItem.ToString(), "INFO6");
                    astdt.ProductCopy(sndfile, Extension, tel, txtName.Text, urun, solded);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected string ControlSound(string telno)
        {
            try
            {
                DateTime dt = new DateTime();
                dt = DateTime.Now;

                string tel = telno;

                if (tel.Substring(0, 1) == "0")
                    tel = tel.Remove(0, 1);


                AsteriskData astdata = new AsteriskData();
                return astdata.GetSoundFileName(Extension, tel, dt);
            }
            catch (Exception)
            {
                return "";
            }
        }

        protected bool CopySound(string filename)
        {
            try
            {
                string sndfile = filename;

                if (sndfile == "")
                {
                    lblSound.Text = "Ses Kayýt dosyasý bulunamadý! ";
                    return false;
                }

                AsteriskData astdata = new AsteriskData();
                return astdata.SSHTransfer(sndfile);
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected bool DeleteTempSound(string filename)
        {
            try
            {
                if (filename == "")
                {
                    lblSound.Text = "Ses Kayýt dosyasý bulunamadý! ";
                    return false;
                }

                File.Delete(Properties.Settings.Default.destin + "/temp/" + filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        private void FrmPreviewCall_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
                this.Text = "Agent : " + UserName;
            
                LoadCampain();
                cmdCampains.SelectedIndex = 0;
                LoadProduct(cmdCampains.SelectedItem.ToString());
                LoadControls(cmdCampains.SelectedItem.ToString());
                LoadSelections(cmdCampains.SelectedItem.ToString());

                btnNewRecord.Enabled = true;
                btnSave.Enabled = false;
           
        }

        private void dialrecord()
        {
            String sshpass = Properties.Settings.Default.astpass;
            String sshuser = Properties.Settings.Default.astuser;
            string tel = txtPhone.Text;
            if (chkPhone.Checked)
                tel = txtNewPhone.Text;

            if (tel.Trim().Length == 10)
            {
                tel = "0" + tel;
            }

            PhoneDialer autodialer = new PhoneDialer(sshuser, sshpass);
            if (!autodialer.DialPhone(Convert.ToInt32(Extension), tel))
            {
                lblErr.Text = "Hata otomatik arama baþarýsýz!";
            }
        }

        private void NewRecord()
        {
            cCallerID = "";
            cState = "";

            ClearRecord();
            if (!chkRunWork.Checked)
                txtID.Text = GetOutBoundPhone(User, cmdCampains.SelectedItem.ToString(), txtPhone.Text).ToString();
            else
                txtID.Text = GetOutBoundPhone(User, cmdCampains.SelectedItem.ToString(),txtPhone.Text).ToString();

            if (txtID.Text.Trim() != "0")
            {
                GetRecordDetail(Convert.ToInt32(txtID.Text));
                CustRecordID = Convert.ToInt32(txtID.Text);
                btnNewRecord.Enabled = false;
                btnSave.Enabled = true;
                dialrecord();
            }
            else
            {
                if (!chkRunWork.Checked)
                {
                    CustRecordID = 0;
                    lblErr.Text = "Kayýt bulunamadý !";
                    OnlyWait(3000);
                    if (!CallStart)
                    {
                        if (AgentState(Extension) == "1")
                        {
                            MessageBox.Show("Bilgi...Aktif çaðrýnýz var...");
                        }
                        else
                        {
                            OnlyWait(3000);
                            NewRecord();
                        }
                    }
                }
            }
        }

        private void btnNewRecord_Click(object sender, EventArgs e)
        {
            if (!CallStart)
            {
                if (AgentState(Extension) == "1")
                {
                    MessageBox.Show("Bilgi...Aktif çaðrýnýz var...");
                }
                else
                {
                    NewRecord();
                }
            }
        }

        private void SaveDatabase()
        {
            //if (!ControlData())
            //    return;
            //else
            //{
            //    if (!ControlSelection())
            //        return;
            //}

            if (SaveRecordDetail())
            {
                if (cmbStatus.SelectedItem.ToString() == "SATILDI")
                {
                    if (!SaveSoundDetail(true))
                        lblErr.Text = "Uyarý ses dosya kayýt hatasý";
                }
                else if (cmbStatus.SelectedItem.ToString() == "ILGILENMIYOR")
                {
                    if (!SaveSoundDetail(false))
                        lblErr.Text = "Uyarý ses dosya kayýt hatasý";
                }
                updateAgentCommentText("true SaveDatabase",Color.Blue);
                if (!CallStart)
                {
                    btnNewRecord.Enabled = true;
                    btnSave.Enabled = false;
                    if (AgentState(Extension) == "1")
                    {
                        MessageBox.Show("Bilgi...Aktif çaðrýnýz var...");
                    }
                    else
                    {
                        if(!chkRunWork.Checked)
                            NewRecord(); 
                    }
                       
                }
            }
            else
            {
                lblErr.Text = "Hata Form Kaydedilemedi ! !";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            AppWaitExit = true;
            if (lblAgentComment.Text == "Çaðrý Baðlandý...")
                if (!chkRunWork.Checked)
                    OnlyWait(5000);

            if ((cmbStatus.SelectedItem == null) || 
                (cmbStatus.SelectedItem.ToString() == "SECINIZ")||
                (cmbStatus.SelectedItem.ToString() == "SEÇÝNÝZ"))
            {
                cmbStatus.SelectedItem = "OTO_KAPAN";
                CallStatus = "OTO_KAPAN";
            }
            SaveDatabase();
        }

        private void cmdCampains_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProduct(cmdCampains.SelectedItem.ToString());
            LoadControls(cmdCampains.SelectedItem.ToString());
            LoadSelections(cmdCampains.SelectedItem.ToString());
        }

        private void btnOtoAra_Click(object sender, EventArgs e)
        {
            AppWaitExit = true;
            OnlyWaitExit = true;
            if (!CallStart)
                dialrecord(); 
         
        }

        private void FrmPreviewCall_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                AppWaitExit = true;
                MainFrm.FrmPreviewCallP.Dispose();
                MainFrm.FrmPreviewCallP = null;
                manager.Logoff();
                
            }
            catch
            {
                
            }
        }

        private void txtAddress2_TextChanged(object sender, EventArgs e)
        {

        }

        private void timerClose_Tick(object sender, EventArgs e)
        {
           
            
        }

       


        delegate void updateTelBagDelegate(string newText, Color newColor);
        private void updateTelBagText(string newText,Color newColor)
        {
            if (lblTelBag.InvokeRequired)
            {
                // this is worker thread
                updateTelBagDelegate del = new updateTelBagDelegate(updateTelBagText);
                lblTelBag.Invoke(del, new object[] { newText,newColor });
            }
            else
            {
                // this is UI thread
                lblTelBag.Text = newText;
                lblTelBag.BackColor = newColor;
            }
        }

        delegate void updateAgentCommentDelegate(string newText, Color newColor);
        private void updateAgentCommentText(string newText, Color newColor)
        {
            if (lblAgentComment.InvokeRequired)
            {
                // this is worker thread
                updateAgentCommentDelegate del = new updateAgentCommentDelegate(updateAgentCommentText);
                lblAgentComment.Invoke(del, new object[] { newText, newColor });
            }
            else
            {
                // this is UI thread
                lblAgentComment.Text = newText;
                lblAgentComment.BackColor = newColor;
            }
        }

        private void chkPhone_CheckStateChanged(object sender, EventArgs e)
        {
            if (chkPhone.Checked)
            {
                if (CeelPhoneWait)
                {
                    CeelPhoneWait = false;
                    if (!chkRunWork.Checked)
                    {
                        AppWaitExit = true;
                        OnlyWaitExit = false;
                        OnlyWait(20000);
                    }
                }
            }
        } 
    }
}