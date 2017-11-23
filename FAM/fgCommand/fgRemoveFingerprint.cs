using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 从设备中删除指定编号的指纹信息
	/// </summary>
	public class fgRemoveFingerprint : fgCommandCore
	{
		public fgRemoveFingerprint( ushort id )
		{
			this.commandData = new byte[ ] { 0x04, ( byte )( id >> 8 ), ( byte )( id ), 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}

	}
}
