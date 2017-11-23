using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;
using Windows.Devices.SerialCommunication;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 获取设备中保存的指纹数量
	/// </summary>
	public class fgGetCount : fgCommandCore
	{

		public ushort Count { get; set; }

		public fgGetCount( )
		{
			this.commandData = new byte[ ] { 0x09, 0x00, 0x00, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			this.Count = ( ushort )( ( data[2] << 8 ) + data[3] );
			return this.ConvertResult( data[4] );
		}

	}
}
