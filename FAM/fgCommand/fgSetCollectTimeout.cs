using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 设置采集超时设定时间，为 0 时采集操作将一直等待到有指纹信息录入
	/// </summary>
	public class fgSetCollectTimeout : fgCommandCore
	{
		public byte CurrentTimeout { get; set; }

		public fgSetCollectTimeout( byte time )
		{
			this.commandData = new byte[ ] { 0x2E, 0x00, time, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			this.CurrentTimeout = data[3];
			return this.ConvertResult( data[4] );
		}

	}
}
