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
        string InputData = String.Empty;
        Pen myPen = new Pen(Color.AntiqueWhite);
        Graphics map = null;
        static int startX = 0, startY = 0,startTmpX=0,startTmpY=0, endX = 0, endY = 0, endTmpX=0,endTmpY=0, preMapDone=0;
        static int angle = 180, length = 0, increment = 0;
        float rad = 0;
        float Kp = 0, Ki = 0, Kd = 0;
        int stopTemp = 0, timeOut=0,timeUp=0;
        string Time = "0",set_point="", Mode = "a",startHour="00",startMin="00",data, nhan = "0";
        double set = 0, set1 = 0, time = 0;
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
            map = PMap.CreateGraphics();
            string[] ports = SerialPort.GetPortNames();
            cbxCom.Items.AddRange(ports);
            UART.ReadTimeout = 2000;
            //UART.DataReceived += new SerialDataReceivedEventHandler(data);
            UART.BaudRate = 115200;
            UART.Parity = Parity.None;
            UART.StopBits = StopBits.One;
            timer1.Interval = 300;
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
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P6.Hide(); P5.Show();P8.Hide();PMap.Hide();
            P5.BringToFront(); picturemenu.Hide();
            startX = PMap.Width / 2;startY = PMap.Height / 2;
            //startTmpX = PMap.Width / 2; startTmpY = PMap.Height / 2;
            //label1.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            _KhoiDong();
            this.UART.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.UART_DataReceived);      
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
                //label1.Text = Tam.Substring(1,2);
                //label1.Text = Tam;
                //read value
                battery = Tam.Substring(1, 3);//tach so 
                container = Tam.Substring(4,3);
                //read time 
                second = Tam.Substring(7, 2);
                minute = Tam.Substring(9, 2);
                hour = Tam.Substring(11, 2);
                day = Tam.Substring(13, 2);
                month = Tam.Substring(15, 2);   
                ////trans to double
                Data[0] = Convert.ToDouble(battery);
                Data[1] = Convert.ToDouble(container);
                Data[2] = Convert.ToDouble(minute);
                Data[3] = Convert.ToDouble(hour);
                Data[4] = Convert.ToDouble(day);
                Data[5] = Convert.ToDouble(month);
                //read coordinates of machine
                Data[6] = Convert.ToDouble(Tam.Substring(18, 3)); //length
                Data[7] = Convert.ToDouble(Tam.Substring(21, 3)); //angle
                ////trans to int 
                if (Data[0]<=100) char_battery.Value = Convert.ToInt16(Data[0]);
                length = Convert.ToInt16(Data[6]);        //scale 2:1
                angle = Convert.ToInt16(Data[7]);
                //if (Data[1] <= 100) char_container.Value = Convert.ToInt16(Data[1]);
                //show time
                txtHr.Text = hour;
                txtMin.Text = minute;
                txtSec.Text = second;

                rad = (float)(angle * .017453292519);
                endX = (int)(startX + (Math.Cos(rad) * length/2)+0.5);
                endY = (int)(startY + (Math.Sin(rad) * length/2)+0.5);//017453292519
                if (endX < 0) endX = 0;if (endY < 0) endY = 0;

                StreamWriter write = new StreamWriter("map1", true);      //save coordinates of map
                write.WriteLine(string.Format("{0:000}",startX) + "-" +
                   string.Format("{0:000}", startY) + "-" + string.Format("{0:000}", endX) + 
                   "-" + string.Format("{0:000}", endY)+"-"+angle.ToString()+"-"+length.ToString());
                write.Close();
                if(preMapDone==1)drawMap();
                startX = endX; startY = endY;

                if (Tam.Substring(17, 1) == "s" && stopTemp == 1)
                { stopTemp = 0; btnDung_Click(sender, e); }
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
                btnConnect.Text = "Disconnected";
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
                //btnDung.Enabled = false;
                //btnChay.Enabled = true;
                stt.Text = "THE MACHINE is CONNECTED.";
                stt.ForeColor = Color.Green;
                btnConnect.BackColor = Color.Green;
                lblwarning.Text = "PLEASE SELECT YOUR OPTIONS."; lblwarn.Text = "";lblwarning.ForeColor = Color.Chocolate;
                timer1.Enabled = true;

                startX = PMap.Width / 2; startY = PMap.Height / 2;preMapDone = 0;
                //startTmpX = PMap.Width / 2; startTmpY = PMap.Height / 2;
                File.Delete("map1");
                StreamWriter write = new StreamWriter("map1", true);write.Close();
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
                if (txtHour.Text == "") txtHour.Text = "00";
                if (txtMinute.Text == "") txtMinute.Text = "00";
                if (txtHour.Text == "00" && txtMinute.Text == "00")
                { startHour = hour; startMin = minute; timeOut = 0;
                    tempHour = Convert.ToDouble(hour); tempMinute = Convert.ToDouble(minute);
                }
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
            if (cbxTime.Text == "None" || cbxTime.Text == "")
            {
                time = 0; cbxTime.Text = "None";timeUp = 0;
                Time = string.Format("{0:00}", time);
            }
            else
            {
                time = Convert.ToDouble(cbxTime.Text);
                Time = string.Format("{0:00}", cbxTime.Text);
                timeUp = 1;
            }
            #endregion

            #region send data
            //send data
            data = "S" + set_point + Time + startHour + startMin + Mode;
            UART.Write(data);
            timer1.Enabled = true;
            #endregion
            //change state of button
            lblwarning.Text = ""; stopTemp = 1;
            btnChay.Text = "STARTED";
            btnDung.Enabled = true; btnDung.BackColor = Color.Teal;
            btnChay.Enabled = false; btnChay.BackColor = Color.Green;
            //disable Setting
            chbxAuto.Enabled = false; chbxMan.Enabled = false;
            cbxLevel.Enabled = false; cbxTime.Enabled = false;
            btnTime.Enabled = false; btnTime.BackColor = Color.FromArgb(33, 42, 52); P8.Hide();
            reportPage();
        }
        #endregion

        #region Controller

        private void btnstraight_MouseUp(object sender, MouseEventArgs e)
        {
            UART.Write("d00000000");
            btnstraight.BackColor = Color.LightSeaGreen; btnleft.BackColor = Color.LightSeaGreen;
            btnback.BackColor = Color.LightSeaGreen; btnright.BackColor = Color.LightSeaGreen;
            btnstop.BackColor = Color.Gainsboro;
            btnback.Enabled = true; btnleft.Enabled = true;
            btnright.Enabled = true; btnstraight.Enabled = true;
        }

        private void btnleft_MouseUp(object sender, MouseEventArgs e)
        {
            UART.Write("d00000000");
            btnstraight.BackColor = Color.LightSeaGreen; btnleft.BackColor = Color.LightSeaGreen;
            btnback.BackColor = Color.LightSeaGreen; btnright.BackColor = Color.LightSeaGreen;
            btnstop.BackColor = Color.Gainsboro;
            btnback.Enabled = true; btnleft.Enabled = true;
            btnright.Enabled = true; btnstraight.Enabled = true;
        }

        private void btnback_MouseUp(object sender, MouseEventArgs e)
        {
            UART.Write("d00000000");
            btnstraight.BackColor = Color.LightSeaGreen; btnleft.BackColor = Color.LightSeaGreen;
            btnback.BackColor = Color.LightSeaGreen; btnright.BackColor = Color.LightSeaGreen;
            btnstop.BackColor = Color.Gainsboro;
            btnback.Enabled = true; btnleft.Enabled = true;
            btnright.Enabled = true; btnstraight.Enabled = true;
        }

        private void btnright_MouseUp(object sender, MouseEventArgs e)
        {
            UART.Write("d00000000");
            btnstraight.BackColor = Color.LightSeaGreen; btnleft.BackColor = Color.LightSeaGreen;
            btnback.BackColor = Color.LightSeaGreen; btnright.BackColor = Color.LightSeaGreen;
            btnstop.BackColor = Color.Gainsboro;
            btnback.Enabled = true; btnleft.Enabled = true;
            btnright.Enabled = true; btnstraight.Enabled = true;
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

        private void btnstraight_MouseDown(object sender, MouseEventArgs e)
        {
            UART.Write("h00000000");
            btnstraight.BackColor = Color.Green;
            //btnstop.BackColor = Color.Red;
            btnback.Enabled = false; btnleft.Enabled = false;
            btnright.Enabled = false;
        }

        private void btnright_MouseDown(object sender, MouseEventArgs e)
        {
            UART.Write("p00000000");
            btnright.BackColor = Color.Green;
            btnleft.Enabled = false; btnstraight.Enabled = false;
            btnback.Enabled = false;
        }

        private void btnback_MouseDown(object sender, MouseEventArgs e)
        {
            UART.Write("l00000000");
            btnback.BackColor = Color.Green;
            btnright.Enabled = false;
            btnleft.Enabled = false; btnstraight.Enabled = false;
        }

        private void btnleft_MouseDown(object sender, MouseEventArgs e)
        {
            UART.Write("t00000000");
            btnleft.BackColor = Color.Green;
            btnback.Enabled = false; //btnleft.Enabled = false;
            btnright.Enabled = false; btnstraight.Enabled = false;
        }
        #endregion

        #region Tab in UI
        private void btnHome_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P7.Hide(); PMap.Hide();
            P5.Show(); P5.BringToFront();
            lbltab.Text = "Home Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 70); btnHome.ForeColor = Color.Aquamarine;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
            btnMap.BackColor = Color.FromArgb(41, 39, 40); btnMap.ForeColor = Color.DimGray;
        }

        private void btnConnecting_Click(object sender, EventArgs e)
        {
            P5.Hide(); P2.Hide(); P3.Hide(); P4.Hide();P7.Hide(); PMap.Hide();
            P1.Show(); P1.BringToFront();
            lbltab.Text = "Connection Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 70);btnConnecting.ForeColor = Color.Aquamarine;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor= Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
            btnMap.BackColor = Color.FromArgb(41, 39, 40); btnMap.ForeColor = Color.DimGray;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            P1.Hide(); P5.Hide(); P3.Hide(); P4.Hide(); P7.Hide(); PMap.Hide();
            P2.Show(); P2.BringToFront();
            lbltab.Text = "Settings Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39,70); btnSetting.ForeColor = Color.Aquamarine;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
            btnMap.BackColor = Color.FromArgb(41, 39, 40); btnMap.ForeColor = Color.DimGray;
        }

        private void btnController_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P5.Hide(); P4.Hide(); P7.Hide(); PMap.Hide();
            P3.Show(); P3.BringToFront();
            lbltab.Text = "Controller Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 70); btnController.ForeColor = Color.Aquamarine;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
            btnMap.BackColor = Color.FromArgb(41, 39, 40); btnMap.ForeColor = Color.DimGray;
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P5.Hide(); P7.Hide(); PMap.Hide();
            P4.Show(); P4.BringToFront();
            lbltab.Text = "Status Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 70); btnStatus.ForeColor = Color.Aquamarine;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
            btnMap.BackColor = Color.FromArgb(41, 39, 40); btnMap.ForeColor = Color.DimGray;
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P5.Hide(); PMap.Hide();
            P7.Show(); P7.BringToFront();
            lbltab.Text = "Report Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 70); btnHistory.ForeColor = Color.Aquamarine;
            btnMap.BackColor = Color.FromArgb(41, 39, 40); btnMap.ForeColor = Color.DimGray;
        }

        private void btnMap_Click(object sender, EventArgs e)
        {
           
            //drawMap();
            P1.Hide(); P2.Hide(); P3.Hide(); P4.Hide(); P5.Hide(); P7.Hide();
            PMap.Show(); PMap.BringToFront();
            drawPreMap();
            lbltab.Text = "Map Page";
            btnConnecting.BackColor = Color.FromArgb(41, 39, 40); btnConnecting.ForeColor = Color.DimGray;
            btnSetting.BackColor = Color.FromArgb(41, 39, 40); btnSetting.ForeColor = Color.DimGray;
            btnController.BackColor = Color.FromArgb(41, 39, 40); btnController.ForeColor = Color.DimGray;
            btnStatus.BackColor = Color.FromArgb(41, 39, 40); btnStatus.ForeColor = Color.DimGray;
            btnHome.BackColor = Color.FromArgb(41, 39, 40); btnHome.ForeColor = Color.DimGray;
            btnHistory.BackColor = Color.FromArgb(41, 39, 40); btnHistory.ForeColor = Color.DimGray;
            btnMap.BackColor = Color.FromArgb(41, 39, 70); btnMap.ForeColor = Color.Aquamarine;
        }

        private void btnIntroduce_Click(object sender, EventArgs e)
        {
            if (panel3.Height == 41) panel3.Height = 196;
            else
            { panel3.Height = 41; }

        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            if (panel7.Height == 44) panel7.Height = 196;
            else
            { panel7.Height = 44; }
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

        //Select Start time
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

        #region Expand & Collect menu
        private void button2_Click(object sender, EventArgs e)
        {
            panel5.Width = 169;
            picturemenu.Hide(); logo.Show();
            P1.Location = new Point(169, 74); P2.Location = new Point(169, 74); P3.Location = new Point(169, 74);
            P4.Location = new Point(169, 74); P5.Location = new Point(169, 74);P7.Location = new Point(169, 74);
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

        #region Map Page
        private void PMap_Paint(object sender, PaintEventArgs e)
        {
            myPen.Width = 1;
        }

        private void drawMap()
        {
            //label1.Text = angle.ToString();
            //length = 10;angle = 270;
            //endX = (int)(startX + Math.Cos(angle * .017453292519) * length );
            //endY = (int)(startY + Math.Sin(angle * .017453292519) * length );//017453292519
            Point[] points =
            {
                new Point(startX, startY),
                new Point(endX,  endY)
            };
            map.DrawLines(myPen, points);
            //startX = endX;startY = endY;
        }

        private void drawPreMap()
        {
            List<string> lines = File.ReadAllLines("map1").ToList();
            int j = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                //label1.Text = Convert.ToInt16(Convert.ToDouble(lines[i].Substring(0, 1))).ToString();
                //label1.Text = length.ToString();
                Point[] points =
                   {
                    new Point(Convert.ToInt16(Convert.ToDouble(lines[i].Substring(0, 3))),Convert.ToInt16(Convert.ToDouble(lines[i].Substring(4, 3)))),
                    new Point(Convert.ToInt16(Convert.ToDouble(lines[i].Substring(8, 3))),  Convert.ToInt16(Convert.ToDouble(lines[i].Substring(12, 3))))
                   };
                map.DrawLines(myPen, points);
            }
            preMapDone = 1;
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            set1 = Data[0];
            double tempTimeOut = tempHour * 60 + tempMinute - Data[3] * 60 - Data[2];
            double tempTimeUp = Data[3] * 60 + Data[2] - tempHour * 60 - tempMinute;
            if (timeOut == 1 && tempTimeOut > 0)
            {
                lblhour.Text = string.Format("{0:00}", Convert.ToInt16(tempTimeOut / 60 - 0.5)) + " hrs";
                lblminute.Text = string.Format("{0:00}", (tempTimeOut % 60)) + " mins";
            }
            else
            {
                lblhour.Text = "";
                lblminute.Text = "";
            }
            if (timeUp == 1 && tempTimeUp >= 0)
            {
                char_container.Value = Convert.ToInt16(tempTimeUp * 100 / time - 0.5);
                //label1.Text = string.Format("{0:00}", Convert.ToInt16(tempTimeUp * 100 / time - 0.5));
            }
            else char_container.Value = 0;
        }
    }
}

