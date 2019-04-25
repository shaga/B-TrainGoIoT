using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_TrainGoIotCore.Models
{
    enum EControlState
    {
        None,
        Initialize,
        Manual,
        AutoDemo,
        SpeechCommand,
    }

    static class StateUtil
    {
        public static bool CheckValue(this EControlState value, object parameter)
        {
            if (!(parameter is string)) return false;

            var paramValue = EControlState.None;

            if (!Enum.TryParse((string) parameter, out paramValue)) return false;

            return value == paramValue;
        }
    }
}
