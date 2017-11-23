using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHR.PAM.Command
{
	public class tcpCommandCore
	{
		/// <summary>
		/// 从 byte 数组中提取数据生成对象
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static tcpCommandCore Parse( byte[ ] data )
		{
			if ( data.Length < 7 ) return null;

			switch ( BitConverter.ToInt16( data, 0 ) )
			{
				case 10: return tcpHandshake.Parse( data );
				case 11: return tcpHandshakeAnswer.Parse( data );
				case 20: return tcpHeartbeat.Parse( data );
				case 30: return tcpCloseConnection.Parse( data );
				case 40: return tcpDebugMessage.Parse( data );

				case 70: return tcpAttendanceRecord.Parse( data );
				case 71: return tcpRequestSync.Parse( data );
				case 72: return tcpSyncPersonnel.Parse( data );

				default: return null;
			}
		}

		/// <summary>
		/// 将一个字符串转换为指定长度的 byte 数组
		/// </summary>
		/// <param name="s">要转化字符串</param>
		/// <param name="length">转化后的数组长度</param>
		/// <param name="def">当字符串长度不够时，补充的值</param>
		/// <returns></returns>
		public static byte[ ] GetStringBytes( string s, int length, byte def )
		{
			byte[ ] ret = new byte[length];
			for ( int i = 0; i < ret.Length; i++ ) ret[i] = def;
			if ( ( s != null ) && ( s.Trim( ) != "" ) )
			{
				byte[ ] tmp = System.Text.Encoding.Unicode.GetBytes( s.Substring( 0, s.Length < length ? s.Length : length ) );
				Array.Copy( tmp, ret, tmp.Length );
			}
			return ret;
		}

		internal static byte[ ] CStr2Bin( string s, int len )
		{
			byte[ ] tmp = System.Text.Encoding.Unicode.GetBytes( s );
			byte[ ] ret = new byte[len];

			Array.Copy( tmp, 0, ret, 0, len > tmp.Length ? tmp.Length : len );
			return ret;
		}

		internal static string CBin2Str( byte[ ] data, int sIdx, int len )
		{
			return System.Text.Encoding.ASCII.GetString( data, sIdx, len ).Replace( System.Text.Encoding.ASCII.GetString( new byte[ ] { 0 } ), String.Empty );
		}

		internal static byte[ ] CDate2Bin( DateTime det )
		{
			return BitConverter.GetBytes( det.ToBinary( ) );
		}

		internal static DateTime CBin2Date( byte[ ] data, int sIdx )
		{
			byte[ ] tmp = new byte[8];
			Array.Copy( data, sIdx, tmp, 0, 8 );
			return DateTime.FromBinary( BitConverter.ToInt64( tmp, 0 ) );
		}


		/// <summary>
		/// 产生一个命令号
		/// </summary>
		protected static Int32 GenerateNumber( )
		{
			return Convert.ToInt32( DateTime.Now.ToString( "HHmmssfff" ) );
		}

		public static string ByteArray2Hex( byte[ ] buff )
		{
			string ret = "";
			int idx = 0;
			idx++;
			for ( int i = 0; i < buff.Length; i++ )
			{
				ret = ret + string.Format( "{0,2:X2}", buff[i] );
			}
			return ret;
		}


		/// <summary>
		/// 命令字
		/// </summary>
		protected short _id;
		public short ID { get { return _id; } }

		/// <summary>
		/// 命令号
		/// </summary>
		protected int _number = -1;
		public int Number
		{
			get
			{
				if ( _number < 0 ) _number = tcpCommandCore.GenerateNumber( );
				return _number;
			}
			set { _number = value; }
		}

		/// <summary>
		/// 将命令信息转化为二进制数组
		/// </summary>
		/// <returns></returns>
		public virtual byte[ ] ToBinary( )
		{
			throw new Exception( "Undefined convert method !" );
		}

		/// <summary>
		/// Override ToString method
		/// </summary>
		/// <returns></returns>
		public override string ToString( )
		{
			return "ID = " + ID.ToString( ) + " : Number = " + Number.ToString( );
		}

		/// <summary>
		/// Generate packet send data
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		protected byte[ ] GenerateBinary( byte[ ] content )
		{
			byte[ ] ret = new byte[content.Length + 6];
			Array.Copy( BitConverter.GetBytes( ID ), 0, ret, 0, 2 );
			Array.Copy( BitConverter.GetBytes( Number ), 0, ret, 2, 4 );
			Array.Copy( content, 0, ret, 6, content.Length );

			return ret;
		}


	}
}
