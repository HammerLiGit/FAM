using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHR.PAM.Command
{
	public class tcpHeartbeat : tcpCommandCore
	{

		#region Inherited

		/// <summary>
		/// Override ToString method
		/// </summary>
		/// <returns></returns>
		public override string ToString( )
		{
			return base.ToString( ) + " - Heartbeat < time: " + Convert.ToDateTime( CommandTime ).ToString( "HH:mm:ss.fff" ) + " >";
		}

		/// <summary>
		/// 重载的方法，将 cHandshake 命令转化为二进制数据
		/// </summary>
		/// <returns></returns>
		public override byte[ ] ToBinary( )
		{
			return GenerateBinary( tcpCommandCore.CDate2Bin( DateTime.Now ) );
		}

		/// <summary>
		/// 从 byte 数组中提取数据生成 cHandshake 对象
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public new static tcpHeartbeat Parse( byte[ ] data )
		{
			return new tcpHeartbeat( )
			{
				_id = BitConverter.ToInt16( data, 0 ),
				_number = BitConverter.ToInt32( data, 2 ),
				_commandTime = tcpCommandCore.CBin2Date( data, 6 )
			};
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="requester">请求方类型</param>
		/// <param name="ipaddress">IP 地址</param>
		/// <param name="port">通信端口</param>
		public tcpHeartbeat( )
		{
			this._id = 20;
		}

		#endregion

		#region Properties

		protected DateTime? _commandTime = null;
		public DateTime? CommandTime
		{
			get { return _commandTime; }
		}

		#endregion
	}
}
