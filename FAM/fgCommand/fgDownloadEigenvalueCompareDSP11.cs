using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 下载指纹特征值与设备中保存的指定编号的指纹信息进行 1 对 1 比对
	/// 比对结果为成功或者失败
	/// </summary>
	public class fgDownloadEigenvalueCompareDSP11 : fgCommandCore
	{
		public byte[ ] Eigenvalue { get; set; }
		public ushort ID { get; set; }

		public fgDownloadEigenvalueCompareDSP11( byte[ ] eigenvalue, ushort id )
		{
			this.commandIncludeDataPackage = true;

			Eigenvalue = eigenvalue;
			ID = id;

			ushort len = ( ushort )( Eigenvalue.Length + 3 );
			this.commandData = new byte[ ] { 0x42, ( byte )( len >> 8 ), ( byte )len, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}

		protected override byte[ ] GetCommandDataPackage( )
		{
			byte[ ] ret = new byte[Eigenvalue.Length + 3];
			ret[0] = ( byte )( ID >> 8 );
			ret[1] = ( byte )ID;

			Array.Copy( Eigenvalue, 0, ret, 3, Eigenvalue.Length );

			return ret;
		}

	}
}
