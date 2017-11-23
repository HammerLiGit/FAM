using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 下载指纹特征值到设备
	/// </summary>
	public class fgDownloadEigenvalue : fgCommandCore
	{
		public byte[ ] Eigenvalue { get; set; }
		public ushort ID { get; set; }
		public byte Authorize { get; set; }

		public fgDownloadEigenvalue( byte[ ] eigenvalue, ushort id, byte authorize )
		{
			this.commandIncludeDataPackage = true;

			Eigenvalue = eigenvalue;
			ID = id;
			Authorize = authorize;

			ushort len = ( ushort )( Eigenvalue.Length + 3 );
			this.commandData = new byte[ ] { 0x41, ( byte )( len >> 8 ), ( byte )len, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			if ( data[4] == ( byte )fgExecuteResult.ACK_FAIL )
				return this.ConvertResult( data[4] );
			else
			{
				ushort id = ( ushort )( ( data[2] << 8 ) + data[3] );
				if ( id == ID )
					return this.ConvertResult( ( byte )fgExecuteResult.ACK_SUCCESS );
				else
					return new Common.CommonResult( Common.ResultCode.FG_DATA_PACKAGEFAIL );
			}
		}

		protected override byte[ ] GetCommandDataPackage( )
		{
			byte[ ] ret = new byte[Eigenvalue.Length + 3];
			ret[0] = ( byte )( ID >> 8 );
			ret[1] = ( byte )ID;
			ret[2] = Authorize;

			Array.Copy( Eigenvalue, 0, ret, 3, Eigenvalue.Length );

			return ret;
		}
	}
}
