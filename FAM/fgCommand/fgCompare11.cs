using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 采集指纹与设备中保存的指定编号的指纹信息进行 1 对 1 比对
	/// 比对结果为成功或者失败
	/// </summary>
	public class fgCompare11 : fgCommandCore
	{
		public fgCompare11( ushort id )
		{
			this.commandData = new byte[ ] { 0x0B, ( byte )( id >> 8 ), ( byte )( id ), 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}

	}
}
