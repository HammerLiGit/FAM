using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 下载指纹特征值进行比对，应用不明
	/// </summary>
	public class fgDownloadEigenvalueCompare : fgCommandCore
	{

		public byte[ ] Eigenvalue { get; set; }

		public fgDownloadEigenvalueCompare( byte[ ] eigenvalue )
		{
			this.commandIncludeDataPackage = true;
			Eigenvalue = eigenvalue;

			ushort len = ( ushort )( Eigenvalue.Length + 3 );
			this.commandData = new byte[ ] { 0x44, ( byte )( len >> 8 ), ( byte )len, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			return this.ConvertResult( data[4] );
		}

		protected override byte[ ] GetCommandDataPackage( )
		{
			byte[ ] ret = new byte[Eigenvalue.Length + 3];
			Array.Copy( Eigenvalue, 0, ret, 3, Eigenvalue.Length );

			return ret;
		}



	}
}
