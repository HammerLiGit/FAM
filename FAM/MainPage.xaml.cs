using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PAM
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage( )
		{
			this.InitializeComponent( );
		}

		private void btnDisconnect_Copy_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( DebugPage ) );
		}

		private void btnDisconnect_Copy1_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( TempRun ) );
		}

		private async void button_Click( object sender, RoutedEventArgs e )
		{
			InfoDialog dia = new InfoDialog( );
			ContentDialogResult ret = await dia.ShowAsync( );

		}

		private void btnDisconnect_Copy2_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( AttendanceReady ) );
		}
	}
}
