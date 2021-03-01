using Common.GDA;
using GUI.DataModel;
using GUI.Helpers;
using Microsoft.Win32;
using NServiceBus;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {

        static ChannelFactory<INetworkModelGDAContract> factory;
        static INetworkModelGDAContract proxy;
        private static IEndpointInstance endpointInstance;
        private DrawingModel drawingModel;


        public MainWindow()
        {
            InitializeComponent();
            

            ZoomViewbox.Width = this.Width;
            ZoomViewbox.Height = this.Height;

            AsyncEndpointCreate().GetAwaiter().GetResult();
            ConnectToNMS("net.tcp://localhost:11123/NMS/GDA/");

            drawingModel = new DrawingModel(proxy);
        }

        static async Task AsyncEndpointCreate()
        {
            var endpointConfiguration = new EndpointConfiguration("GUI");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            endpointConfiguration.PurgeOnStartup(true);

            endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            ResetMeasurements();
            endpointInstance.Stop().ConfigureAwait(false);
            Disconnect();
        }

        private void ResetMeasurements()
        {
            using (JSONParser jP = new JSONParser())
            {
                jP.Reset();
            }
        }

        #region Menu

        private void ImportSchema(object sender, RoutedEventArgs e)
        {
            /*OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*txt, *.xml, *.json) | *.txt; *.xml; *.json;"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                // import logic for schema
            }*/
            drawingModel.ImportModel();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Pan

        private Point _last;
        private bool isDragged = false;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            _last = e.GetPosition(this);

            isDragged = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            isDragged = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragged == false)
                return;

            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                var pos = e.GetPosition(this);
                var matrix = mt.Matrix; // it's a struct
                matrix.Translate((pos.X - _last.X)*5, (pos.Y - _last.Y)*5);
                mt.Matrix = matrix;
                _last = pos;
            }
        }

        #endregion

        #region Zoom

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            UpdateViewBox((e.Delta > 0) ? 50 : -50);
        }

        private void UpdateViewBox(int newValue)
        {
            if ((ZoomViewbox.Width > this.Width) && ZoomViewbox.Height > this.Height)
            {
                ZoomViewbox.Width += newValue * (this.Width / this.Height);
                ZoomViewbox.Height += newValue;
            }
            else if(newValue > 0)
            {
                ZoomViewbox.Width += newValue * (this.Width / this.Height);
                ZoomViewbox.Height += newValue;
            }
        }

        #endregion

        #region connection

        private bool ConnectToNMS(string uri)
        {
            //Disconnect();

            try
            {
                factory = new ChannelFactory<INetworkModelGDAContract>(new NetTcpBinding() { TransferMode = TransferMode.Streamed, MaxReceivedMessageSize = 2147483647 }, new EndpointAddress(new Uri(uri)));
                proxy = factory.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                Disconnect();
                return false;
            }

            MessageBox.Show("Connected to NMS");

            return true;
        }

        private void Disconnect()
        {
            try
            {
                factory.Close();
            }
            catch { }

            proxy = null;
            factory = null;
        }

        #endregion

    }
}
