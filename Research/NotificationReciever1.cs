// NotificationReciever in PebbleW10M project aka NotificationReciever1 (Experimental)

using P3bble.Core;
using P3bble.Core.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.Phone.Notification.Management;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace PebbleWatch
{
    /// <summary>
    /// This class is going to handle all the background operations
    /// Geting the new notification and send it as a message to the pebble
    /// In the future, this will handle all the music/media actions triggered
    /// by the Pebble
    /// </summary>
    public sealed class NotificationReciever1 : IBackgroundTask
    {
        //BackgroundTaskDeferral deferral;
        public static string str;

        //Experimental
        static Guid btr = default;
        static StreamSocket socket = null;

        public static P3bble.Core.P3bble _pebble;



        public static async void NotConnected()
        {
            _pebble = null;
            //RetryConnection.Visibility = Visibility.Visible;
            await TryConnection();
        }

        public static async Task TryConnection()
        {
            P3bble.Core.P3bble.IsMusicControlEnabled = true;
            P3bble.Core.P3bble.IsLoggingEnabled = true;

            List<P3bble.Core.P3bble> pebbles = await P3bble.Core.P3bble.DetectPebbles();

            if (pebbles.Count >= 1)
            {
                _pebble = pebbles[0];
                await _pebble.ConnectAsync(btr, socket);

                if (_pebble != null && _pebble.IsConnected)
                {
                    _pebble.MusicControlReceived += new MusicControlReceivedHandler(MusicControlReceived);
                    //_pebble.InstallProgress += new InstallProgressHandler(this.InstallProgressReceived);

                    //if (_pebble.DisplayName != null)
                    //    PebbleName.Text = "Connected to Pebble " + _pebble.DisplayName;
                    //if (_pebble.FirmwareVersion != null)
                    //    PebbleVersion.Text = "Version " + _pebble.FirmwareVersion.Version + " - " + _pebble.FirmwareVersion.Timestamp.ToString("ddMMYYY");
                    //RetryConnection.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NotConnected();
                }
            }
        }

        public static /*async*/ void MusicControlReceived(MusicControlAction action)
        {
            //Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog(action.ToString());
            //await md.ShowAsync();
            AccessoryManager.IncreaseVolume(1);
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("TESTSTSTST");
        }

        internal static void GetInfoFromPebble()
        {
            //addTestToStack(5);
            return;
        }

        //public async void Run(IBackgroundTaskInstance taskInstance)
        //{


        //    deferral = taskInstance.GetDeferral();
        //    try
        //    {
        //        //First we connect to the pebble
        //        if(str==null)
        //            str = AccessoryManager.RegisterAccessoryApp();
        //        //If we want to so use await methods on this class we must
        //        //get the deferral so this will prevent the class from terminate
        //        //before the current async operation finish.



        //        #region Test Get Notification
        //        //I get the following notification trigger

        //        IAccessoryNotificationTriggerDetails nextTriggerDetails;
        //        do
        //        {
        //            nextTriggerDetails = AccessoryManager.GetNextTriggerDetails();
        //        } while (nextTriggerDetails == null);
        //        //Process what brings this notification
        //        if (nextTriggerDetails != null)
        //        {
        //            AccessoryManager.ProcessTriggerDetails(nextTriggerDetails);
        //        }
        //        //I can make a filtering of the notifications that I want to see
        //        // if (!this.TriggerDetailsTobeIgnored(nextTriggerDetails))
        //        // {
        //        // }
        //        //I go through the details, as long as they are not null
        //        while (nextTriggerDetails != null)
        //        {
        //            Debug.WriteLine("I got a trigger!");
        //            Debug.WriteLine("Name of notification app: " + nextTriggerDetails.AppDisplayName);
        //            Debug.WriteLine("Type of notification: " + nextTriggerDetails.AccessoryNotificationType);

        //            if (_pebble != null && _pebble.IsConnected)
        //            {
        //                //_pebble.FacebookNotificationAsync(nextTriggerDetails.AppDisplayName, nextTriggerDetails.AccessoryNotificationType.ToString());
        //                await _pebble.PingAsync();
        //            }
        //            else
        //            {
        //                await TryConnection();
        //            }


        //                //Then according to the type of notification, with a switch-case I performed the actions
        //                //switch (nextTriggerDetails.AccessoryNotificationType)
        //                //{
        //                //    case AccessoryNotificationType.Phone:
        //                //        //Type of telephone notification, it may be of another type more
        //                //        PhoneNotificationTriggerDetails phonenotificationTriggerDetail = nextTriggerDetails as PhoneNotificationTriggerDetails;
        //                //        switch (phonenotificationTriggerDetail.AccessoryNotificationType)
        //                //        {
        //                //            //The phone differs between SMS and something else.
        //                //        }
        //                //        break;
        //                //}

        //                //Process the following detail that comes in the notifications.
        //                AccessoryManager.ProcessTriggerDetails(nextTriggerDetails);
        //            nextTriggerDetails = AccessoryManager.GetNextTriggerDetails();
        //        }
        //        BackgroundAccessStatus backgroundAccessStatu = await BackgroundExecutionManager.RequestAccessAsync();
        //        if (backgroundAccessStatu == BackgroundAccessStatus.Denied)
        //        {
        //            Debug.WriteLine(String.Concat("Error, Access Denied for background execution ", backgroundAccessStatu));

        //            MessageDialog messageDialog = new MessageDialog("Access denied for background execution, try disabling some apps from background execution from Settings -> Battery Saver!");
        //            await messageDialog.ShowAsync();
        //        }
        //        else
        //        {
        //            //foreach (IBackgroundTaskRegistration value in BackgroundTaskRegistration.get_AllTasks)
        //            //{
        //            //    BackgroundTaskRegistration backgroundTaskRegistration = (BackgroundTaskRegistration)value;
        //            //    if (backgroundTaskRegistration.Name != "BackGroundTaskForPebble")
        //            //    {
        //            //        continue;
        //            //    }
        //            //    //WindowsRuntimeMarshal.AddEventHandler<BackgroundTaskCompletedEventHandler>(new Func<BackgroundTaskCompletedEventHandler, EventRegistrationToken>(backgroundTaskRegistration, BackgroundTaskRegistration.add_Completed), new Action<EventRegistrationToken>(backgroundTaskRegistration, BackgroundTaskRegistration.remove_Completed), new BackgroundTaskCompletedEventHandler(this.registration_Completed));

        //            //}
        //            Debug.WriteLine("Registering BackGround Task for notifications");
        //            BackgroundTaskBuilder backgroundTaskBuilder = new BackgroundTaskBuilder()
        //            {
        //                Name = "BackGroundTaskForPebble",
        //                TaskEntryPoint = "PebbleBackGroundTaskLibrary.BackGroundTaskTestClass"
        //            };

        //            DeviceManufacturerNotificationTrigger deviceManufacturerNotificationTrigger = new DeviceManufacturerNotificationTrigger(String.Concat("Microsoft.AccessoryManagement.Notification:", str), false);
        //            backgroundTaskBuilder.SetTrigger(deviceManufacturerNotificationTrigger);
        //            BackgroundTaskRegistration backgroundTaskRegistration1 = backgroundTaskBuilder.Register();
        //            //WindowsRuntimeMarshal.AddEventHandler<BackgroundTaskCompletedEventHandler>(new Func<BackgroundTaskCompletedEventHandler, EventRegistrationToken>(backgroundTaskRegistration1, BackgroundTaskRegistration.add_Completed), new Action<EventRegistrationToken>(backgroundTaskRegistration1, BackgroundTaskRegistration.remove_Completed), new BackgroundTaskCompletedEventHandler(this.registration_Completed));

        //            Object[] taskId = new Object[] { "Registered background task ", backgroundTaskRegistration1.TaskId, " ,", backgroundTaskRegistration1.Name, "." };
        //            Debug.WriteLine(String.Concat(taskId));

        //            Object[] objArray = new Object[] { "Background task registered:", backgroundTaskRegistration1.TaskId, " ,", backgroundTaskRegistration1.Name, "From:", "" };
        //            Debug.WriteLine(String.Concat(objArray));
        //            AccessoryManager.EnableAccessoryNotificationTypes(2047); //Cualquier notificacion entrante.

        //        }
        //        #endregion
        //    }catch(Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //    }
        //    //Finally we tell that the background task can terminate.
        //    deferral.Complete();
        //}
    }
}
