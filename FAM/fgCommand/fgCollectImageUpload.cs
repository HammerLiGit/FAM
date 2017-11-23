using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 采集指纹图像上传
	/// </summary>
	public class fgCollectImageUpload : fgCommandCore
	{
		private ushort _dataPackageLen = 0;

		public byte[ ] ImageData { get; set; }

		public fgCollectImageUpload( )
		{
			this.answerIncludeDataPackage = true;
			this.commandData = new byte[ ] { 0x24, 0x00, 0x00, 0x00, 0x00 };
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
			ImageData = new byte[_dataPackageLen];

			// 读取图像信息
			Array.Copy( data, 1, ImageData, 0, _dataPackageLen );
		}

	}
}
