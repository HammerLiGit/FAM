using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using PHR.PAM.Common;
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
	public sealed partial class SyncProgress : Page
	{
		public SyncProgress( )
		{
			this.InitializeComponent( );
		}

		protected override void OnNavigatedTo( NavigationEventArgs e )
		{
			CommonRes.SystemWorkState = CommonRes.WorkState.Syncing;
			StartTimer( );
		}

		protected override void OnNavigatingFrom( NavigatingCancelEventArgs e )
		{
			// On page leave
			StopTimer( );
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
				if ( CommonRes.SystemWorkState == WorkState.SyncOver )
					this.Frame.Navigate( typeof( AttendanceReady ) );
			}
			);
		}

		private void StopTimer( )
		{
			if ( t1 != null )
				t1.Cancel( );
		}

		#endregion


	}
}
