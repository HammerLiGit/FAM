using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHR.PAM.Command
{
	public class tcpSyncPersonnel : tcpCommandCore
	{
		#region Inherited

		/// <summary>
		/// Override ToString method
		/// </summary>
		/// <returns></returns>
		public override string ToString( )
		{
			return base.ToString( ) + " - cGetPersonnelInfo < Per count:" + PersonnelList.Count.ToString( ) + " >";
		}

		/// <summary>
		/// 重载的方法，将 cHandshake 命令转化为二进制数据
		/// </summary>
		/// <returns></returns>
		public override byte[ ] ToBinary( )
		{
			List<byte> ret = new List<byte>( );

			ret.AddRange( BitConverter.GetBytes( ( Int32 )this.SyncMode ) );

			for ( int i = 0; i < this.PersonnelList.Count; i++ )
			{
				PerInfo pi = PersonnelList[i];

				ret.AddRange( tcpCommandCore.CStr2Bin( pi.PID, 10 ) );

				ret.AddRange( tcpCommandCore.CStr2Bin( pi.FID, 10 ) );

				ret.AddRange( tcpCommandCore.CStr2Bin( pi.Name, 40 ) );

				ret.AddRange( tcpCommandCore.CStr2Bin( pi.Department, 100 ) );

				if ( pi.Eigenvalue == null || pi.Eigenvalue.Length <= 0 )
				{
					ret.Add( 0x00 );
				}
				else
				{
					ret.Add( 0x01 );
					byte[ ] tmp = new byte[193];
					Array.Copy( pi.Eigenvalue, tmp, 193 );
					ret.AddRange( tmp );
				}
			}

			return GenerateBinary( ret.ToArray( ) );
		}

		/// <summary>
		/// 从 byte 数组中提取数据生成 cHandshake 对象
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public new static tcpSyncPersonnel Parse( byte[ ] data )
		{
			tcpSyncPersonnel ret = new tcpSyncPersonnel( )
			{
				_id = BitConverter.ToInt16( data, 0 ),
				_number = BitConverter.ToInt32( data, 2 ),
				SyncMode = ( SyncRange )BitConverter.ToInt32( data, 6 )
			};

			if ( data.Length > 10 )
			{
				int sIdx = 10;
				while ( true )
				{
					PerInfo item = new PerInfo( );

					item.PID = tcpCommandCore.CBin2Str( data, sIdx, 10 );               // 0 .. 9

					item.FID = tcpCommandCore.CBin2Str( data, sIdx + 10, 10 );          // 10 .. 19

					item.Name = tcpCommandCore.CBin2Str( data, sIdx + 20, 40 );         // 20 .. 59

					item.Department = tcpCommandCore.CBin2Str( data, sIdx + 60, 100 );  // 60 .. 159

					if ( data[sIdx + 160] == 0x00 )
					{
						item.Eigenvalue = null;
						sIdx = sIdx + 161;
					}
					else
					{
						item.Eigenvalue = new byte[193];
						Array.Copy( data, sIdx + 161, item.Eigenvalue, 0, 193 );

						sIdx = sIdx + 354;
					}

					ret.PersonnelList.Add( item );


					if ( sIdx >= data.Length ) break;
				}
			}
			return ret;
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="requester">请求方类型</param>
		/// <param name="ipaddress">IP 地址</param>
		/// <param name="port">通信端口</param>
		public tcpSyncPersonnel( )
		{
			this._id = 72;
			this.PersonnelList = new PerInfoList( );
		}

		#endregion

		#region Properties

		public SyncRange SyncMode { get; set; }

		/// <summary>
		/// 10字符长度
		/// </summary>
		public PerInfoList PersonnelList { get; set; }

		#endregion
	}

	public class PerInfo
	{
		public PerInfo( )
		{

		}

		public PerInfo( byte[ ] data, int sInx )
		{
			this.PID = tcpCommandCore.CBin2Str( data, sInx, 10 );

			this.FID = tcpCommandCore.CBin2Str( data, sInx + 10, 10 );

			this.Name = tcpCommandCore.CBin2Str( data, sInx + 20, 40 );

			this.Department = tcpCommandCore.CBin2Str( data, sInx + 60, 100 );

			if ( data[sInx + 160] == 0x00 )
			{
				this.Eigenvalue = null;
			}
			else
			{
				this.Eigenvalue = new byte[193];
				Array.Copy( data, sInx + 161, this.Eigenvalue, 0, 193 );
			}

		}

		/// <summary>
		/// 10 byte
		/// </summary>
		public string PID { get; set; }
		/// <summary>
		/// 10 byte
		/// </summary>
		public string FID { get; set; }
		/// <summary>
		/// 40 byte
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 100 byte
		/// </summary>
		public string Department { get; set; }
		/// <summary>
		/// 193 byte
		/// </summary>
		public byte[ ] Eigenvalue { get; set; }
	}
	public class PerInfoList : List<PerInfo> { }

	public enum SyncRange : Int32
	{
		Single,
		All
	}
}