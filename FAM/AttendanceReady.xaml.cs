using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using PHR.PAM.Command;
using PHR.PAM.Common;
using PHR.PAM.fgCommand;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static PHR.PAM.Common.CommonRes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PAM
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class AttendanceReady : Page
	{

		//InfoDialog dlg = new InfoDialog( );
		private static CancellationTokenSource msgCTS = null;
		private static Task msgTimeTask = null;

		public AttendanceReady( )
		{
			this.InitializeComponent( );
		}

		protected override async void OnNavigatingFrom( NavigatingCancelEventArgs e )
		{
			// On page leave
			StopTimer( );
			CancleTimeTask( );

			// Stop attendance task
			await StopAttendanceTask( );

			// Power off fingerprint
			CommonRes.GPIOControl.PowerOffFingerprintPower( );
		}

		protected override async void OnNavigatedTo( NavigationEventArgs e )
		{
			CommonRes.SystemWorkState = WorkState.Ready;

			// On page load
			Timer_Tick( null );
			StartTimer( );

			// Power on fingerprint
			await CommonRes.GPIOControl.PowerOnFingerprintPower( );

			// Start attendance task
			StartAttendanceTask( );
		}



		public async void StartAttendanceTask( )
		{
			fgCompare1N cmd = new fgCompare1N( );

			while ( true )
			{
				CommonResult ret = await FingerprintDevice.Execute( cmd );

				if ( ret.Code == ResultCode.GN_TASK_CANCEL )
					break;
				else if ( ret.Code == ResultCode.GN_SUCCESS )
					DistinguishUser( cmd.ID, DateTime.Now );
				else
					ShowNoFound( );
			}
		}

		public async Task StopAttendanceTask( )
		{
			await FingerprintDevice.CancelTask( );
		}

		public async void DistinguishUser( ushort fid, DateTime attendanceTime )
		{

			string pid = StorageControl.GetPerIDByFID( fid );

			if ( pid == string.Empty )
			{
				ShowNoFound( );
				return;
			}

			PerInfo pi = await StorageControl.ReadPerInfo( pid );

			if ( pi == null )
			{
				ShowNoFound( );
				return;
			}


			CommonRes.StorageControl.WriteAttendanceRecord( attendanceTime, fid );
			ShowAttendUser( pi.Name, pi.Department, attendanceTime );


			//var task = Dispatcher.RunAsync( CoreDispatcherPriority.High, ( ) =>
			//	{
			//		tblAttendanceMsg.Text = "用户：" + fid.ToString( ) + " 签到\r\n";
			//		tblAttendanceMsg.Text = tblAttendanceMsg.Text + attendanceTime.ToString( "HH:mm:ss" );
			//	}
			//);
		}

		private void ShowAttendUser( string UserName, string Department, DateTime attendTime )
		{
			CancleTimeTask( );

			txbName.Text = UserName;
			txbDepartment.Text = Department;
			txbAttendanceTime.Text = attendTime.ToString( "HH:mm:ss" );

			spReady.SetValue( Canvas.ZIndexProperty, 0 );
			spNoUser.SetValue( Canvas.ZIndexProperty, 0 );
			spAttend.SetValue( Canvas.ZIndexProperty, 1 );

			CreateMsgTimeTask( );
		}

		private void ShowNoFound( )
		{
			CancleTimeTask( );

			spAttend.SetValue( Canvas.ZIndexProperty, 0 );
			spReady.SetValue( Canvas.ZIndexProperty, 0 );
			spNoUser.SetValue( Canvas.ZIndexProperty, 1 );

			CreateMsgTimeTask( );
		}

		private void ShowReady( )
		{
			CancleTimeTask( );

			spNoUser.SetValue( Canvas.ZIndexProperty, 0 );
			spAttend.SetValue( Canvas.ZIndexProperty, 0 );
			spReady.SetValue( Canvas.ZIndexProperty, 1 );

			txbName.Text = "";
			txbDepartment.Text = "";
			txbAttendanceTime.Text = "";
		}

		ThreadPoolTimer msgTimer = null;

		/// <summary>
		/// 建立一个对话框隐藏控制 Timer 任务，超时 3 秒则关闭对话框
		/// </summary>
		private void CreateMsgTimeTask( )
		{
			msgTimer = ThreadPoolTimer.CreatePeriodicTimer( ReturnReady, TimeSpan.FromMilliseconds( 3000 ) );
		}

		private void ReturnReady( ThreadPoolTimer timer )
		{
			var task = Dispatcher.RunAsync( CoreDispatcherPriority.High, ( ) =>
			{ ShowReady( ); } );
		}

		/// <summary>
		/// 取消对话框控制 Timer 任务
		/// </summary>
		private void CancleTimeTask( )
		{
			if ( msgTimer != null )
				msgTimer.Cancel( );
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
			   txtDate.Text = DateTime.Now.ToString( "yyyy 年 MM 月 dd 日" );
			   txtTime.Text = DateTime.Now.ToString( "HH:mm:ss" );

			   if ( CommonRes.SystemWorkState == WorkState.RequestSync )
				   this.Frame.Navigate( typeof( SyncProgress ) );
		   }
			);
		}

		private void StopTimer( )
		{
			if ( t1 != null )
				t1.Cancel( );
		}

		#endregion

		private void appBarToggleButton_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( Config ) );
		}

		private void appBarToggleButton_Copy_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( DebugPage ) );
		}
	}
}
