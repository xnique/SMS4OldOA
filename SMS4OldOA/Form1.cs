using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Threading;
using System.Globalization;

namespace SMS4OldOA
{
    public partial class Form1 : Form
    {
        private TouchSMS.SmsMessageServiceClient SMSC = null;
        private string username = "smsoldoa";
        private string password = "Sjiaow*6638oa";
        private static bool quit = true;
        private static string oracle_conn = "server=12.5.37.98;Data Source=orcl190;User Id=shoa;Password=Sjiaow_ora#32226638;Integrated Security=no";
        private OracleConnection conn = new OracleConnection(oracle_conn);
        private OracleCommand cmd = new OracleCommand();
        private OracleCommand cmd1 = new OracleCommand();
        private int sms_id = 0;
        private string cell_no = "";
        private string content = "";
        private static int sendinterval = 200;
        private static int scaninterval = 3;
        //private string send_date = "";
        //private CultureInfo CI = Thread.CurrentThread.CurrentCulture;

        private delegate void SetTextBoxDelegate(string text,string text2);
        public Form1()
        {
            InitializeComponent();
            button3.Enabled = quit;
            button4.Enabled = !quit;
            
            cmd.CommandText = "select sms_id,content,cell_no from tbl_ad_sms where sms_id>20886182 and scan_flag is null";
            
            //cmd1.CommandText = 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TouchSMS.SmsMessage msg = new TouchSMS.SmsMessage();
            //这是某个人的手机号码
            msg.destAddr = "18116286599";
            msg.content = "oa测试短信";
            string resp = SMSC.send(username, password, msg);
            textBox1.AppendText(resp + "\r\n");
            //SMSC.
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TouchSMS.SmsReport[] reports = SMSC.getSmsReports(username, password, textBox3.Text, textBox4.Text);
            if (reports.Length>0) textBox1.AppendText(reports[0].statusCode.ToString() + "\r\n");
            //textBox1.Text = DateTime.Now.ToString();
            
        }

        private void setTextBox(string text,string text2)
        {
            if (!this.InvokeRequired)
            {
                if (text != "") textBox1.AppendText(text + "\r\n");
                if (text2 != "") textBox2.Text = text2;
            }
            else
            {
                SetTextBoxDelegate stbd = new SetTextBoxDelegate(setTextBox);
                this.BeginInvoke(stbd, text,text2);
            }
        }



        private void SendSMS()
        {
            while(true)
            {
                try
                {
                    if (quit) break;
                    if (conn.State != ConnectionState.Open) conn.Open();
                    OracleDataReader odr = cmd.ExecuteReader();
                    setTextBox("", "cmd executed..." + DateTime.Now.ToString());
                    while (odr.Read())
                    {
                        sms_id = odr.GetInt32(0);
                        content = odr.GetString(1);
                        cell_no = odr.GetString(2);
                        TouchSMS.SmsMessage msg = new TouchSMS.SmsMessage();
                        msg.destAddr = cell_no;
                        msg.content = content;
                        string resp = SMSC.send(username, password, msg);
                        setTextBox(DateTime.Now.ToString() + "，向" + cell_no + "发送短信，内容为：【" + content + "】。发送结果ID" + resp, "");
                        cmd1.CommandText = "update tbl_ad_sms set scan_flag=1,status=1,send_date=to_date('" + DateTime.Now.ToString() + "','yyyy/mm/dd hh24:mi:ss') where sms_id=" + sms_id.ToString();
                        cmd1.ExecuteNonQuery();
                        System.Threading.Thread.Sleep(sendinterval);
                    }
                    odr.Close();
                    System.Threading.Thread.Sleep(scaninterval * 1000);
                }
                catch(Exception ex)
                {
                    setTextBox(ex.ToString(), "");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (SMSC == null) SMSC = new TouchSMS.SmsMessageServiceClient();
            if (conn.State != ConnectionState.Open)
            {
                try
                {
                    conn.Open();
                    quit = false;
                    Thread t = new Thread(SendSMS);
                    t.Start();
                    button3.Enabled = quit;
                    button4.Enabled = !quit;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("连接数据库失败！" + ex.ToString());
                    button3.Enabled = false;
                    quit = true;
                }
                cmd.Connection = conn;
                cmd1.Connection = conn;
            }
            else
            {
                quit = false;
                Thread t = new Thread(SendSMS);
                t.Start();
                button3.Enabled = quit;
                button4.Enabled = !quit;
            }
            //Thread.Sleep(10000);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            quit = true;
            button3.Enabled = quit;
            button4.Enabled = !quit;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit = true;
            conn.Close();
        }

        private void Form1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            sendinterval = (int)numericUpDown1.Value;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            scaninterval = (int)numericUpDown2.Value;
        }
    }
}
