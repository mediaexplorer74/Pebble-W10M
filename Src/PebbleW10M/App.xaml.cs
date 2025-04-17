using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PebbleWatch
{
    sealed partial class App : Application
    {
      
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
         
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait;
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

         
            if (rootFrame == null)
            {
                
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //
                }

               
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
              
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
          
            Window.Current.Activate();
        }

      
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            LittleWatson.ReportException(e.Exception, "Failed to load Page " + e.SourcePageType.FullName);
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

       
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //         
            deferral.Complete();
        }

        

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;               
               
                
                // TODO: Handle URI activation
                // The received URI is eventArgs.Uri.AbsoluteUri
                Windows.Web.Http.HttpClient c = new Windows.Web.Http.HttpClient();

                var response = await c.GetAsync(new Uri(@"https://api2.getpebble.com/v2/apps/id" 
                          + eventArgs.Uri.AbsolutePath));

                string jsonRespone = await response.Content.ReadAsStringAsync();

                JObject obj = JObject.Parse(jsonRespone);
                try
                {
                    string link = obj["data"][0]["latest_release"]["pbw_file"].ToString();
                    PebbleNotificationWatcher.NotificationReciever.InstallWatchApp(link);
                 /*   return;
                    var r = await c.GetAsync(new Uri(link));
                    var download = await r.Content.ReadAsBufferAsync();

                    var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                    savePicker.SuggestedStartLocation =
                        Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    // Dropdown of file types the user can save the file as
                    savePicker.FileTypeChoices.Add("Pebble File", new List<string>() { ".pbw" });
                    // Default file name if the user does not type one in or select a file to replace
                    savePicker.SuggestedFileName = obj["data"][0]["title"].ToString();
                    Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until
                        // we finish making changes and call CompleteUpdatesAsync.
                        Windows.Storage.CachedFileManager.DeferUpdates(file);
                        // write to file
                        await Windows.Storage.FileIO.WriteBufferAsync(file, download);
                        // Let Windows know that we're finished changing the file so
                        // the other app can update the remote version of the file.
                        // Completing updates may require Windows to ask for user input.
                        Windows.Storage.Provider.FileUpdateStatus status =
                            await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                        
                    }
                 */
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("App.xaml.cs exception:  " + ex.Message);
                }

            }
        }
    }
}
