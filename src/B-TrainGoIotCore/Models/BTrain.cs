using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Core;

namespace B_TrainGoIotCore.Models
{
    enum EDirection
    {
        Right,
        Left,
        Stop,
    }

    class BTrain : IDisposable
    {
        #region const

        #region Device Name

        private const string LocalShortenedName = "BTrain";
        private const string LocalCompleteName = "B-Train Controller";

        #endregion

        #region GATT UUID

        private static readonly Guid ControllerServiceGuid = Guid.Parse("237D1D00-61AF-4B22-AE78-399533CBA7C8");
        private static readonly Guid ControllerCharacteristicGuid = Guid.Parse("237D1D01-61AF-4B22-AE78-399533CBA7C8");
        private static readonly Guid PositionCharacteristicGuid = Guid.Parse("237D1D02-61AF-4B22-AE78-399533CBA7C8");

        #endregion

        #region Characteristic value

        #region train position

        public const int PosUnknown = 0;
        public const int PosFront = 5;
        public const int PosFrontLeft = 4;
        public const int PosLeft = 3;
        public const int PosBackLeft = 2;
        public const int PosBack = 1;
        public const int PosBackRight = 8;
        public const int PosRight = 7;
        public const int PosFrontRight = 6;

        #endregion

        #region train Control

        private const int IdxControlDirection = 0;
        private const int IdxControlSpeed = 1;

        public const int SpeedStop = 0x00;
        public const int SpeedMin = 0x60;
        public const int SpeedMax = 0xff;

        #endregion

        #endregion

        #endregion

        #region field

        private readonly BluetoothLEAdvertisementWatcher _watcher;
        private BluetoothLEDevice _bleDevice;
        private GattDeviceService _controllerService;
        private GattCharacteristic _controllerCharacteristic;
        private GattCharacteristic _positionCharacteristic;

        #endregion

        #region property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;

        public bool IsConnected => (_bleDevice?.ConnectionStatus ?? BluetoothConnectionStatus.Disconnected) ==
                                   BluetoothConnectionStatus.Connected;

        #endregion

        #region event

        public event EventHandler<int> PositionUpdated;

        public event EventHandler<bool> ConnectionChagned;

        #endregion

        #region constructor

        public BTrain()
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.AdvertisementFilter.Advertisement.LocalName = LocalCompleteName;
            //_watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(ControllerServiceGuid);
            _watcher.Received += OnReceivedAdvertisement;
        }

        #endregion

        #region method

        public void Dispose()
        {
            _positionCharacteristic = null;
            _controllerService = null;
            _controllerService?.Dispose();
            _controllerService = null;

            if (_bleDevice != null)
            {
                _bleDevice.ConnectionStatusChanged -= OnConnectionChanged;
                _bleDevice.Dispose();
                _bleDevice = null;
            }
        }

        public void Connect()
        {
            _watcher.Start();
        }

        public async void SetSpeed(EDirection direction, int speed = SpeedStop)
        {
            if (!IsConnected || _controllerCharacteristic == null)
                return;

            if (direction == EDirection.Stop) speed = SpeedStop;
            else if (speed <= 0 || speed < SpeedMin)
            {
                speed = SpeedStop;
                direction = EDirection.Stop;
            }
            else if (speed > SpeedMax)
            {
                speed = SpeedMax;
            }

            var data = new[] { (byte)direction, (byte)speed };

            await _controllerCharacteristic.WriteValueAsync(data.AsBuffer(), GattWriteOption.WriteWithoutResponse);
        }

        public async Task<int> GetPosition()
        {
            if (IsConnected || _positionCharacteristic == null)
                return 0;

            var data = await _positionCharacteristic.ReadValueAsync();

            return data.Value.ToArray()[0];
        }

        private void OnReceivedAdvertisement(BluetoothLEAdvertisementWatcher watcher,
            BluetoothLEAdvertisementReceivedEventArgs e)
        {
            _watcher.Stop();

            ConnectDevice(e.BluetoothAddress);
        }

        private void OnConnectionChanged(BluetoothLEDevice device, object args)
        {
            var isConnected = (device?.ConnectionStatus ?? BluetoothConnectionStatus.Disconnected) ==
                              BluetoothConnectionStatus.Connected;

            ConnectionChagned?.Invoke(this, isConnected);
        }

        private void OnUpdatedPosition(GattCharacteristic characteristic, GattValueChangedEventArgs e)
        {
            var data = e.CharacteristicValue.ToArray();

            if (data == null || data.Length < 1) return;

            var position = data[0];

            PositionUpdated?.Invoke(this, position);
        }

        private async void ConnectDevice(ulong address)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                _bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);

                _bleDevice.ConnectionStatusChanged += OnConnectionChanged;

                var servicesResult = await _bleDevice.GetGattServicesAsync();

                if (servicesResult.Status != GattCommunicationStatus.Success)
                {
                    _bleDevice.Dispose();
                    _bleDevice = null;

                    _watcher.Start();
                    return;
                }

                _controllerService = servicesResult.Services.FirstOrDefault(s => s.Uuid.Equals(ControllerServiceGuid));

                if (_controllerService == null)
                {
                    _bleDevice.Dispose();
                    _bleDevice = null;

                    _watcher.Start();
                    return;
                }

                var characteristicsResult = await _controllerService.GetCharacteristicsAsync();

                if (characteristicsResult.Status != GattCommunicationStatus.Success)
                {
                    _bleDevice.Dispose();
                    _bleDevice = null;

                    _controllerService.Dispose();
                    _controllerService = null;

                    _watcher.Start();
                    return;
                }

                _controllerCharacteristic =
                    characteristicsResult.Characteristics.FirstOrDefault(
                        c => c.Uuid.Equals(ControllerCharacteristicGuid));
                _positionCharacteristic =
                    characteristicsResult.Characteristics.FirstOrDefault(
                        c => c.Uuid.Equals(PositionCharacteristicGuid));
                _positionCharacteristic.ValueChanged += OnUpdatedPosition;
            });
        }

        #endregion
    }
}
