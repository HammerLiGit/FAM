using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 休眠命令，休眠后必须给采集传感器重新上电才能再次采集
	/// </summary>
	public class fgHibernate : fgCommandCore
	{
		public fgHibernate( )
		{
			this.commandData = new byte[ ] { 0x2C, 0x00, 0x00, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}
	}
}
