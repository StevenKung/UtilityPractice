using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralUtility.Joystick
{
    public enum SETEnum : int
    {
        ONE = 0,
        TWO = 1,
        THREE = 2,
    };
    public enum SpeedEnum
    {
        SLOW,
        NORMAL,
        FAST,
    }

    public enum DirectionEnum
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        LEFT_UP
    }
    public class JoyStick
    {
        JoyStick()
        {
            Reset();
        }
        public static JoyStick Instance { get; private set; } = new JoyStick();
        public string TitleName { get; private set; }
        protected const int MAX_NUM_OF_SETS = 3;

        double mCurSpd_X;
        double mCurSpd_Y;
        List<JoystickChannelSet> mChannelSets = new List<JoystickChannelSet>();
        List<JoyStickSpeedSet> mSpeedSets = new List<JoyStickSpeedSet>();
        SETEnum mCurrentChannelSet = SETEnum.ONE;
        DirectionEnum mCurDirection = DirectionEnum.DOWN;
        SpeedEnum mSpeedMode = SpeedEnum.NORMAL;
        string mCallback;
        public string mMessage { get; private set; }
        public List<string> mFuctionName { get; private set; } = new List<string>();
        string mPeriodicCallback;
        int mFactorX;
        int mFactorY;
        IJoyStickCallBack mCurStation = null;

        public void SetCurSpeedMode(SpeedEnum Mode)
        {
            mSpeedMode = Mode;
        }
        public async Task TriggerCommandAsync(DirectionEnum Dir)
        {
            int set = (int)mCurrentChannelSet;
            mCurDirection = Dir;
            bool syncX = false;
            bool syncY = false;

            if (mChannelSets[set].ChannelX != null && isVectorX())
            {
                SetJoyCurSpeedModeX();
                syncX = mChannelSets[set].ChannelX.Increment(mFactorX * StepX());
            }
            if (mChannelSets[set].ChannelY != null && isVectorY())
            {
                SetJoyCurSpeedModeY();
                syncY = mChannelSets[set].ChannelY.Increment(mFactorY * StepY());
            }
            if (syncX || syncY)
            {
                CallBackFunction(mCallback);
            }


            if (syncX)
            {
                MotorSimulation motor = mChannelSets[set].ChannelX.GetMotor();
                var task = CheckMotionStatusDoneAsyn(motor);
                FailureHandling(await task);
            }
            if (syncY)
            {
                MotorSimulation motor = mChannelSets[set].ChannelY.GetMotor();
                var task = CheckMotionStatusDoneAsyn(motor);
                FailureHandling(await task);
            }


        }

        void FailureHandling(bool Result)
        {
            if (!Result)
            {

            }
        }


        async Task<bool> CheckMotionStatusDoneAsyn(MotorSimulation Motor)
        {
            var task = Task.Run(async () =>
            {
                await Task.Yield();
                while (Motor.Status == MotorSimulation.StatusEnum.EXECUTING)
                { }
            });
            return await Task.WhenAny(task, Task.Delay(1000000)) == task;
        }

        internal void CallBackFunction(string FunctionName)
        {
            mCurStation?.CallBack(FunctionName);
        }

        public void SetJoySeep(double Fast, double Normal, double slow, SETEnum ChannelSet = SETEnum.ONE)
        {
            int set = (int)ChannelSet;
            mSpeedSets[set].Xspeed.Fast = Fast;
            mSpeedSets[set].Xspeed.Normal = Normal;
            mSpeedSets[set].Xspeed.Slow = slow;

            mSpeedSets[set].Yspeed.Fast = Fast;
            mSpeedSets[set].Yspeed.Normal = Normal;
            mSpeedSets[set].Yspeed.Slow = slow;
        }

        public void SetJoySpeedX(double Fast, double Normal, double slow, SETEnum ChannelSet = SETEnum.ONE)
        {
            int set = (int)ChannelSet;
            mSpeedSets[set].Xspeed.Fast = Fast;
            mSpeedSets[set].Xspeed.Normal = Normal;
            mSpeedSets[set].Xspeed.Slow = slow;
        }

        public void SetJoySpeedY(double Fast, double Normal, double slow, SETEnum ChannelSet = SETEnum.ONE)
        {
            int set = (int)ChannelSet;
            mSpeedSets[set].Yspeed.Fast = Fast;
            mSpeedSets[set].Yspeed.Normal = Normal;
            mSpeedSets[set].Yspeed.Slow = slow;
        }

        public void SetJoyCurSpeedMod()
        {
            SetJoyCurSpeedModeX();
            SetJoyCurSpeedModeY();
        }
        bool isVectorX()
        {
            bool isDirUpDown = (mCurDirection == DirectionEnum.UP) || (mCurDirection == DirectionEnum.DOWN);
            return !isDirUpDown;
        }
        bool isVectorY()
        {
            bool isDirLeftRight = (mCurDirection == DirectionEnum.LEFT) || (mCurDirection == DirectionEnum.RIGHT);
            return !isDirLeftRight;
        }

        void SetJoyCurSpeedModeX()
        {
            int set = (int)mCurrentChannelSet;
            if (mSpeedMode == SpeedEnum.FAST)
            {
                mCurSpd_X = mSpeedSets[set].Xspeed.Fast;
            }
            else if (mSpeedMode == SpeedEnum.NORMAL)
            {
                mCurSpd_X = mSpeedSets[set].Xspeed.Normal;
            }
            else
            {
                mCurSpd_X = mSpeedSets[set].Xspeed.Slow;
            }
        }
        void SetJoyCurSpeedModeY()
        {
            int set = (int)mCurrentChannelSet;
            if (mSpeedMode == SpeedEnum.FAST)
            {
                mCurSpd_Y = mSpeedSets[set].Yspeed.Fast;
            }
            else if (mSpeedMode == SpeedEnum.NORMAL)
            {
                mCurSpd_Y = mSpeedSets[set].Yspeed.Normal;
            }
            else
            {
                mCurSpd_Y = mSpeedSets[set].Yspeed.Slow;
            }
        }

        public double GetJoyCurSpeedX() { return mCurSpd_X; }
        public double GetJoyCurSpeedY() { return mCurSpd_Y; }

        double StepX()
        {
            double dx = 0;
            switch (mCurDirection)
            {
                case DirectionEnum.DOWN_LEFT:
                    dx = -mCurSpd_X;
                    break;
                case DirectionEnum.LEFT:
                    dx = -mCurSpd_X;
                    break;
                case DirectionEnum.LEFT_UP:
                    dx = -mCurSpd_X;
                    break;

                case DirectionEnum.DOWN_RIGHT:
                    dx = mCurSpd_X;
                    break;
                case DirectionEnum.RIGHT:
                    dx = mCurSpd_X;
                    break;
                case DirectionEnum.UP_RIGHT:
                    dx = mCurSpd_X;
                    break;
            }
            return dx;
        }
        double StepY()
        {
            double dY = 0;
            switch (mCurDirection)
            {
                case DirectionEnum.DOWN:
                    dY = -mCurSpd_Y;
                    break;
                case DirectionEnum.DOWN_LEFT:
                    dY = -mCurSpd_Y;
                    break;
                case DirectionEnum.DOWN_RIGHT:
                    dY = -mCurSpd_Y;
                    break;

                case DirectionEnum.LEFT_UP:
                    dY = mCurSpd_Y;
                    break;
                case DirectionEnum.UP:
                    dY = mCurSpd_Y;
                    break;
                case DirectionEnum.UP_RIGHT:
                    dY = mCurSpd_Y;
                    break;
            }
            return dY;
        }
        public double GetChannelValue(string Name)
        {
            if (Name == null || Name == string.Empty)
                return 0;
            JoyStickChannel channel = FindChannelByName(Name);
            return channel.GetValue();
        }
        JoyStickChannel FindChannelByName(string Name)
        {
            foreach (JoystickChannelSet set in mChannelSets)
            {
                if (set.ChannelX.channelName == Name)
                {
                    return set.ChannelX;
                }
                else if (set.ChannelY.channelName == Name)
                {
                    return set.ChannelY;
                }
            }
            throw new Exception("NO Such Name in JoystickChannelSet");
        }
        public string GetCurChannelXName()
        {
            int set = (int)mCurrentChannelSet;
            return mChannelSets[set].ChannelX?.channelName;
        }
        public string GetCurChannelYName()
        {
            int set = (int)mCurrentChannelSet;
            return mChannelSets[set].ChannelY?.channelName;
        }
        void Reset()
        {
            mChannelSets.Clear();
            mSpeedSets.Clear();
            mFuctionName.Clear();
            for (int i = 0; i < MAX_NUM_OF_SETS; i++)
            {
                mChannelSets.Add(new JoystickChannelSet());
                mSpeedSets.Add(new JoyStickSpeedSet());
                mFuctionName.Add(string.Empty);
                mFuctionName.Add(string.Empty);
            }


            mCallback = string.Empty;
            mMessage = string.Empty;

            mPeriodicCallback = string.Empty;
            mFactorX = 1;
            mFactorY = 1;
            mCurStation = null;
        }

        public void NextChannelSet()
        {
            if ((int)mCurrentChannelSet < mChannelSets.Count())
            {
                mCurrentChannelSet++;
            }
        }
        public void PreviousChannelSet()
        {
            if (mCurrentChannelSet > 0)
            {
                mCurrentChannelSet--;
            }
        }


        void SetJoyDirection(DirectionEnum Direction)
        {
            mCurDirection = Direction;
        }

        public void SetXChannel(string Name, int Value, MotorSimulation Motor, double PosLimit = int.MaxValue, double NegLimit = int.MinValue, SETEnum Set = SETEnum.ONE)
        {
            int set = (int)Set;
            mChannelSets[set].ChannelX = new JoyStickChannel(Name, Motor, Value, PosLimit, NegLimit);
        }
        public void SetXChannel(string Name, int Value, double PosLimit = int.MaxValue, double NegLimit = int.MinValue, SETEnum Set = SETEnum.ONE)
        {
            SetXChannel(Name, Value, null, PosLimit, NegLimit, Set);
        }

        public void SetYChannel(string Name, int Value, MotorSimulation Motor, double PosLimit = 9999, double NegLimit = 9999, SETEnum Set = SETEnum.ONE)
        {
            int set = (int)Set;
            mChannelSets[set].ChannelY = new JoyStickChannel(Name, Motor, Value, PosLimit, NegLimit);
        }
        public void SetYChannel(string Name, int Value, double PosLimit = 9999, double NegLimit = 9999, SETEnum Set = SETEnum.ONE)
        {
            SetYChannel(Name, Value, null, PosLimit, NegLimit, Set);
        }

        public void SetCurrentStn(IJoyStickCallBack Stn)
        {
            Reset();
            mCurStation = Stn;
        }
        public void SetCallBack(string CallBack) { mCallback = CallBack; }
        public void SetPeriodicCallBack(string CallBack) { mCallback = CallBack; }
        public void SetMessage(string Message) { mMessage = Message; }
        public void SetFunction(string Function1, string Function2 = "", string Function3 = "", string Function4 = "", string Function5 = "", string Function6 = "")
        {
            mFuctionName[0] = Function1;
            mFuctionName[1] = Function2;
            mFuctionName[2] = Function3;
            mFuctionName[3] = Function4;
            mFuctionName[4] = Function5;
            mFuctionName[5] = Function6;
        }
        public void RestoreAllChannelsValue()
        {
            foreach (JoystickChannelSet ch in mChannelSets)
            {
                ch.ChannelX?.Restore();
                ch.ChannelY?.Restore();
            }
        }
        public void SetTitle(string Title) { TitleName = Title; }


        public void ReverseX() { mFactorX = -1; }
        public void ReverseY() { mFactorY = -1; }

        class JoyStickSpeed
        {
            internal JoyStickSpeed() { }
            internal double Fast;
            internal double Normal;
            internal double Slow;
        }

        class JoyStickSpeedSet
        {
            internal JoyStickSpeedSet() { }
            internal JoyStickSpeed Xspeed = new JoyStickSpeed();
            internal JoyStickSpeed Yspeed = new JoyStickSpeed();
        }

        class JoystickChannelSet
        {
            internal JoystickChannelSet() { }
            internal JoyStickChannel ChannelX = null;
            internal JoyStickChannel ChannelY = null;
        }

    }
}