// SPlate.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "SPlate.h"

//硬盘录像机相关
NET_DVR_DEVICEINFO_V30 deviceInfo;
NET_DVR_PREVIEWINFO previewInfo;
HWND hRealVideoHanlder;
NVRInfo nvrInfo;
LONG testBufferSize ;
LONG callBackNum;

TH_PlateIDCfg th_PlateIDCfg ;
TH_PlateIDResult recogResult[6];
int nCarNum;
BITMAPINFOHEADER bitmapHeader;
TH_RECT th_RECT;
LONG nPort = -1;

int nCurGetIndex = 0;
int nCurPutIndex = 0;
CarInfoOut carInfoOut[MAX_CAR_COUNT];
unsigned char *videoChan = new unsigned char[MAX_VIDEO_CHANNEL_COUNT];
int nVideoChanCount;
int nCurVideoChan = 0;
int nLogLevel = 3;
int nCurCarCount = 0;
char debugInfo[256] ;





SPLATE_API int SP_InitRunParam(unsigned char *pChan, int lenth)
{
	if (lenth > MAX_VIDEO_CHANNEL_COUNT)
	{
		return INVALID_VIDEO_COUNT;
	}
	memcpy(videoChan, pChan, lenth);
	nVideoChanCount = lenth;
	return SUCCESS;
}

SPLATE_API int SP_InitNVR(char *IpAddress, LONG nPort, char *sAdmin, char *sPassword)
{
	bool ret = NET_DVR_Init();
	NET_DVR_SetConnectTime(2000, 1); 
	NET_DVR_SetReconnect(10000, true);
	if (!ret)
	{
		return NET_DVR_GetLastError();
	}
	nvrInfo.IpAddress = IpAddress;
	nvrInfo.sAdmin = sAdmin;
	nvrInfo.sPassword = sPassword;
	nvrInfo.nPort = nPort;
	nvrInfo.m_lServerID = NET_DVR_Login_V30(nvrInfo.IpAddress, nvrInfo.nPort, nvrInfo.sAdmin, nvrInfo.sPassword, &deviceInfo);
	if (nvrInfo.m_lServerID < 0)
	{
		return NET_DVR_GetLastError();
	}
	return SUCCESS;
}

SPLATE_API int SP_Close()
{
	if (nvrInfo.m_lServerID > 0)
	{
		NET_DVR_Logout(nvrInfo.m_lServerID);
		NET_DVR_Cleanup();
	}
	
	
	return SUCCESS;
}

SPLATE_API int SP_PreviewInfo(NET_DVR_PREVIEWINFO *preInfo)
{
	memcpy(&previewInfo, preInfo, sizeof(previewInfo));
	previewInfo.lChannel = videoChan[nCurVideoChan];
	HWND pUser = nullptr;//用户数据
	nvrInfo.m_lPlayHandle = NET_DVR_RealPlay_V40(nvrInfo.m_lServerID, &previewInfo, RealDataCallBack, pUser);
	if (nvrInfo.m_lPlayHandle < 0)
	{
		return NET_DVR_GetLastError();
	}
	return nvrInfo.m_lPlayHandle;
}
SPLATE_API int SP_BeginRecog()
{
	previewInfo.hPlayWnd = nullptr;
	previewInfo.lChannel = videoChan[nCurVideoChan];

	previewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
	previewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
	previewInfo.bBlocked = false; //0- 非阻塞取流，1- 阻塞取流
	previewInfo.dwDisplayBufNum = 15;
	HWND pUser = nullptr;//用户数据
	nvrInfo.m_lPlayHandle = NET_DVR_RealPlay_V40(nvrInfo.m_lServerID, &previewInfo, RealDataCallBack, pUser);
	
	if (nLogLevel <= 3)
	{
		memset(debugInfo, 0, sizeof(debugInfo));
		strcpy(debugInfo, "NET_DVR_RealPlay_V40 return value:");
		_itoa(nvrInfo.m_lPlayHandle, debugInfo + strlen(debugInfo), 10);
		strcpy(debugInfo+strlen(debugInfo), "通道号:");
		
		write_log_file("Debug.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
	}
	if (nvrInfo.m_lPlayHandle < 0)
	{
		return NET_DVR_GetLastError();
	}
	return nvrInfo.m_lPlayHandle;

}
SPLATE_API int SP_InitAlg(TH_PlateIDCfg *th_plateIDCfg)
{
	memcpy(&th_PlateIDCfg, th_plateIDCfg, sizeof(TH_PlateIDCfg));
	int ret = TH_InitPlateIDSDK(&th_PlateIDCfg);
	if (ret != 0)
	{
		return ret;
	} 
	
	TH_SetImageFormat(ImageFormatYV12, false, false, &th_PlateIDCfg);
	TH_SetRecogThreshold(5, 2, &th_PlateIDCfg);//设置阈值 

	return ret;
}
SPLATE_API int SP_GetCarCount()
{
	return nCurCarCount;
}
SPLATE_API int SP_GetFirstCarInfo(CarInfoOut *carinfo)
{
	if (nCurCarCount > 0)
	{
		memcpy(carinfo, &carInfoOut[nCurGetIndex], sizeof(CarInfoOut));
		nCurCarCount--;
		if (nCurGetIndex++ == MAX_CAR_COUNT)
			nCurGetIndex = 0;
		return SUCCESS;
	}
	return FAIL;
}

SPLATE_API int SP_GetCarInfo(CarInfoOut *carinfo, int carCount)
{
	if (carCount > nCurCarCount || carCount <= 0)
		return INVALID_CAR_COUNT;
	int size = sizeof(CarInfoOut);
	for (int i = 0;i<carCount;i++)
	{
		memcpy(carinfo + size*i, &carInfoOut[nCurGetIndex], size);
		nCurCarCount--;
		if (nCurGetIndex++ == MAX_CAR_COUNT)
			nCurGetIndex = 0;
	}
	return SUCCESS;
}
SPLATE_API int SP_GetNvrStatus()
{
	return nvrInfo.m_lServerID;
}
SPLATE_API int SP_SetLogLevel(int loglevel)
{
	nLogLevel = loglevel;
	return SUCCESS;
}
SPLATE_API int SP_TestAPI()
{
	write_log_file("time.txt", MAX_FILE_SIZE, "begin change channel", 20);
	NET_DVR_StopRealPlay(nvrInfo.m_lPlayHandle);
	write_log_file("time.txt", MAX_FILE_SIZE, "StopRealPlay", 12);
	if (++nCurVideoChan >= nVideoChanCount)
	{
		nCurVideoChan = 0;
	}
	previewInfo.lChannel = videoChan[nCurVideoChan];
	nvrInfo.m_lPlayHandle = NET_DVR_RealPlay_V40(nvrInfo.m_lServerID, &previewInfo, RealDataCallBack, nullptr);
	write_log_file("time.txt", MAX_FILE_SIZE, "end", 3);
	//FILE *pFile = fopen("C:\\test01.txt", "r");
// 	fseek(pFile, 0, SEEK_END); //把指针移动到文件的结尾 ，获取文件长度
// 	int len = ftell(pFile); //获取文件长度
// 	rewind(pFile); //把指针移动到文件开头 因为我们一开始把指针移动到结尾，如果不移动回来 会出错
	/*for (int i=0;i<MAX_CAR_COUNT;i++)
	{
		char *license = "京A12345";
		memcpy(carInfoOut[i].license,license,strlen(license));
		carInfoOut[i].nConfidence = i;
		carInfoOut[i].nPicLenth = 2000+i;
		fread(carInfoOut[i].pic, sizeof(char), 6220800, pFile);
	}
	nCurCarCount = MAX_CAR_COUNT;
	
	return 0;*/
	return 0;
}
bool YV12_to_RGB24(unsigned char* pYV12, unsigned char* pRGB24, int iWidth, int iHeight)
{
	if (!pYV12 || !pRGB24)
		return false;

	const long nYLen = long(iHeight * iWidth);
	const int nHfWidth = (iWidth >> 1);

	if (nYLen < 1 || nHfWidth < 1)
		return false;

	unsigned char* yData = pYV12;
	unsigned char* vData = &yData[nYLen];
	unsigned char* uData = &vData[nYLen >> 2];
	if (!uData || !vData)
		return false;

	int rgb[3];
	int i, j, m, n, x, y;
	m = -iWidth;
	n = -nHfWidth;
	for (y = 0; y < iHeight; y++)
	{
		m += iWidth;

		if (!(y % 2))
			n += nHfWidth;

		for (x = 0; x < iWidth; x++)
		{
			i = m + x;
			j = n + (x >> 1);
			rgb[2] = int(yData[i] + 1.370705 * (vData[j] - 128)); // r分量值  
			rgb[1] = int(yData[i] - 0.698001 * (uData[j] - 128) - 0.703125 * (vData[j] - 128)); // g分量值  
			rgb[0] = int(yData[i] + 1.732446 * (uData[j] - 128)); // b分量值  
			j = nYLen - iWidth - m + x;
			i = (j << 1) + j;
			for (j = 0; j < 3; j++)
			{
				if (rgb[j] >= 0 && rgb[j] <= 255)
					pRGB24[i + j] = rgb[j];
				else
					pRGB24[i + j] = (rgb[j] < 0) ? 0 : 255;
			}
		}
	}

	return true;
}
void CALLBACK DecCBFun(long nPort, char *pBuf, long nSize, FRAME_INFO * pFrameInfo, long nReserved1, long nReserved2)
{
	if (nLogLevel >= 4)
	{
		memset(debugInfo, 0, sizeof(debugInfo));
		strcpy(debugInfo, "宽度");
		_itoa(pFrameInfo->nWidth, debugInfo + strlen(debugInfo), 10);
		strcpy(debugInfo + strlen(debugInfo), " 高度");
		_itoa(pFrameInfo->nHeight, debugInfo + strlen(debugInfo), 10);
		strcpy(debugInfo + strlen(debugInfo), " 视频格式");
		_itoa(pFrameInfo->nType, debugInfo + strlen(debugInfo), 10);
		write_log_file("DecCBFun.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
	}
	long lFrameType = pFrameInfo->nType;
	
	if (lFrameType == T_YV12)
	{
		nCarNum = 1;
		unsigned char * p = (unsigned char *)pBuf;
		const unsigned char *pp = (const unsigned char *)p;
		int ret = TH_RecogImage(pp, pFrameInfo->nWidth, pFrameInfo->nHeight, recogResult, &nCarNum, &th_RECT, &th_PlateIDCfg);
		if (nCarNum > 0)
		{
			NET_DVR_StopRealPlay(nvrInfo.m_lPlayHandle);
			for (int i =0;i<nCarNum;i++)
			{

				if (nLogLevel >= 2)
				{
					memset(debugInfo, 0, sizeof(debugInfo));
					strcpy(debugInfo, "当前识别结果：");
					memcpy(debugInfo+strlen(debugInfo), recogResult[i].license, strlen(recogResult[i].license));
					write_log_file("license.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
				}

				memcpy(carInfoOut[nCurPutIndex].license,recogResult[i].license,16);
				memcpy(carInfoOut[nCurPutIndex].color, recogResult[i].color, 16);
				memcpy(carInfoOut[nCurPutIndex].pic, pBuf, nSize);
				carInfoOut[nCurPutIndex].nCarColor = recogResult[i].nCarColor;
				carInfoOut[nCurPutIndex].nCarLogo = recogResult[i].nCarLogo;
				carInfoOut[nCurPutIndex].nCarType = recogResult[i].nCarType;
				carInfoOut[nCurPutIndex].nColor = recogResult[i].nColor;
				carInfoOut[nCurPutIndex].nConfidence = recogResult[i].nConfidence;
				carInfoOut[nCurPutIndex].nPicLenth = nSize;
				carInfoOut[nCurPutIndex].nVideoChannel = nCurVideoChan;
				carInfoOut[nCurPutIndex].nPicType = T_YV12;
				carInfoOut[nCurPutIndex].nType = recogResult[i].nType;
				nCurPutIndex++;
			}
			if (nLogLevel >= 3)
			{
				memset(debugInfo, 0, sizeof(debugInfo));
				strcpy(debugInfo, "当前视频通道：");
				_itoa(nCurVideoChan, debugInfo, 10);
				write_log_file("DecCBFun.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
			}
			
			if (++nCurVideoChan >= nVideoChanCount)
			{
				nCurVideoChan = 0;
			}
			previewInfo.lChannel = videoChan[nCurVideoChan];
			nvrInfo.m_lPlayHandle = NET_DVR_RealPlay_V40(nvrInfo.m_lServerID, &previewInfo, RealDataCallBack, nullptr);
		}
		
	}
	else
	{
		char *errorInfo = "error midea type";
		write_log_file("error.txt", MAX_FILE_SIZE, errorInfo, strlen(errorInfo));
	}


}
void CALLBACK RealDataCallBack(LONG lPlayHandle, DWORD dwDataType, BYTE *pBuffer, DWORD dwBufSize, void *pUser)
{
	DWORD dRet;

	switch (dwDataType)
	{
	case NET_DVR_SYSHEAD:    //系统头
		if (nLogLevel >= 3)
		{
			memset(debugInfo, 0, sizeof(debugInfo));
			strcpy(debugInfo, "RealDataCallBack-> NET_DVR_SYSHEAD dwBufSize:");
			_itoa(dwBufSize, debugInfo + strlen(debugInfo), 10);
			write_log_file("time.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
		}
		if (nPort < 0)
		{
			if (!PlayM4_GetPort(&nPort)) //获取播放库未使用的通道号
			{
				break;
			}
		}
		//nPort = -1;
		
		if (dwBufSize > 0)
		{
			if (!PlayM4_OpenStream(nPort, pBuffer, dwBufSize, 1024 * 1024))
			{
				dRet = PlayM4_GetLastError(nPort);
				break;
			}
			//设置解码回调函数 只解码不显示
			if (!PlayM4_SetDecCallBack(nPort, DecCBFun))
			{
				dRet = PlayM4_GetLastError(nPort);
				break;
			}



			//打开视频解码
			if (!PlayM4_Play(nPort, nullptr))
			{
				dRet = PlayM4_GetLastError(nPort);
				break;
			}

		}
		break;

	case NET_DVR_STREAMDATA:   //码流数据
		if (nLogLevel >= 4)
		{
			memset(debugInfo, 0, sizeof(debugInfo));
			strcpy(debugInfo, "RealDataCallBack-> NET_DVR_STREAMDATA dwBufSize:");
			_itoa(dwBufSize, debugInfo + strlen(debugInfo), 10);
			write_log_file("Debug.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
		}
		if (dwBufSize > 0 && nPort != -1)
		{
			if (nLogLevel >= 4)
			{
				memset(debugInfo, 0, sizeof(debugInfo));
				strcpy(debugInfo, "RealDataCallBack-> PlayM4_InputData");
				write_log_file("Debug.txt", MAX_FILE_SIZE, debugInfo, strlen(debugInfo));
			}
			BOOL inData = PlayM4_InputData(nPort, pBuffer, dwBufSize);
			while (!inData)
			{
				Sleep(10);
				inData = PlayM4_InputData(nPort, pBuffer, dwBufSize);
			}
		}
		break;
	}



}


