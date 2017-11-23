using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHR.PAM.Common
{
	public class CommonResult : Exception
	{
		private static string GetMessage( ResultCode code )
		{
			switch ( code )
			{
				case ResultCode.GN_SUCCESS: return "成功。";
				case ResultCode.GN_SYS_ERROR: return "";
				case ResultCode.GN_FILE_ATTENFOLDER_CREATEFAIL: return "创建考勤记录数据文件夹失败。";
				case ResultCode.GN_FILE_ATTENFILE_CREATEFAIL: return "创建考勤记录数据文件失败。";
				case ResultCode.GN_FILE_ATTENDANCE_RECORDFAIL: return "写入考勤记录失败。";
				case ResultCode.GN_FILE_PERFILE_CREATEFAIL: return "创建人员信息数据文件失败。";
				case ResultCode.GN_FILE_PERFILE_RECORDFAIL: return "写入人员信息数据失败。";
				case ResultCode.GN_TASK_CANCEL: return "任务取消。";
				case ResultCode.GN_TASK_RUNNING: return "任务正在执行。";

				case ResultCode.FG_PORT_NOTINIT: return "串行端口尚未初始化。";
				case ResultCode.FG_PORT_READSTREAM_NOTINIT: return "串行端口读取数据流尚未初始化。";
				case ResultCode.FG_PORT_WRITESTREAM_NOTINIT: return "串行端口写入数据流尚未初始化。";

				case ResultCode.FG_DATA_NOTINIT: return "数据包未初始化。";
				case ResultCode.FG_DATA_PACKAGEFAIL: return "数据包错误。";

				case ResultCode.FG_BOARD_FAIL: return "操作失败。";
				case ResultCode.FG_BOARD_POWERON: return "电源开起。";
				case ResultCode.FG_BOARD_POWEROFF: return "电源关闭。";
				case ResultCode.FG_BOARD_FULL: return "指纹存储已满。";
				case ResultCode.FG_BOARD_NOFOUND: return "未发现用户。";
				case ResultCode.FG_BOARD_EXISTS: return "用户已经存在。";
				case ResultCode.FG_BOARD_FIN_OPO: return "指纹已经存在。";
				case ResultCode.FG_BOARD_TIMEOUT: return "采集指纹超时。";
				case ResultCode.FG_BOARD_NOANSWER: return "无应答。";

				case ResultCode.FG_GPIO_INITFAIL: return "通用控制设备初始化失败。";
				case ResultCode.FG_GPIO_NOTINIT: return "通用控制设备未初始化。";

				default: return "未知错误。";
			}
		}

		private ResultCode _code;

		public ResultCode Code { get { return _code; } }

		public CommonResult( ResultCode code )
			: base( GetMessage( code ) )
		{ _code = code; }

		public CommonResult( ResultCode code, string Message )
			: base( Message )
		{ _code = code; }
	}

	public enum ResultCode
	{
		GN_SUCCESS = 0x00,
		GN_SYS_ERROR = 0x01,
		GN_SYS_UNKNOWN = 0x02,
		GN_FILE_ATTENFOLDER_CREATEFAIL = 0x03,
		GN_FILE_ATTENFILE_CREATEFAIL = 0x04,
		GN_FILE_ATTENDANCE_RECORDFAIL = 0x05,
		GN_FILE_PERFILE_CREATEFAIL = 0x08,
		GN_FILE_PERFILE_RECORDFAIL = 0x09,
		GN_TASK_CANCEL = 0x06,
		GN_TASK_RUNNING = 0x07,

		FG_PORT_NOTINIT = 0x11,
		FG_PORT_WRITESTREAM_NOTINIT = 0x12,
		FG_PORT_READSTREAM_NOTINIT = 0x13,

		FG_DATA_NOTINIT = 0x21,
		FG_DATA_PACKAGEFAIL = 0X22,

		FG_BOARD_FAIL = 0x31,
		FG_BOARD_POWERON = 0x32,
		FG_BOARD_POWEROFF = 0x33,
		FG_BOARD_FULL = 0x34,
		FG_BOARD_NOFOUND = 0x35,
		FG_BOARD_EXISTS = 0x36,
		FG_BOARD_FIN_OPO = 0x37,
		FG_BOARD_TIMEOUT = 0x38,
		FG_BOARD_NOANSWER = 0x39,

		FG_GPIO_INITFAIL = 0x40,
		FG_GPIO_NOTINIT = 0x41
	}
}
