using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 清除指纹设备中保存的指纹信息
	/// </summary>
	public class fgClearFingerprint : fgCommandCore
	{
		public fgClearFingerprint( )
		{
			this.commandData = new byte[ ] { 0x05, 0x00, 0x00, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}

	}
}
