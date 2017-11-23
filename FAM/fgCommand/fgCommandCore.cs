using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PHR.PAM.Common;
using Windows.Devices.SerialCommunication;

namespace PHR.PAM.fgCommand
{
	/// <summary>
	/// 指纹设备命令基础对象
	/// </summary>
	public class fgCommandCore
	{
		/// <summary>
		/// 命令字内容
		/// </summary>
		protected byte[ ] commandData;
		protected bool answerIncludeDataPackage = false;
		protected bool commandIncludeDataPackage = false;


		#region Internal method

		/// <summary>
		/// 根据给定的命令内容，增加包头、效验字、包尾，生成最终发送数据包
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		protected virtual byte[ ] BuildData( byte[ ] data ) //
		{
			// Format: 0xF5 ... data ... CHK 0xF5

			// Check data
			if ( data.Length <= 0 ) return null;

			// Calc chk
			byte chk = data[0];
			for ( int i = 1; i < data.Length; i++ )
				chk = ( byte )( chk ^ data[i] );

			// Build return data
			List<byte> ret = new List<byte>( );
			ret.Add( 0xF5 );
			ret.AddRange( data );
			ret.Add( chk );
			ret.Add( 0xF5 );

			// Return
			return ret.ToArray( );
		}

		internal virtual byte[ ] BuildCommandData( )
		{
			return BuildData( commandData );
		}

		internal virtual byte[ ] BuildCommandDataWithPackage( )
		{
			byte[ ] body = this.BuildCommandData( );
			byte[ ] pakg = this.BuildPackageData( );
			byte[ ] data = new byte[body.Length + pakg.Length];
			Array.Copy( body, 0, data, 0, body.Length );
			Array.Copy( pakg, 0, data, body.Length, pakg.Length );

			return data;
		}

		internal virtual byte[ ] BuildPackageData( )
		{
			return BuildData( GetCommandDataPackage( ) );
		}


		/// <summary>
		/// 检查返回的数据包是否符合命令格式
		/// </summary>
		/// <param name="data"></param>
		internal virtual void CheckReceiveData( byte[ ] data )
		{
			if ( ( data[0] != 0xF5 ) || ( data[data.Length - 1] != 0xF5 ) ) throw new CommonResult( ResultCode.FG_DATA_PACKAGEFAIL );

			byte chk = data[1];
			for ( int i = 2; i < ( data.Length - 2 ); i++ )
				chk = ( byte )( chk ^ data[i] );
			if ( data[data.Length - 2] != chk ) throw new CommonResult( ResultCode.FG_DATA_PACKAGEFAIL );
		}

		/// <summary>
		/// 检查返回的数据应答是否为本命令的应答，通过命令号进行区别
		/// </summary>
		/// <param name="andwerData"></param>
		/// <returns></returns>
		internal virtual bool CheckCommandAnswer( byte[ ] andwerData )
		{
			return andwerData[1] == commandData[0];
		}

		/// <summary>
		/// 对命令执行的返回信息进行处理，虚方法，在每个派生类中根据具体内容进行处理
		/// </summary>
		/// <param name="data"></param>
		internal virtual CommonResult DispathResult( byte[ ] data ) { throw new CommonResult( ResultCode.FG_DATA_NOTINIT ); }

		/// <summary>
		/// 对读取的数据包进行处理
		/// </summary>
		/// <param name="data"></param>
		internal virtual void DispathDataPackage( byte[ ] data ) { }


		/// <summary>
		/// 获取数据包长度用于读取数据包
		/// </summary>
		/// <returns></returns>
		internal virtual ushort GetDataPackageLen( ) { throw new Exception( "Not override" ); }

		/// <summary>
		/// 获取命令附加数据包
		/// 注意！这个函数返回的应该是特征值通讯使用的数据包，即 0x00 0x00 0x00 { 193byte data }，总长应该是 196 字节
		/// </summary>
		/// <returns></returns>
		protected virtual byte[ ] GetCommandDataPackage( ) { throw new Exception( "Not override" ); }

		#endregion

		#region Publish method

		/// <summary>
		/// 根据返回的成功标志位，转化为系统识别的结果信息
		/// </summary>
		/// <param name="ret"></param>
		/// <returns></returns>
		public CommonResult ConvertResult( byte ret )
		{
			switch ( ( fgExecuteResult )ret )
			{
				case fgExecuteResult.ACK_SUCCESS: return new CommonResult( ResultCode.GN_SUCCESS );
				case fgExecuteResult.ACK_FAIL: return new CommonResult( ResultCode.FG_BOARD_FAIL );
				case fgExecuteResult.ACK_FULL: return new CommonResult( ResultCode.FG_BOARD_FULL );
				case fgExecuteResult.ACK_NOFOUND: return new CommonResult( ResultCode.FG_BOARD_NOFOUND );
				case fgExecuteResult.ACK_EXISTS: return new CommonResult( ResultCode.FG_BOARD_EXISTS );
				case fgExecuteResult.ACK_FIN_OPO: return new CommonResult( ResultCode.FG_BOARD_FIN_OPO );
				case fgExecuteResult.ACK_TIMEOUT: return new CommonResult( ResultCode.FG_BOARD_TIMEOUT );
				default: return new CommonResult( ResultCode.GN_SYS_UNKNOWN );
			}
		}

		#endregion

		#region Publish properties

		public bool AnswerIncludeDataPackage
		{ get { return answerIncludeDataPackage; } }

		public bool CommandIncludeDataPackage
		{ get { return commandIncludeDataPackage; } }

		#endregion
	}

	/// <summary>
	/// 指纹设备执行结果信息
	/// </summary>
	public enum fgExecuteResult
	{
		ACK_SUCCESS = 0x00,
		ACK_FAIL = 0x01,
		ACK_FULL = 0x04,
		ACK_NOFOUND = 0x05,
		ACK_EXISTS = 0x06,
		ACK_FIN_OPO = 0x07,
		ACK_TIMEOUT = 0x08,
	}
}
