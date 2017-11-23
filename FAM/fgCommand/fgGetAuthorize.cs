using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 获取指纹授权信息
	/// </summary>
	public class fgGetAuthorize : fgCommandCore
	{
		public byte Authorize { get; set; }

		public fgGetAuthorize( ushort id )
		{
			this.commandData = new byte[ ] { 0x0A, ( byte )( id >> 8 ), ( byte )( id ), 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{

			if ( ( data[4] == ( byte )fgExecuteResult.ACK_NOFOUND ) )
				return this.ConvertResult( data[4] );
			else
			{
				this.Authorize = data[4];
				return this.ConvertResult( ( byte )fgExecuteResult.ACK_SUCCESS );
			}
		}

	}
}
