using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using PHR.PAM.Command;
using PHR.PAM.fgCommand;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.ApplicationModel.Resources;

namespace PHR.PAM.Common
{
	public class CommonRes
	{
		/// <summary>
		/// 初始化系统环境
		/// </summary>
		public static async void InitializationResource( )
		{
			// 初始化文件存储
			await StorageControl.InitializationFileAccess( );
			await StorageControl.LoadConfig( );

			// 初始化串行设备
			await SerialControl.InitializationPort( );
			// 初始化 GPIO
			CommonRes.GPIOControl.InitGPIO( );
			// 初始化指纹设备
			CommonRes.FingerprintDevice.InitFingerprint( );

			// 初始化通讯内核
			CommonRes.TcpControl.InitializationTcpConnection( );
		}

		//private static readonly AsyncLock state_lock = new AsyncLock( );
		private static object state_lock = new object( );

		private static WorkState _workState;
		public static WorkState SystemWorkState
		{
			get { return _workState; }
			set
			{
				if ( _workState != value )
				{
					lock ( state_lock )
					{
						_workState = value;
					}
				}
			}
		}

		#region Serial

		/// <summary>
		/// Serial device control class
		/// </summary>
		public static class SerialControl
		{

			#region Properties

			//private static SerialDevice _serialPort = null;
			///// <summary>
			///// 当前使用串行设备
			///// </summary>
			//public static SerialDevice SerialPort { get { return _serialPort; } }

			public static List<SerialDevice> _sports = null;
			public static List<SerialDevice> SerialPorts { get { return _sports; } }

			/// <summary>
			/// 串行设备读取超时设定
			/// </summary>
			public static double SerialDeviceReadTimeout { get; set; }
			/// <summary>
			/// 串行设备写入超时设定
			/// </summary>
			public static double SerialDeviceWriteTimeout { get; set; }

			#endregion

			/// <summary>
			/// 获取串行端口列表，并对第一个端口进行初始化
			/// </summary>
			public static async Task InitializationPort( )
			{
				SerialDeviceReadTimeout = 10000;
				SerialDeviceWriteTimeout = 10000;

				_sports = new List<SerialDevice>( );
				string adFilter = SerialDevice.GetDeviceSelector( );
				DeviceInformationCollection dis = await DeviceInformation.FindAllAsync( adFilter );

				foreach ( DeviceInformation dev in dis )
				{
					SerialDevice _port = await SerialDevice.FromIdAsync( dev.Id );

					_sports.Add( _port );
				}

				if ( _sports.Count > 0 )
				{
					foreach ( SerialDevice item in _sports )
						ConfigurePort( item );
				}
			}

			/// <summary>
			/// 终止所有端口
			/// </summary>
			/// <returns></returns>
			public static CommonResult FinalizationPort( )
			{
				try
				{
					if ( _sports.Count > 0 )
					{
						foreach ( SerialDevice item in _sports )
							item.Dispose( );
					}
					return new CommonResult( ResultCode.GN_SUCCESS );
				}
				catch ( Exception ex )
				{
					throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
				}
			}

			/// <summary>
			/// 初始化通讯串口
			/// </summary>
			/// <param name="p">指定的端口设备</param>
			/// <returns>返回初始化过程中的错误信息，无错误返回 string.Empty</returns>
			private static string ConfigurePort( SerialDevice p )
			{
				try
				{
					// 超时
					p.ReadTimeout = TimeSpan.FromMilliseconds( SerialDeviceReadTimeout );
					p.WriteTimeout = TimeSpan.FromMilliseconds( SerialDeviceWriteTimeout );

					// 波特率
					p.BaudRate = 115200;
					// 校验检查
					p.Parity = SerialParity.None;
					// 停止位
					p.StopBits = SerialStopBitCount.One;
					// 数据位
					p.DataBits = 8;
					// 握手方式
					p.Handshake = SerialHandshake.None;

					return string.Empty;
				}
				catch ( Exception ex )
				{
					//if ( _dataReader != null )
					//	_dataReader.DetachStream( );

					//if ( _dataWriter != null )
					//	_dataWriter.DetachStream( );

					//if ( _serialPort != null )
					//	_serialPort.Dispose( );

					return ex.Message;
				}

			}


			/// <summary>
			/// 向串行设备发送二进制数据
			/// </summary>
			/// <param name="data"></param>
			/// <returns></returns>
			public static async Task SendByte( byte[ ] data, SerialDevice port )
			{
				if ( port == null ) throw new CommonResult( ResultCode.FG_PORT_NOTINIT );

				if ( data == null || data.Length <= 0 ) throw new CommonResult( ResultCode.FG_DATA_NOTINIT );

				DataWriter _dataWriter = new DataWriter( port.OutputStream );
				try
				{
					_dataWriter.WriteBytes( data );

					using ( var cts = new CancellationTokenSource( port.WriteTimeout ) )
					{
						await _dataWriter.StoreAsync( ).AsTask( cts.Token );
					}
				}
				catch ( TaskCanceledException )
				{
					throw new CommonResult( ResultCode.GN_TASK_CANCEL );
				}
				catch ( Exception ex )
				{
					throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
				}
				finally
				{
					if ( _dataWriter != null ) _dataWriter.DetachStream( );
				}
			}

			/// <summary>
			/// 从串行设备读取二进制数据
			/// 以串口的超时作为读取超时设定
			/// </summary>
			/// <param name="requestLen">需要读取的长度</param>
			/// <returns></returns>
			public static async Task<byte[ ]> ReadByte( uint requestLen, SerialDevice port )
			{
				using ( var cts = new CancellationTokenSource( port.ReadTimeout ) )
				{
					return await ReadByte( requestLen, port, cts.Token );
				}
			}

			/// <summary>
			/// 从串行设备读取二进制数据
			/// </summary>
			/// <param name="requestLen">需要读取的长度</param>
			/// <param name="cts">取消标识</param>
			/// <returns></returns>
			public static async Task<byte[ ]> ReadByte( uint requestLen, SerialDevice port, CancellationToken cts )
			{
				if ( port == null ) throw new CommonResult( ResultCode.FG_PORT_NOTINIT );

				DataReader _dataReader = new DataReader( port.InputStream );
				try
				{
					var rLen = await _dataReader.LoadAsync( requestLen ).AsTask( cts );

					if ( rLen >= requestLen )
					{
						byte[ ] data = new byte[requestLen];
						_dataReader.ReadBytes( data );
						return data;
					}
					return null;
				}
				catch ( TaskCanceledException )
				{
					throw new CommonResult( ResultCode.GN_TASK_CANCEL );
				}
				catch ( Exception ex )
				{
					throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
				}
				finally
				{
					if ( _dataReader != null ) _dataReader.DetachStream( );
				}
			}

		}

		#endregion

		#region GPIO

		/// <summary>
		/// GPIO control class
		/// </summary>
		public static class GPIOControl
		{

			private static GpioController gpioCtl;
			private static GpioPin fingerprintPowerPin;

			private static int fingerprintPowerPinNo = 18;

			public static void InitGPIO( )
			{
				// Check GPIO object and initialization
				if ( gpioCtl == null )
				{
					gpioCtl = GpioController.GetDefault( );
					if ( gpioCtl == null )
						throw new CommonResult( ResultCode.FG_GPIO_INITFAIL );
				}

				if ( fingerprintPowerPin == null )
				{
					fingerprintPowerPin = gpioCtl.OpenPin( fingerprintPowerPinNo );
					if ( fingerprintPowerPin == null )
						throw new CommonResult( ResultCode.FG_GPIO_INITFAIL );
				}

				// Default power off
				fingerprintPowerPin.Write( GpioPinValue.High );
				fingerprintPowerPin.SetDriveMode( GpioPinDriveMode.Output );

			}

			public static CommonResult ResetFingerprintPower( )
			{
				// Power on/off fingerprint device
				if ( fingerprintPowerPin != null )
				{
					try
					{
						fingerprintPowerPin.Write( GpioPinValue.High );
						Task.Delay( 100 );
						fingerprintPowerPin.Write( GpioPinValue.Low );

						return new CommonResult( ResultCode.GN_SUCCESS );
					}
					catch ( Exception ex )
					{
						throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}
				}
				else
					return new CommonResult( ResultCode.FG_GPIO_NOTINIT );
			}

			public static async Task<CommonResult> PowerOnFingerprintPower( )
			{
				// Power on/off fingerprint device
				if ( fingerprintPowerPin != null )
				{
					try
					{
						fingerprintPowerPin.Write( GpioPinValue.Low );

						await Task.Delay( 500 );

						return new CommonResult( ResultCode.GN_SUCCESS );
					}
					catch ( Exception ex )
					{
						throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}
				}
				else
					return new CommonResult( ResultCode.FG_GPIO_NOTINIT );
			}

			public static CommonResult PowerOffFingerprintPower( )
			{
				// Power on/off fingerprint device
				if ( fingerprintPowerPin != null )
				{
					try
					{
						fingerprintPowerPin.Write( GpioPinValue.High );
						return new CommonResult( ResultCode.GN_SUCCESS );
					}
					catch ( Exception ex )
					{
						throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}
				}
				else
					return new CommonResult( ResultCode.FG_GPIO_NOTINIT );
			}

			public static CommonResult IsPowerOn( )
			{
				// Power on/off fingerprint device
				if ( fingerprintPowerPin != null )
				{
					try
					{
						return fingerprintPowerPin.Read( ) == GpioPinValue.Low ? new CommonResult( ResultCode.FG_BOARD_POWERON ) : new CommonResult( ResultCode.FG_BOARD_POWEROFF );
					}
					catch ( Exception ex )
					{
						throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}
				}
				else
					return new CommonResult( ResultCode.FG_GPIO_NOTINIT );
			}

		}

		#endregion

		#region Fingerprint

		/// <summary>
		/// Fingerprint device control class
		/// </summary>
		public static class FingerprintDevice
		{
			#region 内部变量

			private static SerialDevice _fingerprintPort = null;
			private static CancellationTokenSource cts = null;
			private static Task<CommonResult> lastTask = null;

			/// <summary>
			/// 最后一次执行结果
			/// </summary>
			public static CommonResult LastExecuteResult { get; set; }

			#endregion

			#region 公共方法

			/// <summary>
			/// 初始化指纹设备
			/// </summary>
			public static void InitFingerprint( )
			{
				_fingerprintPort = SerialControl.SerialPorts[0];
			}

			/// <summary>
			/// 执行任务
			/// </summary>
			/// <param name="command"></param>
			/// <returns></returns>
			public static async Task<CommonResult> Execute( fgCommandCore command )
			{
				if ( GPIOControl.IsPowerOn( ).Code == ResultCode.FG_BOARD_POWEROFF )
				{
					await GPIOControl.PowerOnFingerprintPower( );
				}

				// 检查当前是否有进行中的任务
				await CancelTask( );

				// 建立新的命令任务
				lastTask = runCommand( command );

				return await lastTask;
			}

			/// <summary>
			/// 结束当前的工作任务
			/// </summary>
			public static async Task CancelTask( )
			{
				if ( lastTask != null && lastTask.Status != TaskStatus.RanToCompletion )
				{
					// Check cancel token
					if ( cts != null && cts.Token.CanBeCanceled )
						cts.Cancel( );
					else
						throw new CommonResult( ResultCode.GN_TASK_RUNNING );

					// Wait last task run complete
					await lastTask;

					// Clear variable
					cts.Dispose( );
					cts = null;
					lastTask = null;
				}
			}

			/// <summary>
			/// 命令执行方法，调用此方法执行具体命令
			/// </summary>
			/// <returns></returns>
			public static async Task<CommonResult> runCommand( fgCommandCore command )
			{
				try
				{
					// 建立新的任务控制
					cts = new CancellationTokenSource( );

					// 发送命令数据包
					if ( !command.CommandIncludeDataPackage )
					{
						// 发送具体命令主体
						await CommonRes.SerialControl.SendByte( command.BuildCommandData( ), _fingerprintPort );
					}
					else
					{
						// 发送具体命令主体及数据包
						await CommonRes.SerialControl.SendByte( command.BuildCommandDataWithPackage( ), _fingerprintPort );

						//byte[ ] body = command.BuildCommandData( );
						//byte[ ] pakg = command.BuildPackageData( );
						//byte[ ] data = new byte[body.Length + pakg.Length];
						//Array.Copy( body, 0, data, 0, body.Length );
						//Array.Copy( pakg, 0, data, body.Length, pakg.Length );

						//await CommonRes.SerialControl.SendByte( data, _fingerprintPort );
					}

					// 读取返回值，命令字及应答都是 8 字节。数据包读取单独处理。
					byte[ ] ret = null;

					while ( true )
					{
						ret = await CommonRes.SerialControl.ReadByte( 8, _fingerprintPort, cts.Token );

						// 如果返回合格数据
						if ( ret == null || ret.Length <= 0 )
							// 返回数据不正确则标记 未应答
							throw new CommonResult( ResultCode.FG_BOARD_NOANSWER );

						// 检查数据包格式
						command.CheckReceiveData( ret );

						// 检查返回的数据包命令号是否对应，不对应则重新读取
						if ( command.CheckCommandAnswer( ret ) ) break;
					}

					// 对返回的数据进行处理
					LastExecuteResult = command.DispathResult( ret );

					// 检查是否需要读取数据包
					if ( ( ret[4] == ( byte )fgExecuteResult.ACK_SUCCESS ) && command.AnswerIncludeDataPackage )
					{
						// 读取数据包，长度为数据包长度 + 3 （0xF5, CHK, 0xF5）
						byte[ ] dataPackage = await CommonRes.SerialControl.ReadByte( ( uint )( command.GetDataPackageLen( ) + 3 ), _fingerprintPort, cts.Token );

						// 检查数据包格式
						command.CheckReceiveData( dataPackage );

						// 对返回的数据包进行处理
						command.DispathDataPackage( dataPackage );
					}

					// 返回执行结果
					return LastExecuteResult;

				}
				catch ( Exception ex )
				{
					// 异常处理

					if ( ex is CommonResult )
						// 如果是 CommonResult 类型，则为软件触发异常，直接记录返回
						LastExecuteResult = ex as CommonResult;
					else
						// 否则认为是系统异常，捕捉并记录返回
						LastExecuteResult = new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );

					return LastExecuteResult;
				}
			}

			#endregion
		}

		#endregion

		#region Storage

		/// <summary>
		/// File access control class
		/// </summary>
		public static class StorageControl
		{
			#region Declare

			private static Dictionary<string, string> _fuComparisons;

			private static StorageFolder _appStorageFolder;


			private static StorageFolder _adrFolder;
			private static string _adrFolderName = "AttenData";
			private static string _adrFileFormat = "yyyy-MM-dd";
			private static string _adrFileSuffix = ".pamj";
			private static string _adrTimeFormat = "yyyyMMddHHmmss";

			private static StorageFolder _perFolder;
			private static string _perFolderName = "PerData";
			private static string _perFileSuffix = ".peri";
			private static string _perInfoComparisonsFileName = "fuc.cmp";

			private static StorageFolder _settingsFolder;
			private static string _setFolderName = "Settings";
			private static string _setFileName = "Attendance.cfg";

			#endregion


			/// <summary>
			/// 初始化存储环境
			/// </summary>
			public static async Task InitializationFileAccess( )
			{
				_appStorageFolder = ApplicationData.Current.LocalFolder;

				#region Get settings folder

				try
				{
					try
					{
						// 尝试获取目录，如果目录不存在则触发异常
						_settingsFolder = await _appStorageFolder.GetFolderAsync( _setFolderName );
					}
					catch
					{
						// 在异常处理中建立目录，如果无法建立则抛出异常
						_settingsFolder = await _appStorageFolder.CreateFolderAsync( _setFolderName );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFOLDER_CREATEFAIL );
				}

				#endregion

				#region Get attendance folder

				try
				{
					try
					{
						// 尝试获取目录，如果目录不存在则触发异常
						_adrFolder = await _appStorageFolder.GetFolderAsync( _adrFolderName );
					}
					catch
					{
						// 在异常处理中建立目录，如果无法建立则抛出异常
						_adrFolder = await _appStorageFolder.CreateFolderAsync( _adrFolderName );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFOLDER_CREATEFAIL );
				}

				#endregion

				#region Get personnel information folder

				try
				{
					try
					{
						// 尝试获取目录，如果目录不存在则触发异常
						_perFolder = await _appStorageFolder.GetFolderAsync( _perFolderName );
					}
					catch
					{
						// 在异常处理中建立目录，如果无法建立则抛出异常
						_perFolder = await _appStorageFolder.CreateFolderAsync( _perFolderName );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFOLDER_CREATEFAIL );
				}

				#endregion

				await LoadFUComparisons( );
			}

			public static async Task<CommonResult> SwitchToSyncUI( )
			{
				CancellationTokenSource ff = new CancellationTokenSource( 5000 );
				while ( SystemWorkState != WorkState.Syncing )
				{
					if ( ff.IsCancellationRequested ) break;
				}
				if ( SystemWorkState != WorkState.Syncing )
					return new CommonResult( ResultCode.FG_BOARD_FAIL );

				await Task.Delay( 1000 );

				return new CommonResult( ResultCode.GN_SUCCESS );
			}

			#region Personnel

			/// <summary>
			/// 读取人员 ID 和指纹 ID 匹配数据
			/// </summary>
			public static async Task LoadFUComparisons( )
			{
				if ( _fuComparisons != null )
					_fuComparisons.Clear( );

				_fuComparisons = new Dictionary<string, string>( );

				#region Get file

				StorageFile _fucFile;
				try
				{
					try
					{
						_fucFile = await _perFolder.GetFileAsync( _perInfoComparisonsFileName );
					}
					catch
					{
						_fucFile = await _perFolder.CreateFileAsync( _perInfoComparisonsFileName );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFILE_CREATEFAIL );
				}

				#endregion

				#region Read record

				try
				{
					// Open file stream
					using ( StreamReader ss = new StreamReader( await _fucFile.OpenStreamForReadAsync( ) ) )
					{
						string allFid = ss.ReadToEnd( );
						string[ ] tmp = allFid.Split( new string[ ] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );

						for ( int i = 0; i < tmp.Length; i++ )
						{
							string[ ] fu = tmp[i].Split( ':' );
							_fuComparisons.Add( fu[0], fu[1] );
						}
					}

				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENDANCE_RECORDFAIL );
				}

				#endregion

			}

			/// <summary>
			/// 写入人员 ID 和指纹 ID 匹配数据
			/// </summary>
			/// <returns></returns>
			public static async Task WriteFUComparisons( )
			{
				if ( _fuComparisons == null ) return;

				#region Get file

				StorageFile _fucFile;
				try
				{
					_fucFile = await _perFolder.CreateFileAsync( _perInfoComparisonsFileName, CreationCollisionOption.ReplaceExisting );
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFILE_CREATEFAIL );
				}

				#endregion

				#region Write record

				try
				{
					// Open file stream
					using ( StreamWriter ss = new StreamWriter( await _fucFile.OpenStreamForWriteAsync( ) ) )
					{
						foreach ( KeyValuePair<string, string> item in _fuComparisons )
						{
							ss.WriteLine( item.Key + ":" + item.Value );
						}
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENDANCE_RECORDFAIL );
				}

				#endregion

			}


			/// <summary>
			/// 根据指定的指纹 ID 获取人员 ID
			/// </summary>
			/// <param name="fid"></param>
			/// <returns></returns>
			public static string GetPerIDByFID( ushort fid )
			{
				foreach ( KeyValuePair<string, string> item in _fuComparisons )
				{
					if ( ( item.Value != "" ) && ( Convert.ToUInt16( item.Value ) == fid ) )
					{
						return item.Key;
					}
				}
				return string.Empty;
			}

			/// <summary>
			/// 根据指定的人员 ID 获取指纹 ID
			/// </summary>
			/// <param name="pid"></param>
			/// <returns></returns>
			public static string GetFIDByPerID( string pid )
			{
				string ret = "";
				if ( _fuComparisons.TryGetValue( pid.ToString( ), out ret ) )
					return ret;
				else
					return string.Empty;
			}

			/// <summary>
			/// 采集指纹后更新人员的指纹 ID
			/// </summary>
			/// <param name="PID"></param>
			/// <param name="FID"></param>
			public static async Task UpdateFIDByPID( string PID, string FID )
			{
				// Reload data
				await LoadFUComparisons( );

				string val;
				if ( _fuComparisons.TryGetValue( PID, out val ) )
				{
					// Delete from comparisons file
					_fuComparisons[PID] = FID;
					await WriteFUComparisons( );
				}

				// Reload data
				await LoadFUComparisons( );
			}


			/// <summary>
			/// 根据人员 ID 获取人员信息，仅包括简单四项信息 PID、FID、Name、Department
			/// </summary>
			/// <param name="PID"></param>
			/// <returns></returns>
			public static async Task<PerInfo> ReadPerInfo( string PID )
			{
				#region Get file

				StorageFile _fucFile;
				try
				{
					_fucFile = await _perFolder.GetFileAsync( PID + _perFileSuffix );
				}
				catch
				{
					return null;
				}

				#endregion

				#region Read record

				PerInfo ret = new PerInfo( );

				try
				{
					// Open file stream
					using ( StreamReader ss = new StreamReader( await _fucFile.OpenStreamForWriteAsync( ), Encoding.UTF8 ) )
					{
						ret.Name = ss.ReadLine( );
						ret.Department = ss.ReadLine( );
						ret.PID = PID;
						ret.FID = GetFIDByPerID( PID );
					}

					return ret;
				}
				catch
				{
					return null;
				}

				#endregion
			}

			/// <summary>
			/// 写入人员信息
			/// </summary>
			/// <param name="per"></param>
			/// <returns></returns>
			public static async Task WritePerInfo( PerInfo per )
			{
				#region Get file

				StorageFile _fucFile;
				try
				{
					string fname = per.PID + _perFileSuffix;
					try
					{
						_fucFile = await _perFolder.GetFileAsync( fname );
					}
					catch
					{
						_fucFile = await _perFolder.CreateFileAsync( fname );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_PERFILE_CREATEFAIL );
				}

				#endregion

				#region Read record

				PerInfo ret = new PerInfo( );

				try
				{
					// Open file stream
					using ( Stream ss = await _fucFile.OpenStreamForWriteAsync( ) )
					{
						string s = per.Name + "\r\n" + per.Department + "\r\n";
						byte[ ] b = System.Text.Encoding.UTF8.GetBytes( s );
						await ss.WriteAsync( b, 0, b.Length );
						await ss.FlushAsync( );
					}

				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_PERFILE_RECORDFAIL );
				}

				#endregion
			}

			/// <summary>
			/// 获取人员信息列表
			/// </summary>
			/// <returns></returns>
			public static async Task<PerInfoList> GetPerList( )
			{
				PerInfoList ret = new PerInfoList( );

				foreach ( KeyValuePair<string, string> item in _fuComparisons )
				{
					PerInfo tmp = await ReadPerInfo( item.Key );
					if ( tmp != null )
						ret.Add( tmp );
				}

				return ret;
			}

			#endregion

			#region Attendance record

			/// <summary>
			/// 写入一条打卡记录
			/// </summary>
			/// <param name="recordDate"></param>
			/// <param name="fID"></param>
			public static async void WriteAttendanceRecord( DateTime recordDate, ushort fID )
			{

				#region Get file

				StorageFile _recordFile;
				try
				{
					string fname = recordDate.ToString( _adrFileFormat ) + _adrFileSuffix;
					try
					{
						_recordFile = await _adrFolder.GetFileAsync( fname );
					}
					catch
					{
						_recordFile = await _adrFolder.CreateFileAsync( fname );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFILE_CREATEFAIL );
				}

				#endregion

				#region Write record

				try
				{
					// Open file stream
					using ( Stream ss = await _recordFile.OpenStreamForWriteAsync( ) )
					{
						// Seek to file end
						ss.Seek( 0, SeekOrigin.End );
						// Get record content
						string s = recordDate.ToString( _adrTimeFormat ) + ":" + fID.ToString( ) + "\r\n";
						// Convert content to byte array
						byte[ ] b = System.Text.Encoding.ASCII.GetBytes( s );
						// Write Content to file
						await ss.WriteAsync( b, 0, b.Length );
						// Flush buffer
						await ss.FlushAsync( );
					}

				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENDANCE_RECORDFAIL );
				}

				#endregion

			}

			public static async Task<AttendanceRecordList> ReadAttendanceRecord( DateTime? rangeStart, DateTime? rangeEnd )
			{
				string fname = "";
				DateTime? rs = null;
				DateTime? re = null;

				if ( rangeStart != null && rangeEnd != null )
				{
					rs = rangeStart < rangeEnd ? rangeStart : rangeEnd;
					re = rangeStart > rangeEnd ? rangeStart : rangeEnd;
				}
				else
				{
					rs = rangeStart;
					re = rangeEnd;
				}

				// Get record file list
				IReadOnlyList<StorageFile> files = await _adrFolder.GetFilesAsync( Windows.Storage.Search.CommonFileQuery.OrderByName );

				// Scan every record file ( every day )
				AttendanceRecordList ret = new AttendanceRecordList( );
				foreach ( var item in files )
				{
					fname = ( item as StorageFile ).Name;

					// Check range
					if ( rs != null || re != null )
					{
						DateTime curr = DateTime.Parse( fname.Substring( 0, 10 ) );

						if ( rs != null && curr < rs )
							continue;
						else if ( re != null && curr > re )
							continue;
					}

					StorageFile _recordFile;
					try
					{
						try
						{
							_recordFile = await _adrFolder.GetFileAsync( fname );
						}
						catch { continue; }

						try
						{
							// Open file stream
							using ( StreamReader ss = new StreamReader( await _recordFile.OpenStreamForWriteAsync( ), Encoding.UTF8 ) )
							{
								// Read record
								string allFid = ss.ReadToEnd( );
								string[ ] tmp = allFid.Split( new string[ ] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );

								// Load every record
								foreach ( string s in tmp )
								{
									string[ ] data = s.Split( ':' );
									ret.Add( new AttendanceRecord( )
									{
										FID = data[1],
										PID = "",
										RecordDate = new DateTime(
											Convert.ToInt32( data[0].Substring( 0, 4 ) ),
											Convert.ToInt32( data[0].Substring( 4, 2 ) ),
											Convert.ToInt32( data[0].Substring( 6, 2 ) ),
											Convert.ToInt32( data[0].Substring( 8, 2 ) ),
											Convert.ToInt32( data[0].Substring( 10, 2 ) ),
											Convert.ToInt32( data[0].Substring( 12, 2 ) )
										)
									} );
								}
							}
						}
						catch { continue; }

					}
					catch { continue; }
				}

				return ret;
			}

			#endregion

			#region Sync

			public static async Task<CommonResult> SyncPersonnel( tcpSyncPersonnel command )
			{

				if ( command == null ) return new CommonResult( ResultCode.FG_DATA_NOTINIT );


				SystemWorkState = WorkState.RequestSync;
				try
				{
					CommonResult wret = await SwitchToSyncUI( );
					if ( wret.Code != ResultCode.GN_SUCCESS )
						return wret;

					#region Write comparison

					try
					{
						_fuComparisons.Clear( );
						foreach ( PerInfo item in command.PersonnelList )
						{
							_fuComparisons.Add( item.PID, item.FID );
						}
						await WriteFUComparisons( );

						// Reload
						await LoadFUComparisons( );
					}
					catch ( CommonResult ex )
					{
						return ex;
					}
					catch ( Exception ex )
					{
						return new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}

					#endregion

					#region Write per info & fingerprint info

					try
					{
						foreach ( PerInfo item in command.PersonnelList )
						{
							await WritePerInfo( item );
						}
					}
					catch ( CommonResult ex )
					{
						return ex;
					}
					catch ( Exception ex )
					{
						return new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}

					#endregion

					#region Sync fingprint device ( write & erase fingerprint )

					try
					{

						// erase all
						fgClearFingerprint cmdC = new fgClearFingerprint( );
						CommonResult ret = await FingerprintDevice.Execute( cmdC );

						// write new
						List<ushort> tmp = new List<ushort>( );
						foreach ( PerInfo item in command.PersonnelList )
						{
							if ( item.FID != null && item.FID != string.Empty && item.Eigenvalue != null )
							{
								fgDownloadEigenvalue cmdD = new fgDownloadEigenvalue( item.Eigenvalue, Convert.ToUInt16( item.FID ), 1 );
								ret = await FingerprintDevice.Execute( cmdD );

								tmp.Add( Convert.ToUInt16( item.FID ) );
							}
						}
					}
					catch ( CommonResult ex )
					{
						return ex;
					}
					catch ( Exception ex )
					{
						return new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
					}


					#endregion

					return new CommonResult( ResultCode.GN_SUCCESS );
				}
				finally
				{
					SystemWorkState = WorkState.SyncOver;
				}
			}

			public static async Task<PerInfoList> BuildSyncData( )
			{
				SystemWorkState = WorkState.RequestSync;
				try
				{
					CommonResult wret = await SwitchToSyncUI( );
					if ( wret.Code != ResultCode.GN_SUCCESS )
						return null;

					PerInfoList ret = new PerInfoList( );
					await LoadFUComparisons( );
					foreach ( KeyValuePair<string, string> item in _fuComparisons )
					{
						PerInfo inf = await ReadPerInfo( item.Key );
						if ( inf.FID != null && inf.FID != string.Empty )
						{
							fgUploadEigenvalue cmd = new fgUploadEigenvalue( Convert.ToUInt16( inf.FID ) );
							CommonResult rest = await FingerprintDevice.Execute( cmd );
							if ( rest.Code == ResultCode.GN_SUCCESS )
							{
								inf.Eigenvalue = CommonUtil.ArrayFixLenCopy( cmd.Eigenvalue, 193 );
							}
						}

						ret.Add( inf );
					}

					return ret;
				}
				finally
				{
					SystemWorkState = WorkState.SyncOver;
				}

			}

			#endregion

			#region Settings

			public static async Task LoadConfig( )
			{
				#region Get file

				StorageFile _setFile;
				try
				{
					try
					{
						_setFile = await _settingsFolder.GetFileAsync( _setFileName );
					}
					catch
					{
						_setFile = await _settingsFolder.CreateFileAsync( _setFileName );
					}
				}
				catch
				{
					throw new CommonResult( ResultCode.GN_FILE_ATTENFILE_CREATEFAIL );
				}

				#endregion

				using ( StreamReader ss = new StreamReader( await _setFile.OpenStreamForReadAsync( ) ) )
				{
					XmlReaderSettings xs = new XmlReaderSettings( );
					xs.IgnoreComments = true;

					XmlReader xr = XmlReader.Create( ss );
					while ( xr.Read( ) )
					{
						if ( xr.NodeType == XmlNodeType.Element )
						{
							if ( xr.Name == "ip" )
								TcpControl._serverIP = xr.ReadElementContentAsString( ).Trim( );
							else if ( xr.Name == "port" )
								TcpControl._serverPort = xr.ReadElementContentAsString( ).Trim( );
						}
					}
				}
			}

			#endregion
		}


		#endregion

		#region TCP

		public static class TcpControl
		{
			//Keep online task
			//	|
			//	----- Receive task
			//	|
			//	----- Heartbeat task

			/// <summary>
			/// 初始化 TCP 通讯内核
			/// </summary>
			public static void InitializationTcpConnection( )
			{
				// Load settings
				// TODO:

				// Connection to server
				StartKeepOnline( );
			}

			/// <summary>
			/// 释放 TCP 通讯内核
			/// </summary>
			public static void FinalizationTcpConnection( )
			{
				// Connection to server
				StopKeepOnline( );

				SendCommand( new tcpCloseConnection( ) );

				DisconnectionServer( );
			}

			#region Properties

			/// <summary>
			/// 连接状态属性
			/// </summary>
			private static bool Connected { get { return _socket != null && _write != null && _reader != null; } }

			#endregion

			#region Heartbeat

			/// <summary>
			/// 心跳命令属性，True 打开心跳命令；False 关闭心跳命令
			/// </summary>
			private static bool Heartbeat
			{
				get { return _heartbeatTimer != null; }
				set
				{
					if ( ( _heartbeatTimer != null ) != value )
					{
						if ( value )
						{
							_heartbeatTimer = ThreadPoolTimer.CreatePeriodicTimer( Timer_Heartbeat, TimeSpan.FromMilliseconds( _heartbeatInterval ) );
						}
						else
						{
							_heartbeatTimer.Cancel( );
							_heartbeatTimer = null;
						}
					}
				}
			}

			/// <summary>
			/// 心跳命令时钟
			/// </summary>
			private static ThreadPoolTimer _heartbeatTimer = null;
			/// <summary>
			/// Heartbeat interval (ms)
			/// </summary>
			private static Int32 _heartbeatInterval = 10000;

			/// <summary>
			/// 心跳时钟方法
			/// </summary>
			/// <param name="timer"></param>
			private static void Timer_Heartbeat( ThreadPoolTimer timer )
			{
				SendCommand( new tcpHeartbeat( ) );
			}

			#endregion

			#region Connection

			/// <summary>
			/// Server IP
			/// </summary>
			internal static string _serverIP = ""; //"159.226.177.167";
												   /// <summary>
												   /// Server port
												   /// </summary>
			internal static string _serverPort = ""; // "8023";
													 /// <summary>
													 /// Connect socket
													 /// </summary>
			private static StreamSocket _socket = null;
			/// <summary>
			/// Read pipe stream
			/// </summary>
			private static DataReader _reader = null;
			/// <summary>
			/// Write pipe stream
			/// </summary>
			private static DataWriter _write = null;
			/// <summary>
			/// Write sync lock
			/// </summary>
			private static readonly AsyncLock w_lock = new AsyncLock( );

			private static bool sendCommandError = false;

			/// <summary>
			/// 连接服务器
			/// </summary>
			/// <returns></returns>
			private static async Task ConnectionServer( )
			{

				if ( _socket != null ) return;

				try
				{
					HostName serverHost = new HostName( _serverIP.Trim( ) );  //设置服务器IP
					_socket = new StreamSocket( );

					await _socket.ConnectAsync( serverHost, _serverPort.Trim( ) );  //设置服务器端口号  

					_reader = new DataReader( _socket.InputStream );
					_reader.InputStreamOptions = InputStreamOptions.Partial;
					_write = new DataWriter( _socket.OutputStream );

					StartReceive( );
					// Send a first command, for get a _lastreceivetime
					//SendCommand( new tcpHeartbeat( ) );
					SendCommand( new tcpHandshake( HandshakeRequester.Unknown, _socket.Information.LocalAddress.ToString( ), Convert.ToInt32( _socket.Information.LocalPort ) ) );

					Heartbeat = true;

					sendCommandError = false;
				}
				catch ( Exception ex )
				{
					if ( _reader != null )
					{
						_reader.DetachStream( );
						_reader.Dispose( );
						_reader = null;
					}
					if ( _write != null )
					{
						_write.DetachStream( );
						_write.Dispose( );
						_write = null;
					}
					if ( _socket != null )
					{
						_socket.Dispose( );
						_socket = null;
					}
				}
			}

			/// <summary>
			/// 断开服务器
			/// </summary>
			private static void DisconnectionServer( )
			{
				Heartbeat = false;

				if ( !sendCommandError )
					SendCommand( new tcpCloseConnection( ) );

				StopReceive( );

				if ( _reader != null )
				{
					_reader.DetachStream( );
					_reader.Dispose( );
					_reader = null;
				}
				if ( _write != null )
				{
					_write.DetachStream( );
					_write.Dispose( );
					_write = null;
				}
				if ( _socket != null )
				{
					_socket.Dispose( );
					_socket = null;
				}

				sendCommandError = false;
			}

			/// <summary>
			/// 发送命令方法
			/// </summary>
			/// <param name="command"></param>
			public static async void SendCommand( tcpCommandCore command )
			{
				if ( _write == null )
					return;

				// Send specify communicate command to remote

				using ( var releaser = await w_lock.LockAsync( ) )
				{
					// Get ready
					byte[ ] _body = command.ToBinary( );
					byte[ ] _packetHeader = System.Text.Encoding.ASCII.GetBytes( "QITPS" );
					byte[ ] _packetEnd = System.Text.Encoding.ASCII.GetBytes( "SPTIQ" );
					byte[ ] _proof = BitConverter.GetBytes( CommonUtil.CRC32.BuildCRC32( _body ) );
					byte[ ] _packetLen = BitConverter.GetBytes( _body.Length + 18 );

					// Build packet
					List<byte> tmp = new List<byte>( );
					tmp.AddRange( _packetHeader );
					tmp.AddRange( _packetLen );
					tmp.AddRange( _body );
					tmp.AddRange( _proof );
					tmp.AddRange( _packetEnd );

					try
					{
						// Send command
						_write.WriteBytes( tmp.ToArray( ) );
						await _write.StoreAsync( );
					}
					catch ( System.Runtime.InteropServices.COMException )
					{
						sendCommandError = true;
						DisconnectionServer( );
					}
					catch
					{

					}
				}
			}

			/// <summary>
			/// Keep tcp connect task token
			/// </summary>
			private static CancellationTokenSource _keepOnlineTaskToken;
			/// <summary>
			/// Keep tcp connect task
			/// </summary>
			private static Task _keepOnlineTask;
			/// <summary>
			/// Reconnection check time out
			/// </summary>
			private static uint _reconnectTimeout = 20000;
			private static int _reconnectInterval = 30000;

			private static void StartKeepOnline( )
			{
				_keepOnlineTaskToken = new CancellationTokenSource( );
				_keepOnlineTask = new Task( KeepOnline, _keepOnlineTaskToken.Token );

				_keepOnlineTask.Start( );
			}

			private static void StopKeepOnline( )
			{
				if ( _keepOnlineTask != null )
				{
					_keepOnlineTaskToken.Cancel( );
					_keepOnlineTask.Wait( );

					_keepOnlineTask = null;
					_keepOnlineTaskToken = null;
				}
			}

			public static async void KeepOnline( )
			{
				while ( true )
				{
					// 
					if ( !Connected )
					{
						await ConnectionServer( );
					}
					else if ( ( DateTime.Now - _lastReceive ).TotalMilliseconds > _reconnectTimeout )
					{
						DisconnectionServer( );
						await Task.Delay( 5000 );
						await ConnectionServer( );
					}

					if ( _keepOnlineTaskToken.IsCancellationRequested )
						break;

					await Task.Delay( _reconnectInterval );

					//double t = ( DateTime.Now - _lastReceive ).TotalMilliseconds;
					//if ( !Connected || ( t > _reconnectTimeout ) )
					//{
					//	if ( t > _reconnectTimeout )
					//	{
					//		DisconnectionServer( );
					//		await Task.Delay( 5000 );
					//	}

					//	await ConnectionServer( );
					//}

				}
			}


			#endregion

			#region Receive

			/// <summary>
			/// Last receive command time
			/// </summary>
			private static DateTime _lastReceive;
			/// <summary>
			/// Buffer len for once scan tcp port buffer
			/// </summary>
			private static uint _portReceiveBufferLen = 4096;
			/// <summary>
			/// 接收数据的扫描间隔
			/// </summary>
			private static Int32 _receiveInterval = 10;
			/// <summary>
			/// Receive task cancel token
			/// </summary>
			private static CancellationTokenSource _receiveTaskToken;
			/// <summary>
			/// Receive task
			/// </summary>
			private static Task _receiveTask;
			/// <summary>
			/// Read sync lock
			/// </summary>
			private static readonly AsyncLock r_lock = new AsyncLock( );

			private static void StartReceive( )
			{
				_receiveTaskToken = new CancellationTokenSource( );
				_receiveTask = new Task( ReceiveScan, _receiveTaskToken.Token );

				_receiveTask.Start( );
			}

			private static void StopReceive( )
			{
				if ( _receiveTask != null )
				{
					_receiveTaskToken.Cancel( );
					_receiveTask.Wait( );

					_receiveTask = null;
					_receiveTaskToken = null;
				}
			}

			public static async void ReceiveScan( )
			{
				List<byte> buff = new List<byte>( );
				int buffLen = 0;
				try
				{
					while ( true )
					{
						// Sleep a receive interval
						await Task.Delay( _receiveInterval );

						// Check socket core
						if ( !Connected ) throw new Exception( );

						using ( var releaser = await r_lock.LockAsync( ) )
						{
							uint len = await _reader.LoadAsync( _portReceiveBufferLen );

							if ( len > 0 )
							{
								byte[ ] tmp = new byte[len];
								try
								{
									_reader.ReadBytes( tmp );

									// Add new receive data to buff
									buff.AddRange( tmp );
								}
								finally
								{
									tmp = null;
								}

							}
						}

						// Check buffer change
						if ( buff.Count != buffLen )
						{
							// Analysis data
							AnalysisReceivedData( buff );
							// Record len for next
							buffLen = buff.Count;
						}

						if ( _receiveTaskToken.IsCancellationRequested )
							break;
					}
				}
				catch ( Exception ex )
				{
					return;
				}
				finally
				{
					buff.Clear( );
					buff = null;
					//增加远程断开连接事件
				}
			}

			public static void AnalysisReceivedData( List<byte> buff )
			{
				if ( buff.Count < 24 ) return;

				int idx = 0;
				byte[ ] src = buff.ToArray( );
				while ( idx < buff.Count - 5 ) // Dec a packet head len
				{
					try
					{
						// 1 Find packet head
						if ( System.Text.Encoding.ASCII.GetString( buff.GetRange( idx, 5 ).ToArray( ), 0, 5 ) == "QITPS" )
						{
							// 2 Get packet length
							if ( buff.Count - idx - 5 < 2 ) return;
							int pLen = BitConverter.ToInt32( buff.GetRange( idx + 5, 4 ).ToArray( ), 0 );

							// 3 Get packet content
							if ( buff.Count < idx + pLen ) return;
							byte[ ] content = buff.GetRange( idx + 9, pLen - 18 ).ToArray( );// new byte[pLen - 18];

							// 4 Get CRC32
							byte[ ] crc = buff.GetRange( idx + ( pLen - 9 ), 4 ).ToArray( );

							// 5 Get packet end
							string pEnd = System.Text.Encoding.ASCII.GetString( buff.GetRange( idx + ( pLen - 5 ), 5 ).ToArray( ), 0, 5 );

							// Verify
							if ( pEnd != "SPTIQ" ) { idx++; break; }// Head
							if ( string.Compare( CommonUtil.CRC32.BuildCRC32( content ).ToString( ), BitConverter.ToUInt32( crc, 0 ).ToString( ) ) != 0 ) { idx++; break; }// CRC32

							try
							{
								// Analysis command and trigger event
								tcpCommandCore cmd = tcpCommandCore.Parse( content );
								if ( cmd != null )
								{
									// Record receive time
									_lastReceive = DateTime.Now;

									// Do event
									Task.Factory.StartNew( ( ) => OnReceiveCommand( cmd ) );
								}
							}
							finally
							{
								// remove
								buff.RemoveRange( 0, idx + pLen );
								idx = 0;
							}
						}
						else
							idx++;

					}
					catch ( Exception ex )
					{

					}
					finally
					{

					}
				}
			}

			public static async void OnReceiveCommand( object command )
			{
				//_lastReceive = DateTime.Now;

				try
				{
					if ( ( command is tcpHandshake ) )
					{

					}
					else if ( ( command is tcpHandshakeAnswer ) )
					{
						#region tcpHandshakeAnswer

						try
						{
							DateTime dt;
							if ( ( command as tcpHandshakeAnswer ).CommandTime == null )
								return;
							else
								dt = ( DateTime )( ( command as tcpHandshakeAnswer ).CommandTime );

							var localTime = new SystemTimeWin32.Systemtime( )
							{
								wYear = ( ushort )dt.Year,
								wMonth = ( ushort )dt.Month,
								wDay = ( ushort )dt.Day,
								wHour = ( ushort )dt.Hour,
								wMinute = ( ushort )dt.Minute,
								wMiliseconds = ( ushort )dt.Millisecond

							};

							var result = SystemTimeWin32.SetSystemTime( ref localTime );
						}
						catch { }

						#endregion
					}
					else if ( ( command is tcpHeartbeat ) )
					{
						#region tcpHeartbeat

						#endregion
					}
					else if ( ( command is tcpDebugMessage ) )
					{
						#region tcpDebugMessage

						#endregion
					}
					else if ( ( command is tcpSyncPersonnel ) )
					{
						#region Receive personnel data from server

						try
						{
							CommonResult ret = await StorageControl.SyncPersonnel( command as tcpSyncPersonnel );
						}
						catch { }

						#endregion
					}
					else if ( ( command is tcpRequestSync ) )
					{
						#region Receive a sync request from server, build personnel data and send package to server

						try
						{
							if ( ( command as tcpRequestSync ).RequestContent == ContentType.PersonnelInfo )
							{
								PerInfoList tmp = await StorageControl.BuildSyncData( );
								if ( tmp != null )
								{
									tcpSyncPersonnel cmd = new tcpSyncPersonnel( );
									cmd.PersonnelList = tmp;
									TcpControl.SendCommand( cmd );
								}
							}
							else if ( ( command as tcpRequestSync ).RequestContent == ContentType.AttendanceRecord )
							{
								AttendanceRecordList tmp = await StorageControl.ReadAttendanceRecord(
									( command as tcpRequestSync ).AttendanceRecordRange.StartDate,
									( command as tcpRequestSync ).AttendanceRecordRange.EndDate
								);

								if ( tmp != null )
								{
									tcpAttendanceRecord cmd = new tcpAttendanceRecord( );
									cmd.Records = tmp;
									TcpControl.SendCommand( cmd );
								}
							}
						}
						catch { }

						#endregion
					}
					else if ( ( command is tcpAttendanceRecord ) )
					{
						#region tcpAttendanceRecord



						#endregion
					}
				}
				catch
				{

				}

			}

			#endregion

		}

		#endregion

		#region Common

		public class CommonUtil
		{
			public static class CRC32
			{
				static ulong[ ] _crc32Table;

				/// <summary>
				/// Build CRC32 table
				/// </summary>
				private static void GetCRC32Table( )
				{
					ulong Crc;
					_crc32Table = new ulong[256]; int i, j;
					for ( i = 0; i < 256; i++ )
					{
						Crc = ( ulong )i;
						for ( j = 8; j > 0; j-- )
						{
							if ( ( Crc & 1 ) == 1 )
								Crc = ( Crc >> 1 ) ^ 0xEDB88320;
							else
								Crc >>= 1;
						}
						_crc32Table[i] = Crc;
					}
				}

				/// <summary>
				/// Build CRC32 proof
				/// </summary>
				/// <param name="content"></param>
				/// <returns></returns>
				public static UInt32 BuildCRC32( byte[ ] content )
				{
					//生成码表
					if ( _crc32Table == null )
						GetCRC32Table( );

					//byte[ ] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes( content );
					ulong value = 0xffffffff;
					int len = content.Length;
					for ( int i = 0; i < len; i++ )
					{
						value = ( value >> 8 ) ^ _crc32Table[( value & 0xFF ) ^ content[i]];
					}
					return Convert.ToUInt32( value ^ 0xffffffff );
				}
			}

			public static byte[ ] ArrayFixLenCopy( byte[ ] sour, int fixLen )
			{
				if ( sour == null )
					throw new CommonResult( ResultCode.FG_DATA_NOTINIT );

				int copyLen = sour.Length >= fixLen ? fixLen : sour.Length;

				byte[ ] ret = new byte[fixLen];
				Array.Copy( sour, ret, copyLen );

				return ret;
			}
		}

		#region Async Lock

		class AsyncSemaphore
		{
			private readonly static Task s_completed = Task.FromResult( true );
			private readonly Queue<TaskCompletionSource<bool>> m_waiters = new Queue<TaskCompletionSource<bool>>( );
			private int m_currentCount;

			public AsyncSemaphore( int initialCount )
			{
				if ( initialCount < 0 ) throw new ArgumentOutOfRangeException( "initialCount" );
				m_currentCount = initialCount;
			}

			public Task WaitAsync( )
			{
				lock ( m_waiters )
				{
					if ( m_currentCount > 0 )
					{
						--m_currentCount;
						return s_completed;
					}
					else
					{
						var waiter = new TaskCompletionSource<bool>( );
						m_waiters.Enqueue( waiter );
						return waiter.Task;
					}
				}
			}

			public void Release( )
			{
				TaskCompletionSource<bool> toRelease = null;
				lock ( m_waiters )
				{
					if ( m_waiters.Count > 0 )
						toRelease = m_waiters.Dequeue( );
					else
						++m_currentCount;
				}
				if ( toRelease != null )
					toRelease.SetResult( true );
			}
		}

		public class AsyncLock
		{
			private readonly AsyncSemaphore m_semaphore;
			private readonly Task<Releaser> m_releaser;

			public AsyncLock( )
			{
				m_semaphore = new AsyncSemaphore( 1 );
				m_releaser = Task.FromResult( new Releaser( this ) );
			}

			public Task<Releaser> LockAsync( )
			{
				var wait = m_semaphore.WaitAsync( );
				return wait.IsCompleted ?
					m_releaser :
					wait.ContinueWith( ( _, state ) => new Releaser( ( AsyncLock )state ),
						this, CancellationToken.None,
						TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default );
			}

			public struct Releaser : IDisposable
			{
				private readonly AsyncLock m_toRelease;

				internal Releaser( AsyncLock toRelease ) { m_toRelease = toRelease; }

				public void Dispose( )
				{
					if ( m_toRelease != null )
						m_toRelease.m_semaphore.Release( );
				}
			}
		}

		#endregion

		public enum WorkState
		{
			Ready,
			Config,
			RequestSync,
			Syncing,
			SyncOver
		}


		internal class SystemTimeWin32
		{
			[DllImport( "Kernel32.dll", CharSet = CharSet.Ansi )]
			public static extern bool SetSystemTime( ref Systemtime sysTime );
			[DllImport( "Kernel32.dll" )]
			public static extern bool SetLocalTime( ref Systemtime sysTime );
			[DllImport( "Kernel32.dll" )]
			public static extern void GetSystemTime( ref Systemtime sysTime );
			[DllImport( "Kernel32.dll" )]
			public static extern void GetLocalTime( ref Systemtime sysTime );

			/// <summary>
			/// 时间结构体
			/// </summary>
			[StructLayout( LayoutKind.Sequential )]
			public struct Systemtime
			{
				public ushort wYear;
				public ushort wMonth;
				public ushort wDayOfWeek;
				public ushort wDay;
				public ushort wHour;
				public ushort wMinute;
				public ushort wSecond;
				public ushort wMiliseconds;
			}
		}
		#endregion

		#region Settings

		public static class NetworkPresenter
		{
			private readonly static uint EthernetIanaType = 6;

			public static string GetDirectConnectionName( )
			{
				try
				{
					var icp = NetworkInformation.GetInternetConnectionProfile( );
					if ( icp != null )
					{
						if ( icp.NetworkAdapter.IanaInterfaceType == EthernetIanaType )
						{
							return icp.ProfileName;
						}
					}

					return null;
				}
				catch
				{
					return null;
				}
			}

			public static string GetCurrentNetworkName( )
			{
				try
				{
					var icp = NetworkInformation.GetInternetConnectionProfile( );
					if ( icp != null )
					{
						return icp.ProfileName;
					}

					var resourceLoader = ResourceLoader.GetForCurrentView( );
					var msg = resourceLoader.GetString( "NoInternetConnection" );
					return msg;
				}
				catch
				{
					return null;
				}
			}

			public static string GetCurrentIpv4Address( )
			{
				try
				{
					var icp = NetworkInformation.GetInternetConnectionProfile( );
					if ( icp != null && icp.NetworkAdapter != null && icp.NetworkAdapter.NetworkAdapterId != null )
					{
						var name = icp.ProfileName;

						var hostnames = NetworkInformation.GetHostNames( );

						foreach ( var hn in hostnames )
						{
							if ( hn.IPInformation != null &&
								hn.IPInformation.NetworkAdapter != null &&
								hn.IPInformation.NetworkAdapter.NetworkAdapterId != null &&
								hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId &&
								hn.Type == HostNameType.Ipv4 )
							{
								return hn.CanonicalName;
							}
						}
					}

					var resourceLoader = ResourceLoader.GetForCurrentView( );
					var msg = resourceLoader.GetString( "NoInternetConnection" );
					return msg;
				}
				catch
				{
					return null;
				}
			}

			//private Dictionary<WiFiAvailableNetwork, WiFiAdapter> networkNameToInfo;

			//private static WiFiAccessStatus? accessStatus;

			//public static async Task<bool> WifiIsAvailable( )
			//{
			//	if ( ( await TestAccess( ) ) == false )
			//	{
			//		return false;
			//	}

			//	try
			//	{
			//		var adapters = await WiFiAdapter.FindAllAdaptersAsync( );
			//		return adapters.Count > 0;
			//	}
			//	catch ( Exception )
			//	{
			//		return false;
			//	}
			//}

			//private async Task<bool> UpdateInfo( )
			//{
			//	if ( ( await TestAccess( ) ) == false )
			//	{
			//		return false;
			//	}

			//	networkNameToInfo = new Dictionary<WiFiAvailableNetwork, WiFiAdapter>( );

			//	var adapters = WiFiAdapter.FindAllAdaptersAsync( );

			//	foreach ( var adapter in await adapters )
			//	{
			//		await adapter.ScanAsync( );

			//		if ( adapter.NetworkReport == null )
			//		{
			//			continue;
			//		}

			//		foreach ( var network in adapter.NetworkReport.AvailableNetworks )
			//		{
			//			if ( !HasSsid( networkNameToInfo, network.Ssid ) )
			//			{
			//				networkNameToInfo[network] = adapter;
			//			}
			//		}
			//	}

			//	return true;
			//}

			//private bool HasSsid( Dictionary<WiFiAvailableNetwork, WiFiAdapter> resultCollection, string ssid )
			//{
			//	foreach ( var network in resultCollection )
			//	{
			//		if ( !string.IsNullOrEmpty( network.Key.Ssid ) && network.Key.Ssid == ssid )
			//		{
			//			return true;
			//		}
			//	}
			//	return false;
			//}

			//public async Task<IList<WiFiAvailableNetwork>> GetAvailableNetworks( )
			//{
			//	await UpdateInfo( );

			//	return networkNameToInfo.Keys.ToList( );
			//}

			//public WiFiAvailableNetwork GetCurrentWifiNetwork( )
			//{
			//	var connectionProfiles = NetworkInformation.GetConnectionProfiles( );

			//	if ( connectionProfiles.Count < 1 )
			//	{
			//		return null;
			//	}

			//	var validProfiles = connectionProfiles.Where( profile =>
			//	{
			//		return ( profile.IsWlanConnectionProfile && profile.GetNetworkConnectivityLevel( ) != NetworkConnectivityLevel.None );
			//	} );

			//	if ( validProfiles.Count( ) < 1 )
			//	{
			//		return null;
			//	}

			//	var firstProfile = validProfiles.First( ) as ConnectionProfile;

			//	return networkNameToInfo.Keys.First( wifiNetwork => wifiNetwork.Ssid.Equals( firstProfile.ProfileName ) );
			//}

			//public async Task<bool> ConnectToNetwork( WiFiAvailableNetwork network, bool autoConnect )
			//{
			//	if ( network == null )
			//	{
			//		return false;
			//	}

			//	var result = await networkNameToInfo[network].ConnectAsync( network, autoConnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual );

			//	return ( result.ConnectionStatus == WiFiConnectionStatus.Success );
			//}

			//public void DisconnectNetwork( WiFiAvailableNetwork network )
			//{
			//	networkNameToInfo[network].Disconnect( );
			//}

			//public static bool IsNetworkOpen( WiFiAvailableNetwork network )
			//{
			//	return network.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None;
			//}

			//public async Task<bool> ConnectToNetworkWithPassword( WiFiAvailableNetwork network, bool autoConnect, PasswordCredential password )
			//{
			//	if ( network == null )
			//	{
			//		return false;
			//	}

			//	var result = await networkNameToInfo[network].ConnectAsync(
			//		network,
			//		autoConnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual,
			//		password );

			//	return ( result.ConnectionStatus == WiFiConnectionStatus.Success );
			//}

			//private static async Task<bool> TestAccess( )
			//{
			//	if ( !accessStatus.HasValue )
			//	{
			//		accessStatus = await WiFiAdapter.RequestAccessAsync( );
			//	}

			//	return ( accessStatus == WiFiAccessStatus.Allowed );
			//}


			//public class NetworkInfo
			//{
			//	public string NetworkName { get; set; }
			//	public string NetworkIpv6 { get; set; }
			//	public string NetworkIpv4 { get; set; }
			//	public string NetworkStatus { get; set; }
			//}

			//public static async Task<IList<NetworkInfo>> GetNetworkInformation( )
			//{
			//	var networkList = new Dictionary<string, NetworkInfo>( );
			//	var hostNamesList = NetworkInformation.GetHostNames( );
			//	var resourceLoader = ResourceLoader.GetForCurrentView( );

			//	foreach ( var hostName in hostNamesList )
			//	{
			//		if ( ( hostName.Type == HostNameType.Ipv4 || hostName.Type == HostNameType.Ipv6 ) &&
			//			( hostName != null && hostName.IPInformation != null && hostName.IPInformation.NetworkAdapter != null ) )
			//		{
			//			var profile = await hostName.IPInformation.NetworkAdapter.GetConnectedProfileAsync( );
			//			if ( profile != null )
			//			{
			//				NetworkInfo info;
			//				var found = networkList.TryGetValue( profile.ProfileName, out info );
			//				if ( !found )
			//				{
			//					info = new NetworkInfo( );
			//					info.NetworkName = profile.ProfileName;
			//					var statusTag = profile.GetNetworkConnectivityLevel( ).ToString( );
			//					info.NetworkStatus = resourceLoader.GetString( "NetworkConnectivityLevel_" + statusTag );
			//				}
			//				if ( hostName.Type == HostNameType.Ipv4 )
			//				{
			//					info.NetworkIpv4 = hostName.CanonicalName;
			//				}
			//				else
			//				{
			//					info.NetworkIpv6 = hostName.CanonicalName;
			//				}
			//				if ( !found )
			//				{
			//					networkList[profile.ProfileName] = info;
			//				}
			//			}
			//		}
			//	}

			//	var res = new List<NetworkInfo>( );
			//	res.AddRange( networkList.Values );
			//	return res;
			//}
		}

		#endregion

	}
}
