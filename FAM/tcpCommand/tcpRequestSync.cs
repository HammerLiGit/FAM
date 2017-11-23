using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHR.PAM.Command
{
	public class tcpRequestSync : tcpCommandCore
	{
		protected DateTime? _commandTime = null;
		private string _adrFileFormat = "yyyy-MM-dd";

		#region Inherited

		/// <summary>
		/// Override ToString method
		/// </summary>
		/// <returns></returns>
		public override string ToString( )
		{
			return base.ToString( ) + " - cGetPersonnelList < time: " + Convert.ToDateTime( CommandTime ).ToString( "HH:mm:ss.fff" ) + " request content: " + ( RequestContent == ContentType.PersonnelInfo ? "Personnel info" : "Attendance record" ) + " > ";
		}

		/// <summary>
		/// 重载的方法，将 cHandshake 命令转化为二进制数据
		/// </summary>
		/// <returns></returns>
		public override byte[ ] ToBinary( )
		{
			List<byte> ret = new List<byte>( );

			ret.AddRange( tcpCommandCore.CDate2Bin( DateTime.Now ) );
			ret.AddRange( BitConverter.GetBytes( ( int )RequestContent ) );

			if ( RequestContent == ContentType.AttendanceRecord )
			{
				ret.AddRange( tcpCommandCore.CStr2Bin(
					AttendanceRecordRange.StartDate == null ? "            " : ( ( DateTime )AttendanceRecordRange.StartDate ).ToString( _adrFileFormat ), 10
				) );
				ret.AddRange( tcpCommandCore.CStr2Bin(
					AttendanceRecordRange.EndDate == null ? "            " : ( ( DateTime )AttendanceRecordRange.StartDate ).ToString( _adrFileFormat ), 10
				) );
			}

			return GenerateBinary( ret.ToArray( ) );
		}

		/// <summary>
		/// 从 byte 数组中提取数据生成 cHandshake 对象
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public new static tcpRequestSync Parse( byte[ ] data )
		{
			tcpRequestSync ret = new tcpRequestSync( )
			{
				_id = BitConverter.ToInt16( data, 0 ),
				_number = BitConverter.ToInt32( data, 2 ),
				_commandTime = tcpCommandCore.CBin2Date( data, 6 ),
				RequestContent = ( ContentType )BitConverter.ToInt32( data, 14 )
			};

			if ( ret.RequestContent == ContentType.AttendanceRecord )
			{
				string s = tcpCommandCore.CBin2Str( data, 18, 10 );
				string e = tcpCommandCore.CBin2Str( data, 28, 10 );

				ret.AttendanceRecordRange = new RecordDateRange( )
				{
					StartDate = s.Trim( ) == "" ? null : DateTime.Parse( s ) as DateTime?,
					EndDate = e.Trim( ) == "" ? null : DateTime.Parse( e ) as DateTime?
				};
			}

			return ret;
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="requester">请求方类型</param>
		/// <param name="ipaddress">IP 地址</param>
		/// <param name="port">通信端口</param>
		public tcpRequestSync( ContentType type )
		{
			this._id = 71;
			this.RequestContent = type;
		}
		private tcpRequestSync( )
		{
			this._id = 71;
			this.RequestContent = ContentType.PersonnelInfo;
		}

		#endregion

		#region Properties

		public DateTime? CommandTime
		{
			get { return _commandTime; }
		}

		public ContentType RequestContent { get; set; }

		public RecordDateRange AttendanceRecordRange { get; set; }

		#endregion

	}

	public enum ContentType : Int32
	{
		PersonnelInfo,
		AttendanceRecord
	}

	public struct RecordDateRange
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}
