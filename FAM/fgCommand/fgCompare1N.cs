using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 采集指纹与设备中保存的所有指纹信息进行 1 对 N 比对
	/// 比对成功则返回指纹编号，否则返回失败
	/// </summary>
	public class fgCompare1N : fgCommandCore
	{

		public ushort ID { get; set; }

		public byte Authorize { get; set; }

		public fgCompare1N( )
		{
			this.commandData = new byte[ ] { 0x0C, 0x00, 0x00, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			if ( ( data[4] == ( byte )fgExecuteResult.ACK_NOFOUND ) || ( data[4] == ( byte )fgExecuteResult.ACK_TIMEOUT ) )
				return this.ConvertResult( data[4] );
			else
			{
				this.ID = ( ushort )( ( data[2] << 8 ) + data[3] );
				this.Authorize = data[4];
				return this.ConvertResult( ( byte )fgExecuteResult.ACK_SUCCESS );
			}
		}

	}
}
