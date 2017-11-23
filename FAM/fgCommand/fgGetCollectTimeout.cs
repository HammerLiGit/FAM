using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 获取采集超时设定时间
	/// </summary>
	public class fgGetCollectTimeout : fgCommandCore
	{
		public byte CurrentTimeout { get; set; }

		public fgGetCollectTimeout( )
		{
			this.commandData = new byte[ ] { 0x2E, 0x00, 0x00, 0x01, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			this.CurrentTimeout = data[3];
			return this.ConvertResult( data[4] );
		}

	}
}
