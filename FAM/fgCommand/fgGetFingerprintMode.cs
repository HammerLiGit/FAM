using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 获取采集指纹规则，Duplicates 指纹可重复：Unique 指纹必须唯一
	/// </summary>
	public class fgGetFingerprintMode : fgCommandCore
	{
		public FingerprintMode CurrentMode { get; set; }

		public fgGetFingerprintMode( )
		{
			this.commandData = new byte[ ] { 0x2D, 0x00, 0x00, 0x01, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			this.CurrentMode = ( FingerprintMode )data[3];
			return this.ConvertResult( data[4] );
		}

	}
}
