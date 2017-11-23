using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHR.PAM.Common;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 获取设备中保存的所有指纹信息
	/// </summary>
	public class fgGetAllFingerprint : fgCommandCore
	{
		private ushort _dataPackageLen = 0;

		public Dictionary<UInt16, byte> fpList { get; set; }

		public fgGetAllFingerprint( )
		{
			this.answerIncludeDataPackage = true;
			this.commandData = new byte[ ] { 0x2B, 0x00, 0x00, 0x00, 0x00 };
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
			if ( fpList == null )
				fpList = new Dictionary<ushort, byte>( );
			else
				fpList.Clear( );

			ushort fCount = ( ushort )( ( data[1] << 8 ) + data[2] );

			// 读取用户信息
			for ( int idx = 0; idx < fCount; idx++ )
				fpList.Add( ( ushort )( ( data[idx * 3 + 3] << 8 ) + data[idx * 3 + 4] ), data[idx * 3 + 5] );

		}

	}
}
