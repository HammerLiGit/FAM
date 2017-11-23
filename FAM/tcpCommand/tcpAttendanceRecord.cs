using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHR.PAM.Command
{
	public class tcpAttendanceRecord : tcpCommandCore
	{
		#region Inherited

		/// <summary>
		/// Override ToString method
		/// </summary>
		/// <returns></returns>
		public override string ToString( )
		{
			return base.ToString( ) + " - cAttendanceRecord count " + Records.Count.ToString( );
		}

		/// <summary>
		/// 重载的方法，将 cHandshake 命令转化为二进制数据
		/// </summary>
		/// <returns></returns>
		public override byte[ ] ToBinary( )
		{
			List<byte> ret = new List<byte>( );

			ret.AddRange( BitConverter.GetBytes( Convert.ToUInt32( Records.Count ) ) );

			for ( int i = 0; i < Records.Count; i++ )
			{
				ret.AddRange( tcpCommandCore.CStr2Bin( Records[i].FID, 10 ) );

				ret.AddRange( tcpCommandCore.CStr2Bin( Records[i].PID, 10 ) );

				ret.AddRange( tcpCommandCore.CDate2Bin( Records[i].RecordDate ) );
			}

			return GenerateBinary( ret.ToArray( ) );
		}

		/// <summary>
		/// 从 byte 数组中提取数据生成 cHandshake 对象
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public new static tcpAttendanceRecord Parse( byte[ ] data )
		{
			tcpAttendanceRecord ret = new tcpAttendanceRecord( )
			{
				_id = BitConverter.ToInt16( data, 0 ),
				_number = BitConverter.ToInt32( data, 2 )
			};

			ret.Records = new AttendanceRecordList( );
			UInt32 count = BitConverter.ToUInt32( data, 6 );

			int idx = 10;
			for ( int i = 0; i < count; i++ )
			{
				ret.Records.Add(
					new AttendanceRecord( )
					{
						FID = tcpCommandCore.CBin2Str( data, 28 * i + idx, 10 ).Trim( ),
						PID = tcpCommandCore.CBin2Str( data, 28 * i + idx + 10, 10 ).Trim( ),
						RecordDate = tcpCommandCore.CBin2Date( data, 28 * i + idx + 20 )
					}
				);
			}
			return ret;
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="requester">请求方类型</param>
		/// <param name="ipaddress">IP 地址</param>
		/// <param name="port">通信端口</param>
		public tcpAttendanceRecord( )
		{
			this._id = 70;
		}

		#endregion

		#region Properties

		public AttendanceRecordList Records { get; set; }

		#endregion
	}

	public struct AttendanceRecord
	{
		/// <summary>
		/// 10字符长度
		/// </summary>
		public string FID { get; set; }

		/// <summary>
		/// 10字符长度
		/// </summary>
		public string PID { get; set; }

		/// <summary>
		/// 日期型，转二进制 8 字节长度
		/// </summary>
		public DateTime RecordDate { get; set; }
	}

	public class AttendanceRecordList : List<AttendanceRecord> { };
}
