using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using PHR.PAM.Command;
using PHR.PAM.Common;
using PHR.PAM.fgCommand;
using Windows.UI.Xaml.Media.Imaging;
using static PHR.PAM.Common.CommonRes;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PAM
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class DebugPage : Page
	{

		public DebugPage( )
		{
			this.InitializeComponent( );

			//StartTimer( );
		}

		#region Timer

		ThreadPoolTimer t1 = null;

		private void StartTimer( )
		{
			t1 = ThreadPoolTimer.CreatePeriodicTimer( Timer_Tick, TimeSpan.FromMilliseconds( 1000 ) );
		}

		private void Timer_Tick( ThreadPoolTimer timer )
		{
			var task = Dispatcher.RunAsync( CoreDispatcherPriority.High, ( ) =>
				{
					//textDate.Text = DateTime.Now.ToString( "U" );
					//textBlock.Text = DateTime.Now.ToString( "HH:mm:ss" );
				}
			);
		}




		#endregion


		private async void button_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgRemoveFingerprint cmd = new fgRemoveFingerprint( Convert.ToUInt16( textBox.Text ) );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result:" + FingerprintDevice.LastExecuteResult.Message;

			//ContentDialog1 nn = new ContentDialog1( );

			//await nn.ShowAsync( );
			//lock ( _lock )
			//{
			//	ConnectionServer( );
			//}
		}

		private async void btnDisconnect_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgClearFingerprint cmd = new fgClearFingerprint( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result:" + FingerprintDevice.LastExecuteResult.Message;

			//this.Frame.Navigate( typeof( Config ) );
			//lock ( _lock )
			//{
			//	DisconnectionServer( );
			//}
		}

		private async void btnDisconnect_Copy_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgGetCount cmd = new fgGetCount( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nUsers: " + cmd.Count.ToString( );

			//try
			//{
			//	_dataWriter.WriteBytes( BuildCommandData( new byte[ ] { 0x09, 0x00, 0x00, 0x00, 0x00 } ) );
			//	await _dataWriter.StoreAsync( );

			//	await _dataReader.LoadAsync( this._readBufferLength );
			//	if ( _dataReader.UnconsumedBufferLength > 0 )
			//	{
			//		byte[ ] data = new byte[_dataReader.UnconsumedBufferLength];
			//		_dataReader.ReadBytes( data );

			//		textBlock1.Text = "";
			//		for ( int i = 0; i < data.Length; i++ )
			//		{
			//			textBlock1.Text += Convert.ToString( data[i], 16 ) + " ";
			//		}
			//	}

			//}
			//catch ( Exception ex )
			//{
			//}




			// =-----------------------------------------------------------------------

			//string ret = await SendByte( BuildCommandData( new byte[ ] { 0x09, 0x00, 0x00, 0x00, 0x00 } ) );

			//if ( ret != string.Empty )
			//{
			//	textBlock1.Text = ret;
			//	return;
			//}

			//byte[ ] red;
			//ret = ReadByte( out red );
			//if ( ret != string.Empty )
			//{
			//	textBlock1.Text = ret;
			//	return;
			//}
			//else
			//{
			//	textBlock1.Text = "";
			//	for ( int i = 0; i < red.Length; i++ )
			//	{
			//		textBlock1.Text = textBlock1.Text + Convert.ToString( red[i], 2 );
			//	}
			//}


			// =-----------------------------------------------------------------------

			//	if ( _sports.Count <= 0 ) return;


			//	SerialDevice _derialPort = _sports[0];
			//	try
			//	{
			//		_derialPort.ReadTimeout = TimeSpan.FromMilliseconds( 1000 );//超时
			//		_derialPort.BaudRate = 9600;//波特率
			//		_derialPort.Parity = SerialParity.None;//校验检查
			//		_derialPort.StopBits = SerialStopBitCount.One;//停止位
			//		_derialPort.DataBits = 8;//数据位
			//		_derialPort.Handshake = SerialHandshake.None;//握手方式

			//		_dataWriter = new DataWriter( _derialPort.OutputStream );

			//		//设置读取输入流
			//		_dataReader = new DataReader( _derialPort.InputStream );
			//		_dataReader.InputStreamOptions = InputStreamOptions.Partial;

			//		_dataWriter.WriteBytes( new byte[ ] { 0xAA, 0xBB, 0xCC, 0xDD } );
			//		await _dataWriter.StoreAsync( );

			//		await _dataReader.LoadAsync( 100 );

			//		if ( _dataReader.UnconsumedBufferLength > 0 )
			//		{
			//			StringBuilder str_builder = new StringBuilder( );
			//			while ( _dataReader.UnconsumedBufferLength > 0 )
			//			{
			//				str_builder.Append( _dataReader.ReadByte( ).ToString( "x2" ) );
			//			}

			//			txbPort.Text = txbPort.Text + "\r\n Read: " + str_builder.ToString( ).ToUpper( );
			//		}
			//	}
			//	finally
			//	{
			//		if ( _dataReader != null )
			//			_dataReader.DetachStream( );

			//		if ( _dataWriter != null )
			//			_dataWriter.DetachStream( );

			//		if ( _derialPort != null )
			//			_derialPort.Dispose( );
			//	}

			//	Task<UInt32> loadAsyncTask;
			//	//读取数据
			//	loadAsyncTask = _dataReader.LoadAsync( _readBufferLength ).AsTask( );
			//	uint bytesRead = await loadAsyncTask;
			//	//判断获取数据长度
			//	if ( bytesRead > 0 )
			//	{
			//		//转换十六进制数据
			//		string res = LoadData( bytesRead );
			//	}

			//}
			//private string LoadData( uint bytesRead )
			//{
			//	StringBuilder str_builder = new StringBuilder( );

			//	//转换缓冲区数据为16进制
			//	while ( _dataReader.UnconsumedBufferLength > 0 )
			//	{
			//		str_builder.Append( _dataReader.ReadByte( ).ToString( "x2" ) );
			//	}
			//	return str_builder.ToString( ).ToUpper( );
			//}
		}


		private void textDate_SelectionChanged( object sender, RoutedEventArgs e )
		{

		}

		private async void btnDisconnect_Copy1_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgHibernate cmd = new fgHibernate( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message;

		}

		private async void btnDisconnect_Copy2_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgGetFingerprintMode cmd = new fgGetFingerprintMode( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent Mode: " + cmd.CurrentMode.ToString( );

		}

		private async void btnDisconnect_Copy3_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgSetFingerprintMode cmd = new fgSetFingerprintMode( FingerprintMode.Unique );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent Mode: " + cmd.CurrentMode.ToString( );

		}

		private async void btnDisconnect_Copy4_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgSetFingerprintMode cmd = new fgSetFingerprintMode( FingerprintMode.Duplicates );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent Mode: " + cmd.CurrentMode.ToString( );
		}

		private async void btnDisconnect_Copy5_Click( object sender, RoutedEventArgs e )
		{
			//TimeSpan _orgTimeout = CommonRes.SerialPort.ReadTimeout;
			//try
			//{
			textBlock1.Text = "";

			// Get current count
			fgGetCount cmd1 = new fgGetCount( );
			CommonResult ret1 = await FingerprintDevice.Execute( cmd1 );
			if ( ret1.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nGet fingerprint count error.";
				return;
			}
			else
				textBlock1.Text = "Now collect fingerprint first ....";

			ushort newID = ( ushort )( cmd1.Count + 1 );

			//CommonRes.SerialPort.ReadTimeout = TimeSpan.FromMilliseconds( 10000 );

			// First collect
			fgCollectFingerprint cmd2 = new fgCollectFingerprint( 1, newID, 1 );
			CommonResult ret2 = await FingerprintDevice.Execute( cmd2 );
			if ( ret2.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCollect times 1 fail.";
				return;
			}
			else
				textBlock1.Text = "Now collect fingerprint second ....";

			// Second collect
			fgCollectFingerprint cmd3 = new fgCollectFingerprint( 2, newID, 1 );
			CommonResult ret3 = await FingerprintDevice.Execute( cmd3 );
			if ( ret3.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCollect times 2 fail.";
				return;
			}
			else
				textBlock1.Text = "Now collect fingerprint last ....";

			// Last collect
			fgCollectFingerprint cmd4 = new fgCollectFingerprint( 3, newID, 1 );
			CommonResult ret4 = await FingerprintDevice.Execute( cmd4 );
			if ( ret4.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCollect times 2 fail.";
				return;
			}
			else
				textBlock1.Text = "Collect fingerprint finished !!!";

			//}
			//finally
			//{
			//	CommonRes.SerialPort.ReadTimeout = _orgTimeout;
			//}


		}

		private async void btnConnect_Copy_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgSetCollectTimeout cmd = new fgSetCollectTimeout( 0 );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nSet timeout: " + cmd.CurrentTimeout.ToString( );
		}

		private async void btnConnect_Copy1_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgGetCollectTimeout cmd = new fgGetCollectTimeout( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent timeout: " + cmd.CurrentTimeout.ToString( );
		}

		private async void btnConnect_Copy2_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgSetCollectTimeout cmd = new fgSetCollectTimeout( 10 );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nSet timeout: " + cmd.CurrentTimeout.ToString( );
		}

		private async void btnDisconnect_Copy6_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgGetAllFingerprint cmd = new fgGetAllFingerprint( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message;
			textBlock1.Text += "\r\nAll user: \r\n";

			foreach ( KeyValuePair<UInt16, byte> kvp in cmd.fpList )
			{
				textBlock1.Text += kvp.Key.ToString( ) + ":" + kvp.Value.ToString( ) + " \r\n";
			}

		}

		private async void btnDisconnect_Copy7_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCompare11 cmd = new fgCompare11( Convert.ToUInt16( textBox.Text ) );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCompare 1:1 for fingerprint " + textBox.Text.Trim( );
		}

		private async void btnDisconnect_Copy8_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCompare1N cmd = new fgCompare1N( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCompare 1:N: UserID: " + cmd.ID.ToString( );
		}

		private async void btnDisconnect_Copy9_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgGetCompareLevel cmd = new fgGetCompareLevel( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent compare level: " + ( cmd.CurrentCompareLevel + 1 ).ToString( );
		}

		private async void btnDisconnect_Copy10_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgSetCompareLevel cmd = new fgSetCompareLevel( 4 );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent compare level: " + ( cmd.CurrentCompareLevel + 1 ).ToString( );
		}

		private async void btnDisconnect_Copy11_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgSetCompareLevel cmd = new fgSetCompareLevel( 9 );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nCurrent compare level: " + ( cmd.CurrentCompareLevel + 1 ).ToString( );
		}

		private async void btnDisconnect_Copy12_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgGetAuthorize cmd = new fgGetAuthorize( Convert.ToUInt16( textBox.Text ) );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\nUser authorize: " + ( cmd.Authorize ).ToString( );
		}

		private async void btnDisconnect_Copy13_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCollectImageUpload cmd = new fgCollectImageUpload( );

			CommonResult ret = await FingerprintDevice.Execute( cmd );

			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
			if ( FingerprintDevice.LastExecuteResult.Code == ResultCode.GN_SUCCESS )
				textBlock1.Text += "Fingerprint image data len: " + cmd.ImageData.Length.ToString( ) + "\r\n";

			BitmapImage i = new BitmapImage( new Uri( "ms-appx:Assets/SplashScreen.scale-200.png" ) );
			image1.Source = i;

			//for ( int i = 0; i < cmd.ImageData.Length; i++ )
			//{
			//	textBlock1.Text += "0x" + Convert.ToString( cmd.ImageData[i], 16 ) + " ";
			//}
		}

		private async void btnDisconnect_Copy14_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCollectEigenvalueUpload cmd = new fgCollectEigenvalueUpload( );
			CommonResult ret = await FingerprintDevice.Execute( cmd );
			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
			if ( FingerprintDevice.LastExecuteResult.Code == ResultCode.GN_SUCCESS )
				textBlock1.Text += "Fingerprint eigenvalue data len: " + cmd.Eigenvalue.Length.ToString( ) + "\r\n";


			fgDownloadEigenvalueCompare cmdD = new fgDownloadEigenvalueCompare( cmd.Eigenvalue );
			ret = await FingerprintDevice.Execute( cmdD );
			textBlock1.Text += "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";

		}

		private async void btnDisconnect_Copy15_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCollectEigenvalueUpload cmd = new fgCollectEigenvalueUpload( );
			CommonResult ret = await FingerprintDevice.Execute( cmd );
			if ( FingerprintDevice.LastExecuteResult.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Collect sample fingerprint fail. Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
				return;
			}

			fgDownloadEigenvalueCompareDSP11 cmdD = new fgDownloadEigenvalueCompareDSP11( cmd.Eigenvalue, Convert.ToUInt16( textBox.Text ) );
			CommonResult retD = await FingerprintDevice.Execute( cmdD );
			textBlock1.Text += "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
			textBlock1.Text += "Download fingerprint compare DSP 1:1 ";


		}

		private async void btnDisconnect_Copy16_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCollectEigenvalueUpload cmd = new fgCollectEigenvalueUpload( );
			CommonResult ret = await FingerprintDevice.Execute( cmd );
			if ( FingerprintDevice.LastExecuteResult.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Collect sample fingerprint fail. Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
				return;
			}

			fgDownloadEigenvalueCompareDSP1N cmdD = new fgDownloadEigenvalueCompareDSP1N( cmd.Eigenvalue );
			CommonResult retD = await FingerprintDevice.Execute( cmdD );
			textBlock1.Text += "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
			textBlock1.Text += "Download fingerprint compare DSP 1:N result fingerprint id : " + cmdD.ID.ToString( );

		}

		private async void btnDisconnect_Copy17_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgUploadEigenvalue cmd = new fgUploadEigenvalue( Convert.ToUInt16( textBox.Text ) );
			CommonResult ret = await FingerprintDevice.Execute( cmd );
			textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
			if ( FingerprintDevice.LastExecuteResult.Code == ResultCode.GN_SUCCESS )
				textBlock1.Text += "Upload fingerprint " + textBox.Text + " fingerprint eigenvalue data len: " + cmd.Eigenvalue.Length.ToString( ) + "\r\n";

		}

		private async void btnDisconnect_Copy18_Click( object sender, RoutedEventArgs e )
		{
			textBlock1.Text = "";

			fgCollectEigenvalueUpload cmd = new fgCollectEigenvalueUpload( );
			CommonResult ret = await FingerprintDevice.Execute( cmd );

			if ( FingerprintDevice.LastExecuteResult.Code != ResultCode.GN_SUCCESS )
			{
				textBlock1.Text = "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
				textBlock1.Text += "Collect fingerprint fail.";
				return;
			}

			fgDownloadEigenvalue cmdD = new fgDownloadEigenvalue( cmd.Eigenvalue, Convert.ToUInt16( textBox.Text ), 1 );
			CommonResult ret2 = await FingerprintDevice.Execute( cmdD );
			textBlock1.Text += "Result: " + FingerprintDevice.LastExecuteResult.Message + "\r\n";
		}

		private void backButton_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( AttendanceReady ) );
		}

		private void btnDisconnect_Copy19_Click( object sender, RoutedEventArgs e )
		{
			CommonRes.GPIOControl.ResetFingerprintPower( );
		}

		private async void btnDisconnect_Copy20_Click( object sender, RoutedEventArgs e )
		{
			await CommonRes.GPIOControl.PowerOnFingerprintPower( );
		}

		private void btnDisconnect_Copy21_Click( object sender, RoutedEventArgs e )
		{
			CommonRes.GPIOControl.PowerOffFingerprintPower( );
		}

		private async void btnDisconnect_Copy22_Click( object sender, RoutedEventArgs e )
		{
			await CommonRes.FingerprintDevice.CancelTask( );

		}

		private async void button_Click_1( object sender, RoutedEventArgs e )
		{
			//ContentDialogResult ret = await InfoDialog.ShowOK( "中文字体测试字符串" );
			//await InfoDialog.ShowYesNo( "Hello world!" );

			//TcpControl.SendCommand( new tcpDebugMessage( ) { Message = "This message send from IoT!" } );

			//string ip4address = NetworkPresenter.GetCurrentIpv4Address( );
			//string netname = NetworkPresenter.GetCurrentNetworkName( );
			//string connname = NetworkPresenter.GetDirectConnectionName( );

			//textBlock1.Text = "Network address: " + ip4address + "\r\n" + "Network name: " + netname + "\r\n" + "Connection name: " + connname;

		}

		private void CommandInvokedHandler( IUICommand command )
		{
			// Display message showing the label of the command that was invoked
			//rootPage.NotifyUser( "The '" + command.Label + "' command has been selected.",
			//	NotifyType.StatusMessage );
		}
	}
}
