using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PHR.PAM.Common;
using PHR.PAM.fgCommand;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static PHR.PAM.Common.CommonRes;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PAM
{
	public sealed partial class FingerprintCollectDialog : ContentDialog
	{
		private int _step = 0;
		private ushort _fid;

		private CommonResult resultCode = null;

		public CommonResult DialogResult { get { return resultCode; } set { resultCode = value; } }

		public FingerprintCollectDialog( ushort fid )
			: base( )
		{
			this.InitializeComponent( );
			this.Opened += Dialog_Opened;
			this.Closing += Dialog_Closing;

			_fid = fid;
		}

		private async void Dialog_Opened( ContentDialog sender, ContentDialogOpenedEventArgs args )
		{
			await GPIOControl.PowerOnFingerprintPower( );

			DoCollect( );
		}

		private void Dialog_Closing( ContentDialog sender, ContentDialogClosingEventArgs args )
		{
			GPIOControl.PowerOffFingerprintPower( );
		}

		private async void DoCollect( )
		{
			fgCollectFingerprint cmd = null;

			// First collect
			this.Title = "请扫描指纹";
			first:
			brdOne.Background = new SolidColorBrush( Colors.LightBlue );
			brdTwo.Background = new SolidColorBrush( Colors.White );
			brdThree.Background = new SolidColorBrush( Colors.White );
			while ( true )
			{
				cmd = new fgCollectFingerprint( 1, _fid, 1 );
				DialogResult = await FingerprintDevice.Execute( cmd );

				if ( DialogResult.Code == ResultCode.GN_TASK_CANCEL )
					return;
				else if ( DialogResult.Code == ResultCode.GN_SUCCESS )
					break;
				else
					this.Title = DialogResult.Message;
			}

			// Second collect
			this.Title = "请扫描第二遍指纹";
			brdOne.Background = new SolidColorBrush( Colors.LightGreen );
			brdTwo.Background = new SolidColorBrush( Colors.LightBlue );
			brdThree.Background = new SolidColorBrush( Colors.White );
			while ( true )
			{
				cmd = new fgCollectFingerprint( 2, _fid, 1 );
				DialogResult = await FingerprintDevice.Execute( cmd );

				if ( DialogResult.Code == ResultCode.GN_TASK_CANCEL )
					return;
				else if ( DialogResult.Code == ResultCode.GN_SUCCESS )
					break;
				else if ( DialogResult.Code == ResultCode.FG_BOARD_FAIL )
				{
					this.Title = "指纹采集失败，请重新扫描指纹";
					goto first;
				}
				else
					this.Title = DialogResult.Message;
			}

			if ( DialogResult.Code == ResultCode.GN_TASK_CANCEL ) return;

			// Last collect
			this.Title = "请扫描第三遍指纹";
			brdOne.Background = new SolidColorBrush( Colors.LightGreen );
			brdTwo.Background = new SolidColorBrush( Colors.LightGreen );
			brdThree.Background = new SolidColorBrush( Colors.LightBlue );
			while ( true )
			{
				cmd = new fgCollectFingerprint( 3, _fid, 1 );
				DialogResult = await FingerprintDevice.Execute( cmd );

				if ( DialogResult.Code == ResultCode.GN_TASK_CANCEL )
					return;
				else if ( DialogResult.Code == ResultCode.GN_SUCCESS )
					break;
				else if ( DialogResult.Code == ResultCode.FG_BOARD_FAIL )
				{
					this.Title = "指纹采集失败，请重新扫描指纹";
					goto first;
				}
				else
					this.Title = DialogResult.Message;
			}

			if ( DialogResult.Code == ResultCode.GN_TASK_CANCEL ) return;

			this.Hide( );

		}

		private async void ContentDialog_CloseButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
		{
			await FingerprintDevice.CancelTask( );
			this.DialogResult = new CommonResult( ResultCode.GN_TASK_CANCEL );
		}

		private int Step
		{
			get
			{
				return _step;
			}

			set
			{
				switch ( value )
				{
					case 1:


						break;
					case 2:
						break;
					case 3:
						break;

					default: break;
				}
			}
		}


	}
}
