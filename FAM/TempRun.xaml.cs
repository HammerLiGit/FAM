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
using Windows.Storage;
using Windows.Devices.Gpio;
using PHR.PAM.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PAM
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class TempRun : Page
	{
		public TempRun( )
		{
			this.InitializeComponent( );
		}

		private async void button_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				StorageFolder fl = ApplicationData.Current.LocalFolder;
				StorageFolder datafl;
				try
				{
					datafl = await fl.GetFolderAsync( "AttenData" );
				}
				catch
				{
					datafl = await fl.CreateFolderAsync( "AttenData" );
				}


				string fname = DateTime.Now.ToString( "yyyy-MM-dd" ) + ".pamj";
				StorageFile rf;
				try
				{
					rf = await datafl.GetFileAsync( fname );
				}
				catch
				{
					rf = await datafl.CreateFileAsync( fname );
				}

				using ( Stream ss = await rf.OpenStreamForWriteAsync( ) )
				{
					ss.Seek( 0, SeekOrigin.End );

					string s = DateTime.Now.ToString( "yyyyMMddHHmmss" ) + "\r\n";
					byte[ ] b = System.Text.Encoding.ASCII.GetBytes( s );

					await ss.WriteAsync( b, 0, b.Length );

					await ss.FlushAsync( );
				}

			}
			catch ( Exception ex )
			{
				textBlock.Text = ex.Message;
			}

		}

		private async void button_Copy_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				StorageFolder fl = ApplicationData.Current.LocalFolder;
				StorageFolder datafl = await fl.GetFolderAsync( "AttenData" );

				if ( datafl == null ) return;

				string fname = DateTime.Now.ToString( "yyyy-MM-dd" ) + ".pamj";
				StorageFile rf = await datafl.GetFileAsync( fname );
				if ( rf == null ) return;

				textBlock.Text = "";
				using ( Stream ss = await rf.OpenStreamForReadAsync( ) )
				{
					byte[ ] contant = new byte[14];
					int retlen = await ss.ReadAsync( contant, 0, 14 );

					while ( retlen > 0 )
					{
						textBlock.Text += System.Text.Encoding.ASCII.GetString( contant );

						contant = new byte[14];
						retlen = await ss.ReadAsync( contant, 0, 14 );
					}

				}

			}
			catch ( Exception ex )
			{
				textBlock.Text = ex.Message;
			}


		}



		private void button1_Click( object sender, RoutedEventArgs e )
		{
			//CommonRes.GPIOControl.ResetFingerprintPower( true );
		}

		private void button2_Click( object sender, RoutedEventArgs e )
		{
			//CommonRes.GPIOControl.ResetFingerprintPower( false );
		}

		private void backButton_Click( object sender, RoutedEventArgs e )
		{
			this.Frame.Navigate( typeof( MainPage ) );

		}
	}
}
