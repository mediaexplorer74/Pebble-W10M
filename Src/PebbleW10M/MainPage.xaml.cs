/*
    Copyright (C) 2016  Eduardo Elías Noyer Silva

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Phone.Notification.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using P3bble.Core.Types;
using Windows.Storage;
using PebbleNotificationWatcher;
using PebbleWatch.Models;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.Graphics.Imaging;
using Windows.Networking.Proximity;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Newtonsoft.Json.Linq;

namespace PebbleWatch
{
    public sealed partial class MainPage : Page
    {
        CancellationTokenSource backgroundToken;
        bool replaceTasks;

        public List<AppItem> ItemList { get; set; }
        public List<PebbleAppItem> PebbleAppList { get; set; }

        public MainPage()
        {
            LittleWatson.CheckForPreviousException();
            this.InitializeComponent();
            ApplicationData.Current.LocalSettings.Values["Name"] = null;
            ApplicationData.Current.LocalSettings.Values["Version"] = null;

            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            Application.Current.UnhandledException += Current_UnhandledException;
        }

        private async void Current_Resuming(object sender, object e)
        {
            bool? firstTime = ApplicationData.Current.LocalSettings.Values["firstTime"] as bool?;
            ApplicationData.Current.LocalSettings.Values["Name"] = null;
            ApplicationData.Current.LocalSettings.Values["Version"] = null;
            if (firstTime == null || firstTime == true)
            {
                //User must press the Connect button
            }
            else
            {
                backgroundToken.Cancel(false);
                PebbleName.Text = "Connecting...";
                await initPebbleApp(false); //It is not necessary to register the app at all times
                Application.Current.Suspending += Current_Suspending;
            }
        }

        private void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LittleWatson.ReportException(e.Exception, "UnhandledException ");
            Debug.WriteLine(e.Message);
            e.Handled = true;
        }


        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            try
            {
                Debug.WriteLine("Suspending!!!!");
                bool? firstTime = ApplicationData.Current.LocalSettings.Values["firstTime"] as bool?;
                if (firstTime == null || firstTime == true)
                {
                    // User must press the Connect button
                }
                else
                {
                    backgroundToken = new CancellationTokenSource();

                    Application.Current.Suspending -= Current_Suspending;
                    PebbleName.Text = "";
                    PebbleVersion.Text = "";

                    if (applicationtrigger != null)
                    {
                        await Task.Run(() =>
                        {
                            // Simulate background task execution
                            // Replace this with actual asynchronous logic if needed
                            Debug.WriteLine("Background task executed during suspension.");
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool? firstTime = ApplicationData.Current.LocalSettings.Values["firstTime"] as bool?;
            if (firstTime == null || firstTime == true)
            {
                //User must press the Connect button
            }
            else
            {
                PebbleRegisterAcc.Visibility = Visibility.Collapsed;
                PebbleName.Text = "Connecting...";
                await initPebbleApp();
            }
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //await Task.Run(() => { NotificationReciever.DisposePebble(); });
            Debug.WriteLine("Unloaded!");
        }

        string deviceId = null;
        
        
        private async Task<bool> initPebbleApp(bool registerAllApps = true)
        {

            #region BTLE
            var watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += Watcher_Received;
            watcher.Start();

            #endregion

            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";

            try
            {
                var peers = await PeerFinder.FindAllPeersAsync();
                bool isPebblePaired = false;
                foreach (var item in peers)
                {
                    if (item.DisplayName.StartsWith("Pebble", StringComparison.OrdinalIgnoreCase))
                    {
                        isPebblePaired = true;

                        deviceId =  (await BluetoothDevice.FromHostNameAsync(item.HostName)).DeviceId;

                        //foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(Guid.Parse("00000000-deca-fade-deca-deafdecacaff")))))
                        //{                            
                        //   deviceId = di.Id;
                        //}

                        //deviceId = (await Windows.Devices.Bluetooth.BluetoothDevice.FromHostNameAsync(item.HostName)).DeviceInformation.Id.ToString();
                        //deviceId = item.HostName;
                        //deviceId = Guid.Parse("00000000-deca-fade-deca-deafdecacaff").ToString("B");
                        break;
                    }
                }

                if (!isPebblePaired)
                {
                    throw new Exception("NoPebble");
                }

                PebbleData pData = await Task.Run(async () =>
                {
                    if (ItemList == null)
                        ItemList = new List<AppItem>();
                    RegisterBackgrondPebbleWatchTask(replaceTasks, registerAllApps);

                    while (applicationtrigger == null)
                    {
                        //Wait until the task is not null...
                    }
                    backgroundToken = new CancellationTokenSource();
                    if (backgroundToken == null)
                        backgroundToken = new CancellationTokenSource();

                    bool errorInit = false;
                    do
                    {
                        try
                        {
                            //RnD
                            await applicationtrigger.RequestAsync().AsTask(backgroundToken.Token);
                            //await BackgroundTaskInit.TryConnection();
                            errorInit = false;
                        }
                        catch (Exception ex)
                        {
                            errorInit = true;
                            Debug.WriteLine(ex.Message);
                        }
                    } while (errorInit);
                    //ApplicationData.Current.LocalSettings.Values["Name"] = "Pebble test";
                    //ApplicationData.Current.LocalSettings.Values["Version"] = "Version!";
                    return await GetPebbleData();
                    //return await PebbleNotificationWatcher.NotificationReciever.GetInfoFromPebble();                   
                    //No 
                });

                PebbleName.Text = (PebbleName.Text == "" 
                    || PebbleName.Text == "Not Available" || PebbleName.Text == "Connecting...")
                    ? pData.Name
                    : PebbleName.Text;

                PebbleVersion.Text = (PebbleVersion.Text == "" 
                    || PebbleVersion.Text == "Not Available" 
                    || PebbleVersion.Text == "Connecting...") 
                    ? pData.Version
                    : PebbleVersion.Text;
                GetPebbleApps();
                successfullinit = true;
                backgroundToken.Cancel(false);
                
            }
            catch (Exception ex)
            {
                if (
                    (uint)ex.HResult == 0x8007048F 
                    || ex.HResult == -2147483634
                    || ex.Message == "NoPebble"
                    )
                {
                    ApplicationData.Current.LocalSettings.Values["firstTime"] = true;
                    var dg = new Windows.UI.Popups.MessageDialog(
                        "Please turn on BT and pair your Pebble, then Reconnect");
                    await dg.ShowAsync();
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:///"));
                    successfullinit = false;
                }
            }
            return successfullinit;
        }


        
        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, 
            BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // The timestamp of the event
            DateTimeOffset timestamp = args.Timestamp;

            // The type of advertisement
            BluetoothLEAdvertisementType advertisementType = args.AdvertisementType;

            // The received signal strength indicator (RSSI)
            Int16 rssi = args.RawSignalStrengthInDBm;

            // The local name of the advertising device contained within the payload, if any
            string localName = args.Advertisement.LocalName;

            // Check if there are any manufacturer-specific sections.
            // If there is, print the raw data of the first manufacturer section (if there are multiple).
            string manufacturerDataString = "";
            var manufacturerSections = args.Advertisement.ManufacturerData;
            if (manufacturerSections.Count > 0)
            {
                // Only print the first one of the list
                var manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);
                }
                // Print the company ID + the raw data in hex format
                manufacturerDataString = string.Format("0x{0}: {1}",
                    manufacturerData.CompanyId.ToString("X"),
                    BitConverter.ToString(data));
                Debug.WriteLine(manufacturerDataString);
            }
        }

        private async Task<PebbleData> GetPebbleData()
        {
            NotificationReciever.GetInfoFromPebble();
            PebbleData pData = await Task<PebbleData>.Run<PebbleData>(() =>
            {
                object name = null, version = null;
                do
                {
                    name = ApplicationData.Current.LocalSettings.Values["Name"];                    
                    version = ApplicationData.Current.LocalSettings.Values["Version"];
                    Task.Delay(1); //Delay para dejar trabajar a los demas
                } while (name == null || version == null);

                return new PebbleData()
                {
                    Version = version as string,
                    Name = name as string
                };
            }
            );         

            return pData;
        }

        private void GetPebbleApps()
        {
            //I get the information of the apps installed on the pebble
            NotificationReciever.GetAppsFromPebble();
            bool isNull = true;
            JToken ia = null;
            do
            {
                object InstalledApplications = 
                    ApplicationData.Current.LocalSettings.Values["InstalledApplications"];

                if (InstalledApplications == null)
                    continue;
                else
                {
                    ia = JObject.Parse(InstalledApplications.ToString());
                    break;                    
                }
            } while (isNull);
            if (PebbleAppList == null)
                PebbleAppList = new List<PebbleAppItem>();

            PebbleAppList.Clear();
            if (ia != null)
            {
                foreach (var item in ia["ApplicationsInstalled"])
                {
                    PebbleAppList.Add(new PebbleAppItem(item["Name"].ToString(),item["Id"].ToString()));
                }
            }

            lvpItems.ItemsSource = PebbleAppList;
        }

        private async void MusicControlReceived(MusicControlAction action)
        {
            Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog(action.ToString());
            await md.ShowAsync();
        }

        public static bool successfullinit;
        private async void PebbleRegisterAcc_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                PebbleName.Text = "Connecting...";
                ApplicationData.Current.LocalSettings.Values["firstTime"] = false;
                await initPebbleApp();

                if (successfullinit)
                {
                    PebbleRegisterAcc.Visibility = Visibility.Collapsed;
                    PebbleDisconnect.Visibility = Visibility.Visible;
                }
                else
                {
                    PebbleRegisterAcc.Visibility = Visibility.Visible;
                    PebbleDisconnect.Visibility = Visibility.Collapsed;
                    PebbleName.Text = "";
                }
            });
        }

        private void registerTask(int taskID)
        {
            try
            {
                AccessoryManager.EnableAccessoryNotificationTypes((int)taskID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Sorry, cant register:" + taskID + " (" + ex.Message + ")");
            }
        }


        ApplicationTrigger applicationtrigger;
        // Replace the usage of DeviceManufacturerNotificationTrigger with an alternative approach
        // as it is deprecated. Use a different trigger type or refactor the logic to avoid its usage.

        private /*async*/ void RegisterBackgrondPebbleWatchTask(bool replaceTask, bool registerAllApps = true)
        {
            try
            {
                // Register the app as an app.of accessory, so we get access to the Windows notification center.
                string str = AccessoryManager.RegisterAccessoryApp();
                IReadOnlyDictionary<String, AppNotificationInfo> apps = AccessoryManager.GetApps();

                // Enable the app to listen to certain types of notifications.
                Int32 enabledAccessoryNotificationTypes = AccessoryManager.GetEnabledAccessoryNotificationTypes();
                Debug.WriteLine("Enabled Notif types: " + enabledAccessoryNotificationTypes);

                // Check if the task is already registered
                var taskRegistered = false;
                var taskName = "NotificationReciever";
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name.Equals(taskName))
                    {
                        taskRegistered = true;
                        if (replaceTask)
                        {
                            task.Value.Unregister(true); // Always clean the task
                            taskRegistered = false;
                        }
                    }
                }

                if (!taskRegistered)
                {
                    var builder = new BackgroundTaskBuilder();
                    builder.Name = taskName;
                    builder.TaskEntryPoint = "NotificationWatcher.NotificationReciever";

                    // Use an alternative trigger, such as ApplicationTrigger, instead of DeviceManufacturerNotificationTrigger
                    applicationtrigger = new ApplicationTrigger();
                    builder.SetTrigger(applicationtrigger);

                    Debug.WriteLine("Building Task");
                    BackgroundTaskRegistration task = builder.Register();
                    Debug.WriteLine("Task Registered Id: " + task.TaskId);
                }
                else
                {
                    Debug.WriteLine("Task already registered");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: " + ex.Message);
            }
        }

        private async void PebbleSendPing_Click(object sender, RoutedEventArgs e)
        {
            lblStatusTest.Text = "Sending...";
            await Task.Run(() =>
            {
                if (isTaskRegistered())
                    PebbleNotificationWatcher.NotificationReciever.SendPing();
                else
                {
                    showConnectionWarningMessage();
                }
            });
            await Task.Delay(1000);
            lblStatusTest.Text = "";
            //NotificationReciever.DisposePebble();
        }

        private async void PebbleSendCall_Click(object sender, RoutedEventArgs e)
        {
            lblStatusTest.Text = "Sending...";
            await Task.Run(() =>
            {
                if (isTaskRegistered())
                    PebbleNotificationWatcher.NotificationReciever.SendTestCall("Eduardo Noyer", "555-555-555");
                else
                    showConnectionWarningMessage();
            });
            await Task.Delay(1000);
            lblStatusTest.Text = "";
            //NotificationReciever.DisposePebble();
        }

        private async void PebbleSendSMS_Click(object sender, RoutedEventArgs e)
        {
            lblStatusTest.Text = "Sending...";
            await Task.Run(() =>
            {
                if (isTaskRegistered())
                    PebbleNotificationWatcher.NotificationReciever.SendSMSTest("PebbleWatch", 
                        "SMS from Windows Phone to your Pebble!!, yei!");
                else
                    showConnectionWarningMessage();
            });
            await Task.Delay(1000);
            lblStatusTest.Text = "";
            //NotificationReciever.DisposePebble();
        }

        private async void PebbleNotificationTest_Click(object sender, RoutedEventArgs e)
        {
            lblStatusTest.Text = "Sending...";
            await Task.Run(() =>
            {
                if (isTaskRegistered())
                    PebbleNotificationWatcher.NotificationReciever.SendFBTest("PebbleWatch",
                        "Hello!, have a nice day!");
                else
                    showConnectionWarningMessage();
            });
            await Task.Delay(1000);
            lblStatusTest.Text = "";
            //NotificationReciever.DisposePebble();
        }

        private async void PebbleSetCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            lblStatusTest.Text = "Sending...";
            await Task.Run(() =>
            {
                if (isTaskRegistered())
                    PebbleNotificationWatcher.NotificationReciever.SetCurrentTime();
                else
                    showConnectionWarningMessage();
            });
            await Task.Delay(1000);
            lblStatusTest.Text = "";
            //NotificationReciever.DisposePebble();
        }

        private bool isTaskRegistered()
        {
            try
            {
                //First we check if we have a running background task using our pebble connection
                //If it's possible, we call some functions from this app to the background class.                 
                //We set a name to identify this task
                var taskName = "NotificationReciever";
                //And search for it in the AllTasks Object
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name.Equals(taskName))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = (sender as ToggleSwitch);
            var ID = toggle.Tag as string;
            if (toggle.IsOn)
            {
                AccessoryManager.EnableNotificationsForApplication(
                    ID);
                Debug.WriteLine("### Enabled App: " + AccessoryManager.IsNotificationEnabledForApplication(ID));
            }
            else
            {
                AccessoryManager.DisableNotificationsForApplication(
                    ID);
                Debug.WriteLine("### Disabled App: " + AccessoryManager.IsNotificationEnabledForApplication(ID));
            }
        }

        private async Task<WriteableBitmap> Convert(IRandomAccessStreamReference parameter)
        {
            IRandomAccessStreamWithContentType streamContent;
            if (parameter != null)
            {
                BitmapImage bmi = new BitmapImage();
                using (streamContent = await parameter.OpenReadAsync())
                {
                    if (streamContent != null)
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(streamContent);
                        BitmapFrame frame = await decoder.GetFrameAsync(0);

                        var bitmap = new WriteableBitmap((int)frame.PixelWidth, (int)frame.PixelHeight);
                        streamContent.Seek(0);

                        await bitmap.SetSourceAsync(streamContent);
                        return bitmap;
                    }
                }
            }
            return new WriteableBitmap(40, 40);
        }

        private async void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            var m = new Windows.UI.Popups.MessageDialog(
                "PebbleWatch 1.0.2\n© 2015-2025 Eduardo Noyer \n\n@Noyer\n\nAll rights reserved.\n\n",
                "About this app");
            await m.ShowAsync();
        }

        private async void PebbleDisconnect_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PebbleName.Text = "Disonnecting...";
                PebbleVersion.Text = "";
                ApplicationData.Current.LocalSettings.Values["firstTime"] = true;                
                backgroundToken.Cancel(false);
                #region Unregister Task
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name.Equals("NotificationReciever")||
                     task.Value.Name.Equals("AppTrigger") ||
                     task.Value.Name.Equals("SocketInput")
                     )
                    {
                        task.Value.Unregister(true);//Always clean the task
                    }
                    break;
                }
                #endregion

                PebbleDisconnect.Visibility = Visibility.Collapsed;
                PebbleRegisterAcc.Visibility = Visibility.Visible;

                PebbleName.Text = "";
                replaceTasks = true;
            });
        }

        private async void showConnectionWarningMessage()
        {
            // ensure that the showConnectionWarningMessage method is always called on the UI thread 
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var md = new Windows.UI.Popups.MessageDialog("First, Connect to your Pebble");
                await md.ShowAsync();
            });
        }

        private async void btnLicense_Click(object sender, RoutedEventArgs e)
        {
            string str = @"License

Copyright (c) 2014, Steve Robbins.
Copyright (c) 2013-2014, p3root - Patrik Pfaffenbauer (patrik.pfaffenbauer@p3.co.at) All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

Neither the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.";

            var md = new Windows.UI.Popups.MessageDialog(str, "License");
            await md.ShowAsync();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            //I delete the selected app
            Button btn = sender as Button;            
            NotificationReciever.RemoveWatchApp(btn.Tag as string);
            GetPebbleApps(); //I'm getting my apps back.
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            //I launch the app according to your ID
            Button btn = sender as Button;
            NotificationReciever.LaunchWatchApp(btn.Tag as string);
        }
    }
}
