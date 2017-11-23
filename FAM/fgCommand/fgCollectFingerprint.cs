using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 采集指纹信息
	/// 调用三次本命令，采集一个完整的指纹并自动保存与设备
	/// </summary>
	public class fgCollectFingerprint : fgCommandCore
	{
		private byte _times = 0;
		private ushort _fpID = 0;
		private byte _fpAuthorize = 0;

		public int Times { get { return _times; } }
		public int ID { get { return _fpID; } }
		public int Authorize { get { return _fpAuthorize; } }

		/// <summary>
		/// 采集命令
		/// </summary>
		/// <param name="times">当前采集的次数，一般添加一个指纹信息需要采集三次</param>
		/// <param name="id">指定的指纹 ID</param>
		/// <param name="authorize">授权标识，目前无用</param>
		public fgCollectFingerprint( byte times, ushort id, byte authorize )
		{
			_times = times;
			_fpID = id;
			_fpAuthorize = authorize;

			this.commandData = new byte[ ] { times, ( byte )( id >> 8 ), ( byte )id, authorize, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}

	}
}
