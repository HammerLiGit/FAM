using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 上传指定编号的指纹特征值
	/// </summary>
	public class fgUploadEigenvalue : fgCommandCore
	{
		private ushort _dataPackageLen = 0;
		public ushort ID { get; set; }
		public byte Authorize { get; set; }

		public byte[ ] Eigenvalue { get; set; }

		public fgUploadEigenvalue( ushort id )
		{
			this.answerIncludeDataPackage = true;

			ID = id;
			this.commandData = new byte[ ] { 0x31, ( byte )( id >> 8 ), ( byte )id, 0x00, 0x00 };
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

			ushort id = ( ushort )( ( data[1] << 8 ) + data[2] );
			if ( id != ID ) throw new CommonResult( ResultCode.FG_DATA_PACKAGEFAIL );

			Authorize = data[3];

			// 清除列表
			Eigenvalue = new byte[_dataPackageLen - 3];

			// 读取图像信息
			Array.Copy( data, 4, Eigenvalue, 0, _dataPackageLen - 3 );
		}

	}
}
