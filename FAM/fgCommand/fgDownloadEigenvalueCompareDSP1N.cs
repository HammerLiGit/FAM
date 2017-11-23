using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 下载指纹特征值与设备中保存的所有指纹信息进行 1 对 N 比对
	/// 比对成功则返回指纹编号，否则返回失败
	/// </summary>
	public class fgDownloadEigenvalueCompareDSP1N : fgCommandCore
	{
		public byte[ ] Eigenvalue { get; set; }
		public ushort ID { get; set; }

		public fgDownloadEigenvalueCompareDSP1N( byte[ ] eigenvalue )
		{
			this.commandIncludeDataPackage = true;

			Eigenvalue = eigenvalue;

			ushort len = ( ushort )( Eigenvalue.Length + 3 );
			this.commandData = new byte[ ] { 0x43, ( byte )( len >> 8 ), ( byte )len, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			if ( ( data[4] == ( byte )fgExecuteResult.ACK_NOFOUND ) || ( data[4] == ( byte )fgExecuteResult.ACK_TIMEOUT ) )
				return this.ConvertResult( data[4] );
			else
			{
				this.ID = ( ushort )( ( data[2] << 8 ) + data[3] );
				return this.ConvertResult( ( byte )fgExecuteResult.ACK_SUCCESS );
			}
		}

		protected override byte[ ] GetCommandDataPackage( )
		{
			byte[ ] ret = new byte[Eigenvalue.Length + 3];
			Array.Copy( Eigenvalue, 0, ret, 3, Eigenvalue.Length );

			return ret;
		}


	}
}
