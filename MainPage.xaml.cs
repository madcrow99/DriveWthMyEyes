using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System;
using Windows.Devices.Enumeration;
using System.Collections.ObjectModel;
using System.Threading;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace DriveWthMyEyes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isPreviewing;
        DisplayRequest displayRequest = new DisplayRequest();
        private ObservableCollection<DeviceInformation> listOfCameras;
        DeviceInformationCollection cameras;
        //serial
        DeviceInformation entry = null;
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        private String LFF_CMD = "0";//fwd speed, bwd speed, left speed, right speed 
        private String FF_CMD = "1";
        private String RFF_CMD = "2";
        private String LF_CMD = "3";
        private String F_CMD = "4";
        private String RF_CMD = "5";
        private String L_CMD = "6";
        private String R_CMD = "7";
        private String LB_CMD = "8";
        private String B_CMD = "9";
        private String RB_CMD = "a";
        private String LBB_CMD = "b";
        private String BB_CMD = "c";
        private String RBB_CMD = "d";
        private String MODE = "e";
        private String phone = "f";
        private String computer = "g";
        private String roblox = "h";
        private String minecraft = "i";

        private Boolean phone_active = false;
        private Boolean computer_active = false;
        private Boolean camera_active = false;
        private String computer_ctrl_type = "Computer";


        public MainPage()
        {
            InitializeComponent();
            Application.Current.Suspending += Application_Suspending;
            listOfCameras = new ObservableCollection<DeviceInformation>();
            ListAvailableCams();
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();

        }

        //start of camera section
        private async void Camera_Click(object sender, RoutedEventArgs e)
        {
            if (!camera_active)
            {
                await StartPreviewAsync();
                camera_active = true;
            }
            else if (camera_active)
            {
                await CleanupCameraAsync();
                camera_active = false;
            }
        }
            
        private async Task StartPreviewAsync()
        {
                try
                {

                    mediaCapture = new MediaCapture();
               // var selectedDevice = listDevices.SelectedItems;
                
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[0].Id };
                //await mediaCapture.InitializeAsync();
                await mediaCapture.InitializeAsync(settings);

                displayRequest.RequestActive();
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                }
                catch (UnauthorizedAccessException)
                {
                    // This will be thrown if the user denied access to the camera in privacy settings
                    ShowMessageToUser("The app was denied access to the camera");
                    return;
                }

                try
                {
                    PreviewControl.Source = mediaCapture;
                    await mediaCapture.StartPreviewAsync();
                    isPreviewing = true;
                }
                catch (System.IO.FileLoadException)
                {
                    mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
                }
            }
        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }
        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }

        }
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }
        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private async void ListAvailableCams()
        {
            try
            {
                cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                
                for (int i = 0; i < cameras.Count; i++)
                {
                    listOfCameras.Add(cameras[i]);
                }
               // listDevices.ItemsSource = listOfCameras;
                //listDevices.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowMessageToUser(ex.Message);
            }
        }


        private void ShowMessageToUser(string v)
        {
            throw new NotImplementedException();
        }

        //end of camera section 

        //START SERIAL COMMUNICATIO SECTION 

        public Brush AliceBlue { get; private set; }

        private async void OnLegacyInvoked(object sender, RoutedEventArgs e)
        {


            var buttonClicked = sender as Button;
            switch (buttonClicked.Name)
            {
                case "btnSerialConnect":
                    SerialPortConfiguration();

                    break;
                case "btnSerialDisconnect":
                    SerialPortDisconnect();
                    break;
                case "phone_button":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(phone);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "computer_button":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        switch(computer_ctrl_type){
                            case "Roblox":
                                await send_command(roblox);
                                break;
                            case "Minecraft":
                                await send_command(minecraft);
                                break;
                            case "Computer":
                                await send_command(computer);
                                break;
                        }
                       // await send_command(computer);

                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LFF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LFF_CMD);                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "FF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(FF_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RFF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RFF_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LF_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "F":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(F_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RF_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "L":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(L_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "R":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(R_CMD);

                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LB_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "B":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(B_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RB_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LBB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LBB_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "BB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(BB_CMD);

                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RBB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RBB_CMD);
                        
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "mode_button":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(MODE);

                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
            }
        }

            private async void OnGazeInvoked(object sender, DwellInvokedRoutedEventArgs e)
        {

            var buttonClicked = sender as Button;
            switch (buttonClicked.Name)
            {
                case "btnSerialConnect":
                    SerialPortConfiguration();
                    
                    break;
                case "btnSerialDisconnect":
                    SerialPortDisconnect();
                    break;
                case "phone_button":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(phone);
                        e.Handled = true;
                        phone_active = !phone_active;
                        computer_active = false;
                        controlLayout();
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "computer_button":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        switch (computer_ctrl_type)
                        {
                            case "Roblox":
                                await send_command(roblox);
                                break;
                            case "Minecraft":
                                await send_command(minecraft);
                                break;
                            case "Computer":
                                await send_command(computer);
                                break;
                        }
                        // await send_command(computer);

                        e.Handled = true;
                        computer_active = !computer_active;
                        phone_active = false;
                        controlLayout();
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LFF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LFF_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "FF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(FF_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RFF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RFF_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LF_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "F":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(F_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RF":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RF_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "L":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(L_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "R":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(R_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LB_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "B":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(B_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RB_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "LBB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(LBB_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "BB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(BB_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "RBB":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(RBB_CMD);
                        e.Handled = true;
                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;
                case "mode_button":
                    if (serialPort != null)
                    {
                        dataWriteObject = new DataWriter(serialPort.OutputStream);
                        await send_command(MODE);
                        e.Handled = true;

                    }
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;
                    }
                    break;

            }

        }
        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }
                lstSerialDevices.ItemsSource = listOfDevices;
                phone_button.IsEnabled = true;
                computer_button.IsEnabled = true;
                lstSerialDevices.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                //msg_text_block.Text = ex.Message;
            }
        }
        private async void SerialPortConfiguration()
        {

            if (entry == null)
            {
                var selection = lstSerialDevices.SelectedItems;
                if (selection.Count <= 0)
                {
                   // msg_text_block.Text = "select device";
                    return;
                }
                 entry = (DeviceInformation)selection[0];
            }
            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                //serialPort = await SerialDevice.GetDeviceSelectorFromUsbVidPid(0x10C4, 0xEA60);
                //msg_text_block.Text = entry.Id;
                
                if (serialPort != null)
                {
                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.BaudRate = 9600;
                    serialPort.Parity = SerialParity.None;
                    serialPort.StopBits = SerialStopBitCount.One;
                    serialPort.DataBits = 8;
                    serialPort.Handshake = SerialHandshake.None;
                    //msg_text_block.Text = "Serial port opened";
                    ReadCancellationTokenSource = new CancellationTokenSource();
                    Listen();
                }
            }
            catch (Exception ex)
            {
                //msg_text_block.Text = ex.Message;
                phone_button.IsEnabled = false;
                computer_button.IsEnabled = false;
            }
        }
        private void SerialPortDisconnect()
        {
            try
            {
                CancelReadTask();
                CloseDevice();
                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                //msg_text_block.Text = ex.Message;
            }
        }

        private async Task send_command(string value)
        {
            var command = value;
            Task<UInt32> storeAsyncTask;
            if (command.Length != 0)
            {
                dataWriteObject.WriteString(command);
                //dataWriteObject.WriteBytes()
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
                Thread.Sleep(10);
                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                   // msg_text_block.Text = "command sent";
                }
            }
            else
            {
               // msg_text_block.Text = "nothing to send";
            }
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    while (true)
                    {
                        await ReadData(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
               // msg_text_block.Text = ex.Message;
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    CloseDevice();
                }
                else
                {
                    //msg_text_block.Text = "Task annullato";
                    CloseDevice();
                    SerialPortConfiguration();
                }
            }
            finally
            {
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        private async Task ReadData(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 1024;
            cancellationToken.ThrowIfCancellationRequested();
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
               // msg_text_block.Text = dataReaderObject.ReadString(bytesRead);
            }
        }

        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
            phone_button.IsEnabled = false;
            computer_button.IsEnabled = false;
           // listOfDevices.Clear();
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            computer_ctrl_type = e.AddedItems[0].ToString();
            
        }
        private void controlLayout()
        {
            LFF.Content = "LFF";
            FF.Content = "FF";
            RFF.Content = "RFF";
            LF.Content = "LF";
            F.Content = "F";
            RF.Content = "RF";
            L.Content = "L";
            R.Content = "R";
            LB.Content = "LB";
            B.Content = "B";
            RB.Content = "RB";
            LBB.Content = "LBB";
            BB.Content = "BB";
            RBB.Content = "RBB";
            mode_button.Content = "Mode";
            phone_button.FontStyle = Windows.UI.Text.FontStyle.Normal;
            phone_button.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            computer_button.FontStyle = Windows.UI.Text.FontStyle.Normal;
            computer_button.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);

            if (phone_active)
            {
                LFF.Content = "Scroll Down";
                RFF.Content = "Scroll Up";
                LBB.Content = "Left Click";
                RBB.Content = "Right Click";
                phone_button.FontStyle = Windows.UI.Text.FontStyle.Italic;
                phone_button.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
                computer_button.FontStyle = Windows.UI.Text.FontStyle.Normal;
                computer_button.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
            else if (computer_active)
            {
                if (computer_ctrl_type == "Roblox")
                {
                    mode_button.Content = "Latch";
                    LFF.Content = "Left";
                    FF.Content = "FWD";
                    RFF.Content = "Right";
                    LF.Content = "LeftF";
                    F.Content = "SlowF";
                    RF.Content = "RightF";
                    L.Content = "Turn L";
                    R.Content = "Turn R";
                    LB.Content = "Left";
                    B.Content = "Jump";
                    RB.Content = "Right";
                    LBB.Content = "Scroll Down";
                    BB.Content = "Back";
                    RBB.Content = "Scroll Up";
                }
                else if (computer_ctrl_type == "Minecraft")
                {
                    mode_button.Content = "Latch";
                    LFF.Content = "Left";
                    FF.Content = "FWD";
                    RFF.Content = "Right";
                    LF.Content = "LeftF";
                    F.Content = "SlowF";
                    RF.Content = "RightF";
                    L.Content = "Turn L";
                    R.Content = "Turn R";
                    LB.Content = "Left";
                    B.Content = "Jump";
                    RB.Content = "Right";
                    LBB.Content = "Stack";
                    BB.Content = "Back";
                    RBB.Content = "Scroll Up";
                }
                else
                {
                    LFF.Content = "Scroll Down";
                    RFF.Content = "Scroll Up";
                    LBB.Content = "Left Click";
                    RBB.Content = "Right Click";
                }
                computer_button.FontStyle = Windows.UI.Text.FontStyle.Italic;
                computer_button.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                phone_button.FontStyle = Windows.UI.Text.FontStyle.Normal;
                phone_button.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
            
        }
    }
}