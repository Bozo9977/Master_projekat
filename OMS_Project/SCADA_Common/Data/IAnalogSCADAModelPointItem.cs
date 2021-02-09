using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.Data
{
    public interface IAnalogSCADAModelPointItem
    {
        float CurrentEguValue { get; set; }
        float NormalValue { get; set; }
        float EGU_Min { get; set; }
        float EGU_Max { get; set; }
        float ScalingFactor { get; set; }
        float Deviation { get; set; }

        int CurrentRawValue { get; }
        int MinRawValue { get; }
        int MaxRawValue { get; }

        float RawToEguValueConversion(int rawValue);
        int EguToRawValueConversion(float eguValue);
    }
}
