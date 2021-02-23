using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private static IEndpointInstance endpointInstance;
        public MainWindow()
        {
            InitializeComponent();
            AsyncEndpointCreate().GetAwaiter().GetResult();
        }

        static async Task AsyncEndpointCreate()
        {
            var endpointConfiguration = new EndpointConfiguration("GUI");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}
