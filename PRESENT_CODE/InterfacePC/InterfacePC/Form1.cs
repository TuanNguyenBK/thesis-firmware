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
using System.IO;


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
        int dem, timeOut=0;
        string Time = "0",set_point="", Mode = "a",startHour="00",startMin="00";
        string data;
        string nhan = "0";
        double set = 0, set1 = 0;
        string Tam = "",battery="000",timer="000",container="",minute="00",hour="00",day="00",month="00",second="00";
        double[] Data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        double[] Data_2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        double tempHour, tempMinute;
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
            //initial state of settings
            cbxLevel.Text = "Low";cbxTime.Text = "None";
            btnChay.Enabled = false;
            btnDung.Enabled = false; btnDung.BackColor = Color.FromArgb(33, 42, 52);
            btnTime.Enabled = false; btnTime.BackColor = Color.FromArgb(33, 42, 52);
            chbxAuto.Enabled = false; chbxMan.Enabled = false;
            cbxLevel.Enabled = false; cbxTime.Enabled = false;
            //initial state of controller
            btnback.Enabled = false; btnleft.Enabled = false;
            btnstop.Enabled = false; btnright.Enabled = false;
            btnstraight.Enabled = false;
            //initial state of panel
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P6.Hide(); P5.Show();P8.Hide();
            P5.BringToFront(); picturemenu.Hide(); //label1.Text = "tuannguyen"
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
            reportPage();
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

       
        

        private void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //lbNhietDo.Text = UART.ReadTo("\r");
                Tam = UART.ReadTo("\r");//nhan du lieu tu vdk gui len
                //label1.Text = Tam;
                
                //read value
                battery = Tam.Substring(0, 3);//tach so 
                container = Tam.Substring(3, 3);
                //read time
                second= Tam.Substring(6, 2);
                minute = Tam.Substring(8, 2);
                hour = Tam.Substring(10, 2);
                day = Tam.Substring(12, 2);
                month = Tam.Substring(14, 2);
             
                //trans to double
                Data[0] = Convert.ToDouble(battery);
                Data[1] = Convert.ToDouble(container);
                Data[2] = Convert.ToDouble(minute);
                Data[3] = Convert.ToDouble(hour);
                Data[4] = Convert.ToDouble(day);
                Data[5] = Convert.ToDouble(month);
                //trans to int 
                char_battery.Value = Convert.ToInt16(Data[0]);
                char_container.Value = Convert.ToInt16(Data[1]);
                //show time
                txtHr.Text = hour;
                txtMin.Text = minute;
                txtSec.Text = second;
                if (Tam.Substring(16, 1) == "s")
                { btnDung_Click(sender,e); }
                

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
                btnChay.Enabled = true;
                btnDung.Enabled = true;
            //enable Setting
                chbxAuto.Enabled = true;
                chbxMan.Enabled = true;
                cbxLevel.Enabled = true;
                //cbxTime.Enabled = true;
                btnChay.Text = "SET UP";
            //other changes
                btnDung.Enabled = false;
                btnChay.Enabled = true;
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
                btnChay.Enabled = false;
                btnDung.Enabled = false;
                //disable Controller
                btnback.Enabled = false; btnback.BackColor = Color.FromArgb(33, 42, 52);
                btnleft.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
                btnstop.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
                btnright.Enabled = false; btnright.BackColor = Color.FromArgb(33, 42, 52);
                btnstraight.Enabled = false; btnstraight.BackColor = Color.FromArgb(33, 42, 52);
                //disable Setting
                chbxAuto.Enabled = false;chbxAuto.Checked = false;
                chbxMan.Enabled = false;chbxMan.Checked = false;
                cbxLevel.Enabled = false; cbxTime.Enabled = false;
                btnTime.Enabled = false; btnTime.BackColor = Color.FromArgb(33, 42, 52);

                P3.BringToFront();
                status.Text = "THE MACHINE ARE DISCONNECT";
                lblwarning.Text = "YOU DON'T HAVE CONNECTION YET.";
                lblwarn.Text = "YOU DON'T HAVE CONNECTION YET.";
                lblwarn.ForeColor = Color.Red; lblwarning.ForeColor = Color.Red;
                UART.Close();
            }
        }

        #region Setup machine

        private void btnDung_Click(object sender, EventArgs e)
        {
            data = "D" + "00000000";
            UART.Write(data);
            btnChay.Text = "SET UP"; lblwarning.Text = "PLEASE SELECT YOUR OPTIONS.";
            lblwarning.ForeColor = Color.Chocolate;
            btnDung.BackColor = Color.FromArgb(33, 42, 52);
            btnChay.BackColor = Color.Teal;
            btnDung.Enabled = false;
            btnChay.Enabled = true;
            //enable Setting
            chbxAuto.Enabled = true; chbxMan.Enabled = true;
            cbxLevel.Enabled = true; cbxTime.Enabled = false; btnTime.BackColor = Color.FromArgb(33, 42, 52);
            btnTime.Enabled = false;chbxAuto.Checked = false;chbxMan.Checked = false;
            //disable controller
            btnback.Enabled = false; btnback.BackColor = Color.FromArgb(33, 42, 52);
            btnleft.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
            btnstop.Enabled = false; btnleft.BackColor = Color.FromArgb(33, 42, 52);
            btnright.Enabled = false; btnright.BackColor = Color.FromArgb(33, 42, 52);
            btnstraight.Enabled = false; btnstraight.BackColor = Color.FromArgb(33, 42, 52);
            lblwarn.Text = "";
            //write status
            StreamWriter write = new StreamWriter("sample", true);      //Counting row
            write.WriteLine("10-02 Start  10:56      Stop  12:67099");
            write.Write("flat"); write.Close();
            
            StreamWriter write1 = new StreamWriter("database", true);    // read 4 newest data
            write1.WriteLine(day + "-" + month + " Start  " + startHour + ":" + startMin + "      Stop  " + hour + ":" + minute + battery + Mode); write1.Close();
           

        }

        private void btnChay_Click(object sender, EventArgs e)
        {
            #region select mode
            if (chbxMan.Checked == true)
            {
                Mode = "m";
                //label on controller page
                lblwarn.Text = "THE MACHINE IS BEING IN MANUAL MODE.";
                lblwarn.ForeColor = Color.Green;
                //enable controller
                btnback.Enabled = true; btnback.BackColor = Color.LightSeaGreen;
                btnleft.Enabled = true; btnleft.BackColor = Color.LightSeaGreen;
                btnstop.Enabled = true;
                btnright.Enabled = true; btnright.BackColor = Color.LightSeaGreen;
                btnstraight.Enabled = true; btnstraight.BackColor = Color.LightSeaGreen;
                //save start time
                 startHour = hour;
                 startMin = minute; timeOut = 0;
            }

            if (chbxAuto.Checked == true)
            {
                Mode = "a";
                lblwarn.Text = "THE MACHINE IS BEING IN AUTO MODE."; lblwarn.ForeColor = Color.DarkOrange;
                //select start time
                if (txtHour.Text == "00" && txtMinute.Text == "00")
                { startHour = hour; startMin = minute; timeOut = 0; }
                else
                {
                    timeOut = 1;
                    startHour = string.Format("{0:00}", txtHour.Text); tempHour = Convert.ToDouble(txtHour.Text);
                    startMin = string.Format("{0:00}", txtMinute.Text); tempMinute = Convert.ToDouble(txtMinute.Text);
                }
            }

            if (chbxAuto.Checked == false && chbxMan.Checked == false)
            {
                MessageBox.Show("Please select mode.");
                return;
            }
            #endregion

            //change state of button
            lblwarning.Text = "";
            btnChay.Text = "STARTED";          
            btnDung.Enabled = true; btnDung.BackColor = Color.Teal;
            btnChay.Enabled = false; btnChay.BackColor = Color.Green;
            //disable Setting
            chbxAuto.Enabled = false; chbxMan.Enabled = false;
            cbxLevel.Enabled = false; cbxTime.Enabled = false;
            btnTime.Enabled = false; btnTime.BackColor = Color.FromArgb(33, 42, 52); P8.Hide();

            #region select level vaccum
            //select level for vaccum
            if (cbxLevel.Text == "Low")
            { set_point = "L"; }
            if (cbxLevel.Text == "Medium")
            { set_point = "M"; }
            if (cbxLevel.Text == "High")
            { set_point = "H"; }
            #endregion

            #region  select time up
            if (cbxTime.Text == "None")
            {
                time = 0;
                Time = string.Format("{0:00}", time);
            }
            else Time = string.Format("{0:00}", cbxTime.Text);
           
            #endregion

            #region send data
            //send data
            data = "S" + set_point + Time +startHour+startMin+Mode;
            UART.Write(data);
            timer1.Enabled = true;
            #endregion

            reportPage();
        }
        #endregion

        #region Controller
        private void btnstraight_Click(object sender, EventArgs e)
        {
            UART.Write("h00000000");
            btnstraight.BackColor = Color.Green;
            btnstop.BackColor = Color.Red;
            btnback.Enabled = false; btnleft.Enabled = false;
            btnright.Enabled = false;
            //btnstraight.Enabled = false;
        }

        private void btnleft_Click(object sender, EventArgs e)
        {
            UART.Write("t00000000");
            btnleft.BackColor = Color.Green; btnstop.BackColor = Color.Red;
            btnback.Enabled = false; //btnleft.Enabled = false;
            btnright.Enabled = false; btnstraight.Enabled = false;
        }

        private void btnback_Click(object sender, EventArgs e)
        {
            UART.Write("l00000000");
            btnback.BackColor = Color.Green; btnstop.BackColor = Color.Red;
            //btnback.Enabled = false; btnright.Enabled = false;
            btnleft.Enabled = false; btnstraight.Enabled = false;                      
        }

        private void btnright_Click(object sender, EventArgs e)
        {
            UART.Write("p00000000");
            btnright.BackColor = Color.Green; btnstop.BackColor = Color.Red;
            btnleft.Enabled = false; btnstraight.Enabled = false;
            //btnright.Enabled = false;
            btnback.Enabled = false;
        }

        private void btnstop_Click(object sender, EventArgs e)
        {
            UART.Write("d00000000");
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
            P5.Hide(); P2.Hide(); P3.Hide(); P4.Hide();P7.Hide();
            P1.Show(); P1.BringToFront();
            lbltab.Text = "Connection Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 70);btnConnecting.ForeColor = Color.Aquamarine;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor= Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            P1.Hide(); P5.Hide(); P3.Hide(); P4.Hide(); P7.Hide();
            P2.Show(); P2.BringToFront();
            lbltab.Text = "Settings Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39,70); btnSetting.ForeColor = Color.Aquamarine;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
        }

        private void btnController_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P5.Hide(); P4.Hide(); P7.Hide();
            P3.Show(); P3.BringToFront();
            lbltab.Text = "Controller Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 70); btnController.ForeColor = Color.Aquamarine;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P5.Hide(); P7.Hide();
            P4.Show(); P4.BringToFront();
            lbltab.Text = "Status Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 70); btnStatus.ForeColor = Color.Aquamarine;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
        }     

        private void btnHistory_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P5.Hide();
            P7.Show(); P7.BringToFront();
            lbltab.Text = "Report Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 70); btnHistory.ForeColor = Color.Aquamarine;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P7.Hide();
            P5.Show(); P5.BringToFront();
            lbltab.Text = "Home Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 70); btnHome.ForeColor = Color.Aquamarine;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
        }

        private void btnIntroduce_Click(object sender, EventArgs e)
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

        #region setting time
        private void txtMinute_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txtMinute.Text.Length < 2)
            {
                char ch = e.KeyChar;
                if (!char.IsDigit(ch) && ch != 8) e.Handled = true;
            }
            else
            {
                if (e.KeyChar != 8) e.Handled = true;
            }
        }

        private void txtHour_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txtHour.Text.Length < 2)
            {
                char ch = e.KeyChar;
                if (!char.IsDigit(ch) && ch != 8) e.Handled = true;
            }
            else
            {
                if (e.KeyChar != 8) e.Handled = true;
            }
        }

        private void btnTime_Click(object sender, EventArgs e)
        {
            if (P8.Visible == true) P8.Hide();
            else P8.Show();

        }

        private void P2_Click(object sender, EventArgs e)
        {
            P8.Hide();
        }
        #endregion

        #region user logout

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            P6.Hide();
        }

        private void btnUser_Click(object sender, EventArgs e)
        {
            if (P6.Visible == true) P6.Hide();
            else
            {
                P6.Show();
                P6.BringToFront();
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        //expand & collect menu
        private void button2_Click(object sender, EventArgs e)
        {
            panel5.Width = 200;
            picturemenu.Hide(); logo.Show();
            P1.Location = new Point(200, 74); P2.Location = new Point(200, 74); P3.Location = new Point(200, 74);
            P4.Location = new Point(200, 74); P5.Location = new Point(200, 74);P7.Location = new Point(200, 74);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel5.Width = 53;
            picturemenu.Show();
            logo.Hide();
            P1.Location = new Point(120, 74); P2.Location = new Point(120, 74); P3.Location = new Point(120, 74);
            P4.Location = new Point(120, 74); P5.Location = new Point(120, 74); P7.Location = new Point(120, 74);
        }

        #endregion

        #region select mode
        private void cbx_man_Click(object sender, EventArgs e)
        {
            chbxMan.Checked = true; chbxAuto.Checked = false; cbxTime.Enabled = false;
            btnTime.Enabled = false; btnTime.BackColor = Color.FromArgb(33, 42, 52);
        }

        private void cbx_auto_Click(object sender, EventArgs e)
        {
            chbxMan.Checked = false; chbxAuto.Checked = true; cbxTime.Enabled = true;
            btnTime.Enabled = true; btnTime.BackColor = Color.Turquoise;

        }
        #endregion

        #region Report status
        private String exportArray(string a,int i)
        {
            string text,text1,previousMode="";
            if (a.Substring(38, 1) == "a") previousMode = "Auto";
            else previousMode = "Manual";         
            text = a.Substring(0, 6)+"             "+previousMode + "\r" + a.Substring(6, 29);       
            text1 = a.Substring(35,3);
            double temp = Convert.ToDouble(text1);
            if(i == 1)  status1.Value = Convert.ToInt16(temp);
            if (i == 2) status2.Value = Convert.ToInt16(temp);
            if (i == 3) status3.Value = Convert.ToInt16(temp);
            if (i == 4) status4.Value = Convert.ToInt16(temp);
            return text;
        }

        private void reportPage()
        {
            int loop = 0, i = 1;
            StreamReader read = new StreamReader("sample");
            while (i == 1)
            {
                if (read.ReadLine() != "flat") loop++;
                else i = 0;
            }
            read.Close();
            StreamReader read1 = new StreamReader("database");
            if (loop == 0)
            {
                txtstatus1.Hide(); txtstatus2.Hide(); txtstatus3.Hide(); txtstatus4.Hide();
                status1.Hide(); status2.Hide(); status3.Hide(); status4.Hide();
                btnreset.Hide(); imgreset.Hide();
            }
            else { btnreset.Show();imgreset.Show(); }
            if (loop == 1)
            {
                txtstatus1.Show();status1.Show();
                txtstatus1.Text = exportArray(read1.ReadLine(), 1);
                txtstatus2.Hide(); txtstatus3.Hide();
                txtstatus4.Hide(); status2.Hide(); status3.Hide(); status4.Hide();
            }
            if (loop == 2)
            {
                txtstatus2.Show(); status2.Show();
                txtstatus2.Text = exportArray(read1.ReadLine(), 2);
                txtstatus1.Text = exportArray(read1.ReadLine(), 1);
                txtstatus3.Hide(); txtstatus4.Hide(); status3.Hide(); status4.Hide();
            }
            if (loop == 3)
            {
                txtstatus3.Show(); status3.Show();
                txtstatus3.Text = exportArray(read1.ReadLine(), 3);
                txtstatus2.Text = exportArray(read1.ReadLine(), 2);
                txtstatus1.Text = exportArray(read1.ReadLine(), 1); txtstatus4.Hide(); status4.Hide();
            }

            if (loop >= 4)
            {
                txtstatus4.Show(); status4.Show();
                int j = 0;
                while (j < loop - 4)
                {
                    if (read1.ReadLine() != "") j++;
                }
                txtstatus4.Text = exportArray(read1.ReadLine(), 4);
                txtstatus3.Text = exportArray(read1.ReadLine(), 3);
                txtstatus2.Text = exportArray(read1.ReadLine(), 2);
                txtstatus1.Text = exportArray(read1.ReadLine(), 1);
            }
            read1.Close();

        }

        private void btnreset_Click(object sender, EventArgs e)
        {
            File.Delete("sample");
            File.Delete("Database");
            StreamWriter write = new StreamWriter("sample", true);
            write.Write("flat"); write.Close();
            StreamWriter write1 = new StreamWriter("database", true); write1.Close();
            reportPage();
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            set1 = Data[0];
            double temp = tempHour * 60 + tempMinute - Data[3] * 60 - Data[2];
            if (timeOut == 1&& temp>0)
            {
                lblhour.Text = string.Format("{0:00}",Convert.ToInt16(temp / 60 - 0.5)) + " hrs";
                lblminute.Text = string.Format("{0:00}",(temp % 60)) + " mins";
            }
            else
            {
                lblhour.Text = "";
                lblminute.Text = "";
            }

            //char_battery.Value = Convert.ToInt16(Data[0]);
            //char_container.Value = Convert.ToInt16(Data[1]);
            //label1.Text = battery;

        }
    }
}

