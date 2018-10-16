using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GeneralUtility.Joystick;

using System.Windows;

namespace GeneralUtility
{
    public class MainViewModel : ViewModelBase
    {
        private JoyStick joy = JoyStick.Instance;

        public string Message { get { return joy.mMessage; } }
        public List<string> FunctionNames { get { return joy.mFuctionName; } }
        public string ChannelNameX { get { return joy.GetCurChannelXName(); } }
        public string ChannelNameY { get { return joy.GetCurChannelYName(); } }
        public double ValueX { get { return joy.GetChannelValue(ChannelNameX); } }
        public double ValueY { get { return joy.GetChannelValue(ChannelNameY); } }

        public MainViewModel()
        {
            Testused();

            RadioButtonCommand = new RelayCommand<object>(RadioButtonExecute);
            FunctionCommand = new RelayCommand<object>(FunctionExecute);
            ChangeChannelCommand = new RelayCommand<object>(ChangeChannelExecute);
            JoyStickTriggerCommand = new RelayCommand<object>(JoystickTriggerExecute, IsTriggerExecutable);
            ConfirmCommand = new RelayCommand<object>(ConfirmExecutte, IsTriggerExecutable);
            CancelCommand = new RelayCommand<object>(CancelExecute, IsTriggerExecutable);
            joy.FailureOccured = (sender, e) => { ControllerFailed(this, EventArgs.Empty); };
        }

        public EventHandler ControllerFailed;

        #region Public Commands

        public ICommand RadioButtonCommand { get; private set; }
        private void RadioButtonExecute(object parameter)
        {
            joy.SetCurSpeedMode((SpeedEnum)parameter);
        }

        public ICommand FunctionCommand { get; private set; }
        private void FunctionExecute(object parameter)
        {
            joy.CallBackFunction(parameter.ToString());
        }

        public ICommand ChangeChannelCommand { get; private set; }
        private void ChangeChannelExecute(object parameter)
        {
            if (bool.Parse((string)parameter))
                joy.PreviousChannelSet();
            else
                joy.NextChannelSet();
        }

        private bool isComplete = true;
        public ICommand JoyStickTriggerCommand { get; private set; }
        private async void JoystickTriggerExecute(object parameter)
        {
            isComplete = false;
            var dir = (DirectionEnum)parameter;
            await joy.TriggerCommandAsync(dir);
            isComplete = true;
            RaisePropertyChanged(nameof(ValueX));
            RaisePropertyChanged(nameof(ValueY));
        }
        private bool IsTriggerExecutable(object parameter) { return isComplete; }

        public ICommand CancelCommand { get; private set; }

        public ICommand ConfirmCommand { get; private set; }
        private void ConfirmExecutte(object parameter)
        {
            var window = (Window)parameter;
            window.DialogResult = true;
            window?.Close();
        }
        private void CancelExecute(object parameter)
        {
            joy.RestoreAllChannelsValue();
            var window = (Window)parameter;
            window.DialogResult = false;
            window?.Close();
        }

        #endregion


        private void Testused()
        {
            joy.SetMessage("SetMessage");
            joy.SetFunction("Function1", "Function2", "Function3");
            MotorSimulation motor = new MotorSimulation()
            {
                DeviceName = "compMotor"
            };
            int benchmark = 1111;
            motor.Drive(benchmark);
            joy.SetXChannel(motor.DeviceName, benchmark, motor);
            joy.SetJoySpeedX(int.MaxValue / 20, int.MaxValue / 30, 100);
        }
    }
}
