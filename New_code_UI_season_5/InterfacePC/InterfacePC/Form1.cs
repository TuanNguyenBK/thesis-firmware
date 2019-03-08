using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;


namespace InterfacePC
{
    public partial class Form1 : Form
    {
        #region Quan ly bien
        List<Panel> listPanel = new List<Panel>();
        SerialPort UART = new SerialPort();
        long tickStart = 0;
        public enum _enCheDo { Compact = 0, Scroll };
        _enCheDo CheDo = _enCheDo.Compact;
        string InputData = String.Empty;
        delegate void SetTextCallback(string text);
        float Kp = 0, Ki = 0, Kd = 0;
        int time = 0, w = 0, Tg = 0, dr = 0;
        int dem, www;
        string set_point = "0";
        string Time = "0", Mode = "A";
        string data;
        string nhan = "0";
        double set = 0, set1 = 0;
        string Tam = "",battery="",timer="",container="";

        #endregion

        #region Quan ly ham
        private void _KhoiDong()
        {
            cbxCom.DataSource = SerialPort.GetPortNames();
            if (cbxCom.Items.Count > 0)
            {
                cbxCom.SelectedIndex = 0;
            }
        }
        #endregion

        #region Quan ly form
        public Form1()
        {
            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();
            cbxCom.Items.AddRange(ports);
            UART.ReadTimeout = 2000;
            //UART.DataReceived += new SerialDataReceivedEventHandler(data);
            UART.BaudRate = 115200;
            UART.Parity = Parity.None;
            UART.StopBits = StopBits.One;
            timer1.Interval = 100;
            cbxlevel.Text = "Low";
            cbxtime.Text = "None";
            btnback.Enabled = false;
            btnleft.Enabled = false;
            btnstop.Enabled = false;
            btnright.Enabled = false;
            btnstraight.Enabled = false;
            btnchay.Enabled = false;
            btndung.Enabled = false; btndung.BackColor = Color.FromArgb(33, 42, 52);
            cbx_auto.Enabled = false;
            cbx_man.Enabled = false;
            cbxlevel.Enabled = false;
            cbxtime.Enabled = false;
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P6.Hide(); P5.Show();
            P5.BringToFront(); picturemenu.Hide(); //label1.Text = "tuannguyen";
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            _KhoiDong();
            this.UART.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.UART_DataReceived);
            //listPanel.Add(P1);
            //listPanel.Add(P2);
            //listPanel.Add(P3);
            //listPanel.Add(P4);
            //listPanel.Add(P5);
            //listPanel[4].BringToFront();

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    {
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                        break;
                    }
            }
        }
        #endregion

       
        double[] Data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        double[] Data_2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //lbNhietDo.Text = UART.ReadTo("\r");
                Tam = UART.ReadExisting();//nhan du lieu tu vdk gui len
                //lbtocdo.Text = Tam.ToString();
                battery = Tam.Substring(0, 3);//tach so 
                container= Tam.Substring(3, 3);
                //set = Convert.ToDouble(battery);
                Data[0] = Convert.ToDouble(battery);
                Data[1] = Convert.ToDouble(container);
                char_battery.Value = Convert.ToInt16(Data[0]);
                char_container.Value = Convert.ToInt16(Data[1]);

                // lbtocdo.Text = Data[0].ToString() ;
                //if (Index <= 8)
                //{
                //    Index = Index + 1;
                //}

                //for (int i = Index; i >= 1; i--)
                //{
                //    Data[i] = Data[i - 1];
                //    Data_2[i] = Data_2[i - 1];                   
                //}

                BeginInvoke(new Action(() =>
                {


                }));
            }
            catch (Exception)
            {

            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Connect")
            {
                try
                {
                    UART.PortName = cbxCom.Text;
                    UART.Open();
                }
                catch (Exception)
                {
                    MessageBox.Show("This Com is unavailable now. PLease select another one.");
                    return;
                }
                btnConnect.Text = "Disconnect";
                //enable START & STOP
                btnchay.Enabled = true;
                btndung.Enabled = true;
                //enable Setting
                cbx_auto.Enabled = true;
                cbx_man.Enabled = true;
                cbxlevel.Enabled = true;
                cbxtime.Enabled = true;
                btnchay.Text = "SET UP";
                //btnchay.BackColor = Color.Teal;
                btndung.Enabled = false;
                btnchay.Enabled = true;
                stt.Text = "THE MACHINE is CONNECTED.";
                stt.ForeColor = Color.Green;
                btnConnect.BackColor = Color.Green;
                lblwarning.Text = "PLEASE SELECT YOUR OPTIONS."; lblwarn.Text = "";lblwarning.ForeColor = Color.Chocolate;
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
                btnConnect.Text = "Connect";
                stt.Text = "THE MACHINE IS DISCONNECTED.";
                stt.ForeColor = Color.Red;
                btnConnect.BackColor = Color.Teal;
                //disable START & STOP
                btnchay.Enabled = false;
                btndung.Enabled = false;
                //disable Controller
                btnback.Enabled = false; btnback.BackColor = Color.FromArgb(33, 42, 52);
                btnleft.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
                btnstop.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
                btnright.Enabled = false; btnright.BackColor = Color.FromArgb(33, 42, 52);
                btnstraight.Enabled = false; btnstraight.BackColor = Color.FromArgb(33, 42, 52);
                //disable Setting
                cbx_auto.Enabled = false;
                cbx_man.Enabled = false;
                cbxlevel.Enabled = false;
                cbxtime.Enabled = false;

                P3.BringToFront();
                status.Text = "THE MACHINE ARE DISCONNECT";
                lblwarning.Text = "YOU DON'T HAVE CONNECTION YET.";
                lblwarn.Text = "YOU DON'T HAVE CONNECTION YET.";
                lblwarn.ForeColor = Color.Red; lblwarning.ForeColor = Color.Red;
                UART.Close();
            }
        }

        #region Setup machine

        private void btndung_Click(object sender, EventArgs e)
        {
            data = "D" + "0000";
            UART.Write(data);
            btnchay.Text = "SET UP"; lblwarning.Text = "PLEASE SELECT YOUR OPTIONS.";
            lblwarning.ForeColor = Color.Chocolate;
            btndung.BackColor = Color.FromArgb(33, 42, 52);
            btnchay.BackColor = Color.Teal;
            btndung.Enabled = false;
            btnchay.Enabled = true;
            //enable Setting
            cbx_auto.Enabled = true;
            cbx_man.Enabled = true;
            cbxlevel.Enabled = true;
            cbxtime.Enabled = true;
            //disable controller
            btnback.Enabled = false; btnback.BackColor = Color.FromArgb(33, 42, 52);
            btnleft.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
            btnstop.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
            btnright.Enabled = false; btnright.BackColor = Color.FromArgb(33, 42, 52);
            btnstraight.Enabled = false; btnstraight.BackColor = Color.FromArgb(33, 42, 52);
            lblwarn.Text = "";
            //timer1.Enabled = false;
            // timer1.Interval = 50;    

        }

        private void btnchay_Click(object sender, EventArgs e)
        {
            #region select mode
            if (cbx_man.Checked == true)
            {
                Mode = "m";
                lblwarn.Text = "THE MACHINE IS BEING IN MANUAL MODE.";
                lblwarn.ForeColor = Color.Green;
                btnback.Enabled = true; btnback.BackColor = Color.LightSeaGreen;
                btnleft.Enabled = true; btnleft.BackColor = Color.LightSeaGreen;
                btnstop.Enabled = true;
                btnright.Enabled = true; btnright.BackColor = Color.LightSeaGreen;
                btnstraight.Enabled = true; btnstraight.BackColor = Color.LightSeaGreen;
            }

            if (cbx_auto.Checked == true)
            {
                lblwarn.Text = "THE MACHINE IS BEING IN AUTO MODE.";
                lblwarn.ForeColor = Color.DarkOrange;
            }
            //enable Controller
            if (cbx_auto.Checked == false && cbx_man.Checked == false)
            {
                MessageBox.Show("Please select mode.");
                return;
            }

            #endregion
            lblwarning.Text = "";
            btnchay.Text = "STARTED";
            btnchay.BackColor = Color.Green;
            btndung.BackColor = Color.Teal;
            btndung.Enabled = true;
            btnchay.Enabled = false;
            //disable Setting
            cbx_auto.Enabled = false;
            cbx_man.Enabled = false;
            cbxlevel.Enabled = false;
            cbxtime.Enabled = false;

            #region select level vaccum
            //select level for vaccum
            if (cbxlevel.Text == "Low")
            { set_point = "L"; }
            if (cbxlevel.Text == "Medium")
            { set_point = "M"; }
            if (cbxlevel.Text == "High")
            { set_point = "H"; }
            #endregion

            #region  select timer
            if (cbxtime.Text == "None")
            {
                time = 0;
                Time = string.Format("{0:00}", time);
            }
            if (cbxtime.Text == "10")
            {
                time = 10;
                Time = string.Format("{0:00}", time);
            }
            if (cbxtime.Text == "20")
            {
                time = 20;
                Time = string.Format("{0:00}", time);
            }

            if (cbxtime.Text == "30")
            {
                time = 30;
                Time = string.Format("{0:00}", time);
            }

            if (cbxtime.Text == "60")
            {
                time = 60;
                Time = string.Format("{0:00}", time);
            }
            #endregion

            #region send data
            //send data
            data = "S" + set_point + Time + Mode;
            UART.Write(data);
            timer1.Enabled = true;
            #endregion
            //timer1.Interval = Tg;
            //btnchay.Enabled = false;
            //btndung.Enabled = true;

        }

        #endregion

        #region Controller
        private void btnstraight_Click(object sender, EventArgs e)
        {
            UART.Write("h0000");
            btnstraight.BackColor = Color.Green;
            btnstop.BackColor = Color.Red;
            btnback.Enabled = false; btnleft.Enabled = false;
            btnright.Enabled = false;
            //btnstraight.Enabled = false;
        }

        private void btnleft_Click(object sender, EventArgs e)
        {
            UART.Write("t0000");
            btnleft.BackColor = Color.Green; btnstop.BackColor = Color.Red;
            btnback.Enabled = false; //btnleft.Enabled = false;
            btnright.Enabled = false; btnstraight.Enabled = false;
        }


        private void btnback_Click(object sender, EventArgs e)
        {
            UART.Write("l0000");
            btnback.BackColor = Color.Green; btnstop.BackColor = Color.Red;
            //btnback.Enabled = false; btnright.Enabled = false;
            btnleft.Enabled = false; btnstraight.Enabled = false;
            
           
        }

        private void btnright_Click(object sender, EventArgs e)
        {
            UART.Write("p0000");
            btnright.BackColor = Color.Green; btnstop.BackColor = Color.Red;
            btnleft.Enabled = false; btnstraight.Enabled = false;
            //btnright.Enabled = false;
            btnback.Enabled = false;
        }

        private void btnstop_Click(object sender, EventArgs e)
        {
            UART.Write("d0000");
            btnstraight.BackColor = Color.LightSeaGreen; btnleft.BackColor = Color.LightSeaGreen;
            btnback.BackColor = Color.LightSeaGreen; btnright.BackColor = Color.LightSeaGreen;
            btnstop.BackColor = Color.Gainsboro;
            btnback.Enabled = true; btnleft.Enabled = true;
            btnright.Enabled = true; btnstraight.Enabled = true;


        }


        #endregion

        #region Tab in UI

        private void btnConnecting_Click(object sender, EventArgs e)
        {
            P5.Hide(); P2.Hide(); P3.Hide(); P4.Hide();
            P1.Show(); P1.BringToFront();
            lbltab.Text = "Connection Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 70);
            btnsetting.BackColor = Color.FromArgb(41, 39, 40);
            btncontroller.BackColor = Color.FromArgb(41, 39, 40);
            btnstatus.BackColor = Color.FromArgb(41, 39, 40);
            btnhome.BackColor = Color.FromArgb(41, 39, 40);
        }

        private void btnsetting_Click(object sender, EventArgs e)
        {
            P1.Hide(); P5.Hide(); P3.Hide(); P4.Hide();
            P2.Show(); P2.BringToFront();
            lbltab.Text = "Settings Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40);
            btnsetting.BackColor = Color.FromArgb(41, 39, 70);
            btncontroller.BackColor = Color.FromArgb(41, 39, 40);
            btnstatus.BackColor = Color.FromArgb(41, 39, 40);
            btnhome.BackColor = Color.FromArgb(41, 39, 40);
        }

        private void btncontroller_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P5.Hide(); P4.Hide();
            P3.Show(); P3.BringToFront();
            lbltab.Text = "Controller Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40);
            btnsetting.BackColor = Color.FromArgb(41, 39, 40);
            btncontroller.BackColor = Color.FromArgb(41, 39, 70);
            btnstatus.BackColor = Color.FromArgb(41, 39, 40);
            btnhome.BackColor = Color.FromArgb(41, 39, 40);
        }

        private void btnstatus_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P5.Hide();
            P4.Show(); P4.BringToFront();
            lbltab.Text = "Status Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40);
            btnsetting.BackColor = Color.FromArgb(41, 39, 40);
            btncontroller.BackColor = Color.FromArgb(41, 39, 40);
            btnstatus.BackColor = Color.FromArgb(41, 39, 70);
            btnhome.BackColor = Color.FromArgb(41, 39, 40);
        }

        private void btnhome_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide();
            P5.Show(); P5.BringToFront();
            lbltab.Text = "Home Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40);
            btnsetting.BackColor = Color.FromArgb(41, 39, 40);
            btncontroller.BackColor = Color.FromArgb(41, 39, 40);
            btnstatus.BackColor = Color.FromArgb(41, 39, 40);
            btnhome.BackColor = Color.FromArgb(41, 39, 70);
        }

        private void btnintroduce_Click(object sender, EventArgs e)
        {
            if (panel3.Height == 46) panel3.Height = 196;
            else
            { panel3.Height = 46; }

        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            if (panel7.Height == 46) panel7.Height = 196;
            else
            { panel7.Height = 46; }
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {

        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            P6.Hide();
        }

        private void btnuser_Click(object sender, EventArgs e)
        {
            P6.Show();
            P6.BringToFront();
        }

        private void btnlogout_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P6.Hide();
            P5.Hide(); lbltab.Text = "Login Page";
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel5.Width = 200;
            picturemenu.Hide(); logo.Show();
            P1.Location = new Point(200, 74); P2.Location = new Point(200, 74); P3.Location = new Point(200, 74);
            P4.Location = new Point(200, 74); P5.Location = new Point(200, 74);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel5.Width = 53;
            picturemenu.Show();
            logo.Hide();
            P1.Location = new Point(120, 74); P2.Location = new Point(120, 74); P3.Location = new Point(120, 74);
            P4.Location = new Point(120, 74); P5.Location = new Point(120, 74);
        }

        #endregion

        private void cbx_man_Click(object sender, EventArgs e)
        {
            cbx_man.Checked = true;
            cbx_auto.Checked = false;
        }

        private void cbx_auto_Click(object sender, EventArgs e)
        {
            cbx_man.Checked = false;
            cbx_auto.Checked = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            set1 = Data[0];
            //char_battery.Value = Convert.ToInt16(Data[0]);
            //char_container.Value = Convert.ToInt16(Data[1]);
            //label1.Text = battery;

        }
    }
}

