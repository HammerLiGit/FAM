using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using PHR.PAM.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PAM
{
	public sealed partial class InfoDialog : ContentDialog
	{
		public static async Task<ContentDialogResult> ShowYesNo( string title )
		{
			InfoDialog dlg = new InfoDialog( );
			dlg.Title = title;

			dlg.PrimaryButtonText = "确  定";
			dlg.SecondaryButtonText = "取  消";

			dlg.IsPrimaryButtonEnabled = true;
			dlg.IsSecondaryButtonEnabled = true;

			return await dlg.ShowAsync( );
		}

		public static async Task<ContentDialogResult> ShowOK( string title )
		{
			InfoDialog dlg = new InfoDialog( );
			dlg.Title = title;

			dlg.PrimaryButtonText = "";
			dlg.SecondaryButtonText = "确  定";

			dlg.IsPrimaryButtonEnabled = false;
			dlg.IsSecondaryButtonEnabled = true;

			return await dlg.ShowAsync( );
		}


		public InfoDialog( )
		{
			this.InitializeComponent( );
		}


		private void ContentDialog_Closed( ContentDialog sender, ContentDialogClosedEventArgs args )
		{

		}

		private void ContentDialog_Closing( ContentDialog sender, ContentDialogClosingEventArgs args )
		{

		}

		private void ContentDialog_Opened( ContentDialog sender, ContentDialogOpenedEventArgs args )
		{
		}

		private void ContentDialog_PrimaryButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
		{

		}

		private void ContentDialog_SecondaryButtonClick( ContentDialog sender, ContentDialogButtonClickEventArgs args )
		{

		}

	}
}
