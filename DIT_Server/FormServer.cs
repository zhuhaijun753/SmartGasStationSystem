﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DIT_Server
{
    public partial class FormServer : Form
    {
        
        SocketTool sockTool;
        public FormServer()
        {
            InitializeComponent();
        }
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == Convert.ToInt32(SysUnit.WM_CARSNAP))
            {
                if (comboMode.SelectedIndex == 0)
                {
                    Random rd = new Random();
                    comboPlateColor.SelectedIndex = rd.Next(comboPlateColor.Items.Count);
                    comboCarBrand.SelectedIndex = rd.Next(comboCarBrand.Items.Count);
                    comboCarColor.SelectedIndex = rd.Next(comboCarColor.Items.Count);
                    comboPlateColor.SelectedIndex = rd.Next(comboPlateColor.Items.Count);
                    string plate = (100000 + rd.Next(100000)).ToString();
                    textLicense.Text = "京A" + plate.Substring(1, 5);
                }
                comboNozzle.SelectedIndex = SysUnit.byNozzle;
                comboNozzleStatus.SelectedIndex = SysUnit.byNozzleStatus;
                sentSnapInfo();
            }
            else if (m.Msg == Convert.ToInt32(SysUnit.WM_CARTRADE))
            {
                lblOilType.Text = SysUnit.tradeInfo.nOilType.ToString();
                lblTradeLitre.Text = SysUnit.tradeInfo.fTradeLitre.ToString();
                lblTradeMoney.Text = SysUnit.tradeInfo.fTradeMoney.ToString();
                lblTradePrice.Text = SysUnit.tradeInfo.fTradePrice.ToString();
                lblStartTime.Text = System.Text.Encoding.Default.GetString(SysUnit.tradeInfo.sStartTime);
                lblEndTime.Text = System.Text.Encoding.Default.GetString(SysUnit.tradeInfo.sStartTime);
                lblStartRead.Text = SysUnit.tradeInfo.fStartRead.ToString();
                lblEndRead.Text = SysUnit.tradeInfo.fEndRead.ToString();
                comboNozzle.Text = SysUnit.tradeInfo.nPumpID.ToString();
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }
        private void sendCallbackInfo()
        {
            NET_ITS_PLATE_RESULT callBackInfo = new NET_ITS_PLATE_RESULT();
            callBackInfo.sLicense = new byte[16];
            byte[] byLicense = System.Text.Encoding.Default.GetBytes(textLicense.Text.Trim());
            Buffer.BlockCopy(byLicense, 0, callBackInfo.sLicense, 0, byLicense.Length);
            //SysUnit.carInfo.sLicense = textLicense.Text.Trim().ToCharArray();
            callBackInfo.byColor = (byte)comboCarColor.SelectedIndex;
            callBackInfo.byPlateColor = (byte)comboPlateColor.SelectedIndex;
            callBackInfo.wVehicleLogoRecog = (short)comboCarBrand.SelectedIndex;
            callBackInfo.wVehicleSubLogoRecog = 0;
            callBackInfo.byVehicleShape = 0;
            callBackInfo.byVehicleState = (byte)(comboInOut.SelectedIndex + 1);
            byte[] data = StrutsToBytesArray(callBackInfo);
           
            byte[] sendbuf = new byte[data.Length +8];
            sendbuf[0] = 0xFF;
            sendbuf[1] = 0xFF;
            sendbuf[2] = 0x06;
            sendbuf[3] = 24;
            Buffer.BlockCopy(data, 0, sendbuf, 4, data.Length);
            uint crc = getCRC(sendbuf, 0, data.Length +4);
            sendbuf[data.Length+4] = (byte)(crc / 256);
            sendbuf[data.Length + 5] = (byte)(crc % 256);
            sendbuf[data.Length + 6] = 0xEE;
            sendbuf[data.Length + 7] = 0xEE;
            sockTool.Send(sendbuf);
           
            
        }
        private void sentSnapInfo()
        {
            NET_DVR_PLATE_RESULT cnapInfo = new NET_DVR_PLATE_RESULT();
            cnapInfo.sLicense = new byte[16];
            byte[] byLicense = System.Text.Encoding.Default.GetBytes(textLicense.Text.Trim());
            Buffer.BlockCopy(byLicense, 0, cnapInfo.sLicense, 0, byLicense.Length);
            //SysUnit.carInfo.sLicense = textLicense.Text.Trim().ToCharArray();
            cnapInfo.byColor = (byte)comboCarColor.SelectedIndex;
            cnapInfo.byPlateColor = (byte)comboPlateColor.SelectedIndex;
            cnapInfo.wVehicleLogoRecog = (short)comboCarBrand.SelectedIndex;
            cnapInfo.wVehicleSubLogoRecog = 0;
            cnapInfo.byVehicleShape = 0;
            cnapInfo.byPumpID = (byte)comboNozzle.SelectedIndex;
            cnapInfo.byPumpStatus = (byte)comboNozzleStatus.SelectedIndex;
            //NET_DVR_PLATE_RESULT result = SysUnit.carInfo;
            byte[] data = StrutsToBytesArray(cnapInfo);
            byte[] sendbuf = new byte[data.Length+8];
            sendbuf[0] = 0xFF;
            sendbuf[1] = 0xFF;
            sendbuf[2] = 0x03;
            sendbuf[3] = 25;
            Buffer.BlockCopy(data, 0, sendbuf, 4, data.Length);
            uint crc = getCRC(sendbuf, 0, 29);
            sendbuf[data.Length + 4] = (byte)(crc / 256);
            sendbuf[data.Length + 5] = (byte)(crc % 256);
            sendbuf[data.Length + 6] = 0xEE;
            sendbuf[data.Length + 7] = 0xEE;
            sockTool.Send(sendbuf);
        }
        public byte[] StrutsToBytesArray(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (null != sockTool)
            {
                sockTool.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboNozzle.SelectedIndex = 0;
            comboNozzleStatus.SelectedIndex = 0;
            comboPlateColor.SelectedIndex = 0;
            comboCarBrand.SelectedIndex = 0;
            comboCarColor.SelectedIndex = 0;
            comboPlateColor.SelectedIndex = 0;
            comboMode.SelectedIndex = 0;
            comboInOut.SelectedIndex = 0;
            sockTool = new SocketTool("127.0.0.1", 8870);
            sockTool.Run();
        }

        private void timerSend_Tick(object sender, EventArgs e)
        {
            if (comboMode.SelectedIndex == 0)
            {
                Random rd = new Random();
                comboPlateColor.SelectedIndex = rd.Next(comboPlateColor.Items.Count);
                comboCarBrand.SelectedIndex = rd.Next(comboCarBrand.Items.Count);
                comboCarColor.SelectedIndex = rd.Next(comboCarColor.Items.Count);
                comboPlateColor.SelectedIndex = rd.Next(comboPlateColor.Items.Count);
                string plate = (100000 + rd.Next(100000)).ToString();
                textLicense.Text = "京A" + plate.Substring(1, 5);
            }

            sendCallbackInfo();
        }

        private void btnSendAuto_Click(object sender, EventArgs e)
        {
            timerSend.Interval = 1000 * int.Parse(textInterval.Text.Trim());
            if(timerSend.Enabled  == false)
            {
                btnSendAuto.Text = "停止发送";
            }else
            {
                btnSendAuto.Text = "循环发送";
            }
            timerSend.Enabled = !timerSend.Enabled;

        }
        public  ushort getCRC(byte[] buff, int index, int len)
        {
            byte uchCRCHi = 0xFF;
            byte uchCRCLo = 0xFF;
            int uIndex = 0;
            int j = 0;
            int length = len;
            while (length-- > 0)
            {
                uIndex = uchCRCHi ^ buff[index + j++]; /* 计算CRC */
                uchCRCHi = (byte)(uchCRCLo ^ auchCRCHi[uIndex]);
                uchCRCLo = (byte)auchCRCLo[uIndex];
            }
            ushort ret = (ushort)(uchCRCHi << 8 | uchCRCLo);
            return ret;
        }
        static byte[] auchCRCHi = {
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
                };
        static byte[] auchCRCLo = {
                0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
                0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
                0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
                0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
                0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
                0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
                0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
                0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
                0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
                0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
                0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
                0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
                0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
                0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
                0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
                0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
                0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
                0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
                0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
                0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
                0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
                0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
                0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
                0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
                0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
                0x43, 0x83, 0x41, 0x81, 0x80, 0x40
                };

        private void btnSendSingle_Click(object sender, EventArgs e)
        {
            if (comboMode.SelectedIndex == 0)
            {
                Random rd = new Random();
                comboPlateColor.SelectedIndex = rd.Next(comboPlateColor.Items.Count);
                comboCarBrand.SelectedIndex = rd.Next(comboCarBrand.Items.Count);
                comboCarColor.SelectedIndex = rd.Next(comboCarColor.Items.Count);
                comboPlateColor.SelectedIndex = rd.Next(comboPlateColor.Items.Count);
                string plate = (100000 + rd.Next(100000)).ToString();
                textLicense.Text = "京A" + plate.Substring(1, 5);
            }

            sendCallbackInfo();
        }
    }
}
