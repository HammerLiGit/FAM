using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PHR.PAM.Command;
using PHR.PAM.Common;
using PHR.PAM.fgCommand;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using static PHR.PAM.Common.CommonRes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PAM
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Config : Page
	{
		private Random rr = new Random( );

		public Config( )
		{
			this.InitializeComponent( );
		}

		protected override async void OnNavigatedTo( NavigationEventArgs e )
		{
			await LoadFingprinterInfo( );
		}

		private void appBarToggleButton_Checked( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( AttendanceReady ) );
		}

		/// <summary>
		/// 导航按钮事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tlbNav_Click( object sender, RoutedEventArgs e )
		{
			for ( int i = 0; i < spNav.Children.Count; i++ )
			{
				( spNav.Children[i] as ToggleButton ).IsChecked = false;
			}

			( sender as ToggleButton ).IsChecked = true;

			if ( ( sender as ToggleButton ).Name == "tlbFingerprintManage" )
			{
				spServer.SetValue( Canvas.ZIndexProperty, 0 );
				spFingerprint.SetValue( Canvas.ZIndexProperty, 1 );
			}
			else if ( ( sender as ToggleButton ).Name == "tlbServerConfig" )
			{
				spFingerprint.SetValue( Canvas.ZIndexProperty, 0 );
				spServer.SetValue( Canvas.ZIndexProperty, 1 );
			}
		}

		/// <summary>
		/// 读取人员指纹信息到列表
		/// </summary>
		/// <returns></returns>
		private async Task LoadFingprinterInfo( )
		{
			// 判断当前列表的数据源是否已经指定
			if ( lsvUser.ItemsSource == null )
			{
				// 建立数据源
				lsvUser.ItemsSource = new custPerDisplayItems( );
			}
			// 获取数据源连接
			custPerDisplayItems dit = lsvUser.ItemsSource as custPerDisplayItems;

			// 清理数据源
			dit.Clear( );
			PerInfoList user = await StorageControl.GetPerList( );
			// 添加数据源
			for ( int i = 0; i < user.Count; i++ )
			{
				dit.Add( new custPerDisplayItem( ( user[i].FID == "" ? "" : "ms-appx:Assets/FingerprintIcon.png" ), user[i].Name, user[i].PID, user[i].FID ) );
			}
		}

		private void lsvUser_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{

		}

		private async void btnDelete_Click( object sender, RoutedEventArgs e )
		{
			if ( lsvUser.SelectedItem == null )
			{
				await InfoDialog.ShowOK( "请选择需要删除指纹信息的人员。" );
				return;
			}

			if ( ( lsvUser.SelectedItem as custPerDisplayItem ).FID != "" )
			{
				if ( await InfoDialog.ShowYesNo( "确定删除指定人员的指纹信息吗？" ) == ContentDialogResult.Secondary )
					return;

				await GPIOControl.PowerOnFingerprintPower( );
				try
				{
					// Delete from fingerprint device
					fgRemoveFingerprint cmd = new fgRemoveFingerprint( Convert.ToUInt16( ( lsvUser.SelectedItem as custPerDisplayItem ).FID.Trim( ) ) );
					CommonResult ret = await FingerprintDevice.Execute( cmd );

					// Update compare file
					await StorageControl.UpdateFIDByPID( ( lsvUser.SelectedItem as custPerDisplayItem ).PID, string.Empty );
					await LoadFingprinterInfo( );
				}
				catch ( Exception ex )
				{
					throw new CommonResult( ResultCode.GN_SYS_ERROR, ex.Message );
				}
				finally
				{
					GPIOControl.PowerOffFingerprintPower( );
				}
			}
			else
				await InfoDialog.ShowOK( "指定的人员没有记录指纹信息。" );

		}

		private async void btnAdd_Click( object sender, RoutedEventArgs e )
		{
			if ( lsvUser.SelectedItem == null )
			{
				await InfoDialog.ShowOK( "请选择需要录入指纹信息的人员。" );
				return;
			}

			string f = ( lsvUser.SelectedItem as custPerDisplayItem ).FID;
			if ( f == "" )
				f = ( lsvUser.SelectedItem as custPerDisplayItem ).PID;
			else
			{
				if ( await InfoDialog.ShowYesNo( "指定人员已经具有指纹信息，重新采集指纹将删除之前的指纹信息，确定需要更新指纹信息吗？" ) == ContentDialogResult.Secondary )
					return;

				fgRemoveFingerprint cmd = new fgRemoveFingerprint( Convert.ToUInt16( f ) );
				CommonResult ret = await FingerprintDevice.Execute( cmd );
			}

			FingerprintCollectDialog dialog = new FingerprintCollectDialog( Convert.ToUInt16( f ) );

			await dialog.ShowAsync( );

			if ( dialog.DialogResult.Code == ResultCode.GN_SUCCESS )
			{
				await StorageControl.UpdateFIDByPID( ( lsvUser.SelectedItem as custPerDisplayItem ).PID, f );
				await LoadFingprinterInfo( );
			}
		}

	}

	public class custPerDisplayItem
	{
		public string image { get; set; }
		public string Name { get; set; }
		public string PID { get; set; }

		public string FID { get; set; }

		public custPerDisplayItem( string img, string name, string pid, string fid )
		{
			image = img;
			Name = name;
			PID = pid;
			FID = fid;
		}

	}
	public class custPerDisplayItems : ObservableCollection<custPerDisplayItem> { }
}
