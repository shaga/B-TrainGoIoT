using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using B_TrainGoIotCore.Models;
using Prism.Mvvm;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

namespace B_TrainGoIotCore.ViewModels
{
    class MainPageViewModel : ViewModelBase
    {
        #region const

        private static readonly SolidColorBrush EmergencyDisableBrush = new SolidColorBrush(Colors.LightGray);
        private static readonly SolidColorBrush EmergencyEnableBrush = new SolidColorBrush(Colors.Red);

        #endregion

        #region field

        private Brush _emergencyBrush = EmergencyDisableBrush;

        private MasterController _masterController;

        private BTrain _bTrain;

        private ThreadPoolTimer _timer;

        private byte _speed;

        private byte _accel;

        private byte _brake;

        private string _initialMessage;

        private EControlState _controlState;

        #endregion

        #region property

        public Brush EmergencyBrush
        {
            get => _emergencyBrush;
            set => SetProperty(ref _emergencyBrush, value);
        }

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;

        public string InitialMessage
        {
            get => _initialMessage;
            set => SetProperty(ref _initialMessage, value);
        }

        public EControlState ControlState
        {
            get => _controlState;
            set => SetProperty(ref _controlState, value);
        }

        #endregion

        #region constructor

        #endregion

        #region method

        public async Task<bool> Initialize()
        {
            // initialize controller
            try
            {
                _masterController = new MasterController();

                await _masterController.Initialize();
            }
            catch (Exception e)
            {
                return false;
            }

            try
            {
                _bTrain = new BTrain();

                _bTrain.ConnectionChagned += OnChangedConnectionBTrain;
                _bTrain.PositionUpdated += OnPositionUpdatedBTrain;

                _bTrain.Connect();
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
        }

        private void OnPositionUpdatedBTrain(object sender, int e)
        {
            
        }

        private void OnChangedConnectionBTrain(object sender, bool isConnected)
        {
            if (isConnected)
            {
                // connected to B-Train
                _timer?.Cancel();
                _timer = null;

                _timer = ThreadPoolTimer.CreatePeriodicTimer(OnTimerTickReadController, TimeSpan.FromMilliseconds(50));
            }
            else
            {
                // disconnected from B-Train
                _timer?.Cancel();

                _timer = null;
            }
        }

        private void OnTimerTickReadController(ThreadPoolTimer t)
        {

        }

        #endregion
    }
}
