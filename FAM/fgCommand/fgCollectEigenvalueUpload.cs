using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 采集指纹特征值并上传
	/// </summary>
	public class fgCollectEigenvalueUpload : fgCommandCore
	{
		private ushort _dataPackageLen = 0;

		public byte[ ] Eigenvalue { get; set; }

		public fgCollectEigenvalueUpload( )
		{
			this.answerIncludeDataPackage = true;
			this.commandData = new byte[ ] { 0x23, 0x00, 0x00, 0x00, 0x00 };
		}

		internal override CommonResult DispathResult( byte[ ] data )
		{
			_dataPackageLen = ( ushort )( ( data[2] << 8 ) + data[3] );
			return this.ConvertResult( data[4] );
		}

		internal override ushort GetDataPackageLen( )
		{ return _dataPackageLen; }
		
		internal override void DispathDataPackage( byte[ ] data )
		{
			// 检查数据包长度
			if ( data.Length != _dataPackageLen + 3 ) throw new CommonResult( ResultCode.FG_DATA_PACKAGEFAIL );

			// 清除列表
			Eigenvalue = new byte[_dataPackageLen - 3];

			// 读取图像信息
			Array.Copy( data, 4, Eigenvalue, 0, _dataPackageLen - 3 );
		}


	}
}
