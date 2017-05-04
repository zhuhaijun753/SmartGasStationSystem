﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace SPManager
{
    public partial class FormMain : Form
    {
        TH_RECT th_RECT = new TH_RECT();
        
        TH_PlateResult th_PlateResult = new TH_PlateResult();
        NET_DVR_PREVIEWINFO previewInfo = new NET_DVR_PREVIEWINFO();

        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] videoChan = new byte[] { 4, 5, 6,7 };
            IntPtr ip = Marshal.AllocHGlobal(videoChan.Length);
            Marshal.Copy(videoChan, 0, ip, videoChan.Length);
            SPlate.SP_InitRunParam(ip, videoChan.Length);
            SPlate.SP_InitNVR("192.168.1.65",8000,"admin","sd123456").ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TH_PlateIDCfg th_PlateIDCfg = new TH_PlateIDCfg();
            th_PlateIDCfg.nMaxPlateWidth = 400;
            th_PlateIDCfg.nMinPlateWidth = 60;

            th_PlateIDCfg.nMaxImageWidth = 3000;
            th_PlateIDCfg.nMaxImageHeight = 2500;

            th_PlateIDCfg.nFastMemorySize = 16000;//DSP内存大小  
            th_PlateIDCfg.pFastMemory = Marshal.AllocHGlobal(16000);//DSP申请内存 

            th_PlateIDCfg.pMemory = Marshal.AllocHGlobal(100000000);//申请普通内存  
            th_PlateIDCfg.nMemorySize = 100000000;
            th_PlateIDCfg.bUTF8 = 0;
            th_PlateIDCfg.bShadow = 1;
            th_PlateIDCfg.bCarLogo = 0;
            th_PlateIDCfg.bLeanCorrection = 1;
            th_PlateIDCfg.bCarModel = 0;
            th_PlateIDCfg.bOutputSingleFrame = 1;
            th_PlateIDCfg.bMovingImage = 0;
            int ret =  SPlate.SP_InitAlg(ref th_PlateIDCfg);
        }

        private void button4_Click(object sender, EventArgs e)
        {
             previewInfo.hPlayWnd = new IntPtr();//预览窗口
           // previewInfo.hPlayWnd = realVideo.Handle;//预览窗口
            previewInfo.lChannel = 33;//预te览的设备通道
            previewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
            previewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
            previewInfo.bBlocked = false; //0- 非阻塞取流，1- 阻塞取流
            previewInfo.dwDisplayBufNum = 15;
            SPlate.SP_PreviewInfo(ref previewInfo).ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //toolCPU.Text = SystemUnit.getCpuLoad().ToString()+"%";
            MEMORY_INFO MemInfo;
            MemInfo = new MEMORY_INFO();
            SystemUnit.GlobalMemoryStatus(ref MemInfo);
            toolRAM.Text = MemInfo.dwMemoryLoad.ToString() + "%";

            SPlate.SP_TestAPI();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Init();

//             Global.mysqlHelper = new MysqlHelper(new DBInfo("mysql", "gsims", "172.16.84.1", 3306, "root", "root"));
//             CarInfo car = new CarInfo();
//             Global.mysqlHelper.ExecuteSql(car.toSqlString());

            //             Global.nLogLevel = 5;
            //             Global.LogServer = new Log(Global.nLogLevel);
            //             Global.LogServer.Run();
            //             for (int i=0;i<100;i++)
            //             {
            //                 Global.LogServer.Add(new LogInfo("test", i.ToString(), (int)EnumLogLevel.ERROR, DateTime.Now));
            //             }
            //             Global.LogServer.Run();
            //             for (int i = 0; i < 100; i++)
            //             {
            //                 Global.LogServer.Add(new LogInfo("test", i.ToString(), (int)EnumLogLevel.ERROR, DateTime.Now));
            //             }




        }

        private void FormMain_Load(object sender, EventArgs e)
        {
//             this.notifyIconMain.Visible = true;//在通知区显示Form的Icon
// 
//             this.WindowState = FormWindowState.Minimized;
// 
//             this.Visible = false;
// 
//             this.ShowInTaskbar = false;//使Form不在任务栏上显示
        }

        private void notifyIconMain_DoubleClick(object sender, EventArgs e)
        {
            
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;//使Form不在任务栏上显示
        }

       

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnCloseMain_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            int ret = Init();
            if (ret == 0)
                return;
            if((ret&0x01) == 0x01)
            {
                MessageBox.Show("日志启动失败");
            }
            if ((ret & 0x02) == 0x02)
            {
                MessageBox.Show("数据库连接失败");
            }
            if ((ret & 0x04) == 0x04)
            {
                MessageBox.Show("参数初始化失败");
            }
            if ((ret & 0x08) == 0x08)
            {
                MessageBox.Show("算法初始化失败");
            }
            if ((ret & 0x10) == 0x10)
            {
                MessageBox.Show("设备初始化失败");
            }
            if ((ret & 0x20) == 0x20)
            {
                MessageBox.Show("网络服务初始化失败");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            int ret = SPlate.SP_BeginRecog();
            Global.LogServer.Add(new LogInfo("Debug", "main->SP_BeginRecog done return value" +ret.ToString(), (int)EnumLogLevel.DEBUG));
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            CarInfoOut carOut = new CarInfoOut();
            IntPtr pCarOut = Marshal.AllocHGlobal(Marshal.SizeOf(carOut));
            SPlate.SP_GetFirstCarInfo(pCarOut);
            carOut = (CarInfoOut)Marshal.PtrToStructure(pCarOut, typeof(CarInfoOut));
            //carOut = (CarInfoOut)obj;
            MessageBox.Show(carOut.license + carOut.nConfidence.ToString() + carOut.nPicLenth.ToString());
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //previewInfo.hPlayWnd = new IntPtr();//预览窗口
            previewInfo.hPlayWnd = realVideo.Handle;//预览窗口
           // previewInfo.lChannel = 33;//预te览的设备通道
            previewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
            previewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
            previewInfo.bBlocked = false; //0- 非阻塞取流，1- 阻塞取流
            previewInfo.dwDisplayBufNum = 15;
            SPlate.SP_PreviewInfo(ref previewInfo).ToString();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            SPlate.SP_Close();
            if(Global.LogServer != null)
            Global.LogServer.Stop();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SPlate.SP_SetLogLevel(comboLogLevel.SelectedIndex + 1);
        }
    }
}
