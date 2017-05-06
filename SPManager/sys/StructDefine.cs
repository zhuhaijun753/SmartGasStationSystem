﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SPManager
{
    //定义CPU的信息结构  
    [StructLayout(LayoutKind.Sequential,Pack =1)]
    public struct CPU_INFO
    {
        public uint dwOemId;
        public uint dwPageSize;
        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public uint dwProcessorLevel;
        public uint dwProcessorRevision;
    }
    //定义内存的信息结构  
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MEMORY_INFO
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public uint dwTotalPhys;
        public uint dwAvailPhys;
        public uint dwTotalPageFile;
        public uint dwAvailPageFile;
        public uint dwTotalVirtual;
        public uint dwAvailVirtual;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct structNozzle
    {
        public int nozzleNo;
        public int videoChanNo;
        public int left;
        public int right;
        public int top;
        public int bottom;
    }
    class StructDefine
    {
    }
}