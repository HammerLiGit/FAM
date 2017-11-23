using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 获取比对等级设定，1~10级，数字越高比对越严格
	/// </summary>
	public class fgGetCompareLevel : fgCommandCore
	{
		public byte CurrentCompareLevel { get; set; }

		public fgGetCompareLevel( )
		{
			this.commandData = new byte[ ] { 0x28, 0x00, 0x00, 0x01, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			this.CurrentCompareLevel = data[3];
			return this.ConvertResult( data[4] );
		}

	}
}
