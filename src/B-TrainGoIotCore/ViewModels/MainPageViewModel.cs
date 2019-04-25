using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
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

        private byte _speedRaw;

        private byte _accelCounter;

        private byte _brakeCounter;

        private byte _inertiaCounter;

        private string _initialMessage;

        private EDirection _direction;

        private EControlState _controlState;

        private EControlState _nextState;

        private EControllerButtons _buttons = EControllerButtons.None;

        private int _trainPosition;

        private bool _stoppingDemo;

        private VoiceCommand _voiceCommand;

        private bool _runningSpeech;

        private bool _changingSpeechState;

        private string _speechMessage;

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

        public byte Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        public EDirection Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        public int TrainPosition
        {
            get => _trainPosition;
            set => SetProperty(ref _trainPosition, value);
        }

        public bool StoppingDemo
        {
            get => _stoppingDemo;
            set => SetProperty(ref _stoppingDemo, value);
        }

        public bool ChangingSpeechState
        {
            get => _changingSpeechState;
            set => SetProperty(ref _changingSpeechState, value);
        }

        public string SpeechMessage
        {
            get => _speechMessage;
            set => SetProperty(ref _speechMessage, value);
        }

        #endregion

        #region constructor

        #endregion

        #region method

        public async Task<bool> Initialize()
        {
            SetState(EControlState.Initialize);

            // initialize controller
            try
            {
                SetInitMessage("コントローラ初期化中");

                _masterController = new MasterController();

                await _masterController.Initialize();
            }
            catch (Exception e)
            {
                SetInitMessage("コントローラの初期化に失敗しました。");
                return false;
            }

            try
            {
                SetInitMessage("音声認識初期化中");
                _voiceCommand = new VoiceCommand();
                _voiceCommand.VoiceCommandReceived += OnReceivedSpeechCommand;

                var result = await _voiceCommand.Initialize();

                if (!result)
                {
                    SetInitMessage("音声認識の初期化に失敗しました");
                    return false;
                }
            }
            catch (Exception e)
            {
                SetInitMessage("音声認識の初期化に失敗しました");
                return false;
            }

            _bTrain = new BTrain();

            _bTrain.ConnectionChagned += OnChangedConnectionBTrain;
            _bTrain.PositionUpdated += OnPositionUpdatedBTrain;

            return ConnectBase();
        }

        private void OnPositionUpdatedBTrain(object sender, int e)
        {
            UpdateTrainPosition(e);

            if (ControlState == EControlState.AutoDemo) SetAutoDemoSpeed();
            else if (ControlState == EControlState.SpeechCommand) SetSpeechDemoSpeed();
        }

        private void OnChangedConnectionBTrain(object sender, bool isConnected)
        {
            if (isConnected)
            {
                SetState(EControlState.Manual);

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

                SetState(EControlState.Initialize);
                ConnectBase();
            }
        }

        private async void OnTimerTickReadController(ThreadPoolTimer t)
        {
            _masterController.ReadState();

            var buttons = _masterController.ButtonState;

            if (_masterController.IsButtonDown(EControllerButtons.Select))
            {
                if (_masterController.IsButtonReleased(EControllerButtons.A))
                {
                    // マニュアルモードへ
                    if (ControlState == EControlState.AutoDemo && !_stoppingDemo)
                    {
                        _nextState = EControlState.Manual;
                        StopDemo();
                    }
                    else if (ControlState == EControlState.SpeechCommand)
                    {
                        _nextState = EControlState.Manual;
                        LeaveSpeechDemo();
                    }
                }
                else if (_masterController.IsButtonReleased(EControllerButtons.B))
                {
                    // オートデモモードへ
                    if (ControlState == EControlState.Manual && Speed == 0)
                    {
                        StartDemo();
                    }
                    else if (ControlState == EControlState.SpeechCommand)
                    {
                        _nextState = EControlState.AutoDemo;
                        LeaveSpeechDemo();
                    }
                }
                else if (_masterController.IsButtonReleased(EControllerButtons.C))
                {
                    // 音声コマンドモードへ
                    if (ControlState == EControlState.Manual && Speed == 0)
                    {
                        StartSpeechCommand();
                    }
                    else if (ControlState == EControlState.AutoDemo && !_stoppingDemo)
                    {
                        _nextState = EControlState.SpeechCommand;
                        StopDemo();
                    }
                }
            }

            //if (buttons.HasFlag(EControllerButtons.Select) && !buttons.HasFlag(EControllerButtons.Start) &&
            //    _buttons.HasFlag(EControllerButtons.Start))
            //{
            //    if (ControlState == EControlState.AutoDemo && !_stoppingDemo)
            //    {
            //        StopDemo();
            //    }
            //    else if (ControlState == EControlState.Manual && Speed == 0)
            //    {
            //        SetState(EControlState.AutoDemo);
            //        StartDemo();
            //    }


            //    return;
            //}

            if (ControlState == EControlState.Manual) ControlManual();

            _buttons = buttons;
        }

        private bool ConnectBase()
        {
            SetInitMessage("Bトレインに接続中");

            try
            {
                _bTrain.Connect();
            }
            catch (Exception e)
            {
                SetInitMessage(e.Message);
                return false;
            }

            return true;
        }

        private async void LeaveSpeechDemo()
        {
            if (ControlState != EControlState.SpeechCommand) return;

            if (Speed > 0)
            {
                _changingSpeechState = true;

                while (_changingSpeechState) await Task.Delay(100);

                if (_nextState == EControlState.Initialize || _nextState == EControlState.None)
                    _nextState = EControlState.Manual;

                if (_nextState == EControlState.AutoDemo) StartDemo();
                else SetState(_nextState);
            }
            else
            {
                if (_nextState == EControlState.AutoDemo) StartDemo();
                else SetState(_nextState);
            }
        }

        private void StartSpeechCommand()
        {
            SetSpeechMessage("「出発進行」の掛け声で発車します。");
            SetState(EControlState.SpeechCommand);
            _voiceCommand.Resume();
        }

        private void SetSpeechDemoSpeed()
        {
            switch (TrainPosition)
            {
                // 奥中央
                case 1:
                    SetSpeed(Direction == EDirection.Left ? (byte)200 : (byte)170);
                    break;
                // 奥左
                case 2:
                    SetSpeed(Direction == EDirection.Left ? (byte)200 : (byte)170);
                    break;
                // 左
                case 3:
                    SetSpeed(Direction == EDirection.Left ? (byte)170 : (byte)140);
                    break;
                // 手前左
                case 4:
                    SetSpeed(Direction == EDirection.Left ? (byte)140 : (byte)120);
                    break;
                // 手前中央
                case 5:
                    SetSpeed(0);
                    _runningSpeech = false;
                    if (_changingSpeechState)
                    {
                        _changingSpeechState = false;
                    }
                    else
                    {
                        SetSpeechMessage("「出発進行」の掛け声で発車します。");
                        _voiceCommand.Resume();
                    }
                    break;
                // 手前右
                case 6:
                    SetSpeed(Direction == EDirection.Left ? (byte)120 : (byte)140);
                    break;
                // 右
                case 7:
                    SetSpeed(Direction == EDirection.Left ? (byte)140 : (byte)170);
                    break;
                // 奥右
                case 8:
                    SetSpeed(Direction == EDirection.Left ? (byte)170 : (byte)200);
                    break;
            }
        }

        private async void SetAutoDemoSpeed(bool isDemoStart = false)
        {
            switch (TrainPosition)
            {
                // 奥中央
                case 1:
                    SetSpeed(Direction == EDirection.Left ? (byte)200 : (byte)170);
                    break;
                // 奥左
                case 2:
                    SetSpeed(Direction == EDirection.Left ? (byte)200 : (byte)170);
                    break;
                // 左
                case 3:
                    SetSpeed(Direction == EDirection.Left ? (byte)170 : (byte)140);
                    break;
                // 手前左
                case 4:
                    SetSpeed(Direction == EDirection.Left ? (byte)140 : (byte)120);
                    break;
                // 手前中央
                case 5:
                    if (!isDemoStart)
                    {
                        SetSpeed(0);
                        for (var i = 0; i < 100; i++)
                        {
                            if (_stoppingDemo)
                            {
                                SetStoppingDemo(false);
                                return;
                            }

                            await Task.Delay(50);
                        }
                    }

                    SetSpeed(120);
                    break;
                // 手前右
                case 6:
                    SetSpeed(Direction == EDirection.Left ? (byte)120 : (byte)140);
                    break;
                // 右
                case 7:
                    SetSpeed(Direction == EDirection.Left ? (byte)140 : (byte)170);
                    break;
                // 奥右
                case 8:
                    SetSpeed(Direction == EDirection.Left ? (byte)170 : (byte)200);
                    break;
            }
        }

        private void StartDemo()
        {
            SetState(EControlState.AutoDemo);
            SetAutoDemoSpeed(true);
        }

        private async void StopDemo()
        {
            if (ControlState != EControlState.AutoDemo) return;

            SetStoppingDemo(true);

            while (_stoppingDemo) await Task.Delay(100);

            if (_nextState == EControlState.Initialize || _nextState == EControlState.None)
                _nextState = EControlState.Manual;

            if (_nextState == EControlState.SpeechCommand) StartSpeechCommand();
            else SetState(_nextState);
        }

        private void ControlManual()
        {
            UpdateEmergency();

            var buttons = _masterController.ButtonState;

            var speed = _speedRaw;

            var accel = 0;


            if (_masterController.IsEmergency)
            {
                // 非常ブレーキ作動
                speed = 0;
            }
            else if (_masterController.AccelValue > 0)
            {
                // 非常ブレーキ解除済み
                if (speed == 0)
                {
                    speed = 110;
                }
                else
                {
                    _accelCounter++;

                    if ((_masterController.AccelValue == 1 && _accelCounter >= 10) ||
                        (_masterController.AccelValue == 2 && _accelCounter >= 7) ||
                        (_masterController.AccelValue == 3 && _accelCounter >= 5) ||
                        (_masterController.AccelValue == 4 && _accelCounter >= 3) ||
                        (_masterController.AccelValue == 5 && _accelCounter >= 2))
                    {
                        _accelCounter = 0;
                        if (speed < 255) speed++;
                    }
                }
            }
            else if (_masterController.BrakeValue > 0)
            {
                _brakeCounter++;

                if ((_masterController.BrakeValue == 1 && _brakeCounter >= 12) ||
                    (_masterController.BrakeValue == 2 && _brakeCounter >= 9) ||
                    (_masterController.BrakeValue == 3 && _brakeCounter >= 7) ||
                    (_masterController.BrakeValue == 4 && _brakeCounter >= 5) ||
                    (_masterController.BrakeValue == 5 && _brakeCounter >= 4) ||
                    (_masterController.BrakeValue == 6 && _brakeCounter >= 3) ||
                    (_masterController.BrakeValue == 7 && _brakeCounter >= 2) ||
                    (_masterController.BrakeValue == 8 && _brakeCounter >= 1))
                {
                    _brakeCounter = 0;
                    if (speed > 0) speed--;
                }

                if (speed < 100)
                {
                    speed = 0;
                }
            }
            else
            {
                if (Speed == 0 && _masterController.IsButtonReleased(EControllerButtons.Start))
                {
                    InvertDirection();
                }

                _accelCounter = 0;
                _brakeCounter = 0;
                _inertiaCounter++;

                if (_inertiaCounter > 10)
                {
                    _inertiaCounter = 0;
                    if (speed > 0) speed--;

                    if (speed < 100) speed = 0;
                }
            }

            SetSpeed(speed);
        }

        private async void OnReceivedSpeechCommand(object sender, VoiceCommandEventArgs e)
        {
            if (_runningSpeech) return;

            _runningSpeech = true;

            await _voiceCommand.Pause();

            SetSpeechMessage("自動運行中");

            switch (TrainPosition)
            {
                // 奥中央
                case 1:
                    SetSpeed(Direction == EDirection.Left ? (byte)200 : (byte)170);
                    break;
                // 奥左
                case 2:
                    SetSpeed(Direction == EDirection.Left ? (byte)200 : (byte)170);
                    break;
                // 左
                case 3:
                    SetSpeed(Direction == EDirection.Left ? (byte)170 : (byte)140);
                    break;
                // 手前左
                case 4:
                    SetSpeed(Direction == EDirection.Left ? (byte)140 : (byte)120);
                    break;
                // 手前中央
                case 5:
                    SetSpeed(120);
                    break;
                // 手前右
                case 6:
                    SetSpeed(Direction == EDirection.Left ? (byte)120 : (byte)140);
                    break;
                // 右
                case 7:
                    SetSpeed(Direction == EDirection.Left ? (byte)140 : (byte)170);
                    break;
                // 奥右
                case 8:
                    SetSpeed(Direction == EDirection.Left ? (byte)170 : (byte)200);
                    break;
            }
        }

        private async void InvertDirection()
        {
            var direciton = Direction == EDirection.Left ? EDirection.Right : EDirection.Left;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Direction = direciton);
        }

        private async void SetSpeed(byte speed)
        {
            if (speed == _speedRaw) return;
            _speedRaw = speed;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _bTrain.SetSpeed(_direction, speed);
                Speed = (byte) (speed > 100 ? (speed - 100) : 0);                
            });
        }

        private async void SetStoppingDemo(bool value)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StoppingDemo = value);
        }

        private async void SetState(EControlState state)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ControlState = state);
        }

        private async void SetInitMessage(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => InitialMessage = message);
        }

        private async void UpdateTrainPosition(int position)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => TrainPosition = position);
        }

        private async void UpdateEmergency()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                EmergencyBrush = _masterController.IsEmergency ? EmergencyEnableBrush : EmergencyDisableBrush;
            });
        }

        private async void SetSpeechMessage(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SpeechMessage = message);
        }

        #endregion
    }
}
