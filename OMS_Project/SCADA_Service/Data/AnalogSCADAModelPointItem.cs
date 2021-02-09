using Common.GDA;
using SCADA_Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service.Data
{
    public class AnalogSCADAModelPointItem : SCADAModelPointItem, IAnalogSCADAModelPointItem
    {
        public AnalogSCADAModelPointItem(List<Property> props, ModelCode type)
            : base(props, type)
        {
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.ANALOG_MAXVALUE:
                        EGU_Max = ((FloatProperty)item).Value;
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        EGU_Min = ((FloatProperty)item).Value;
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        NormalValue = ((FloatProperty)item).Value;
                        break;

                    /*case ModelCode.ANALOG_SCALINGFACTOR:
                        ScalingFactor = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_DEVIATION:
                        Deviation = item.AsFloat();
                        break;*/

                    default:
                        break;
                }
            }

            Initialized = true;
        }

        private float currentEguValue;
        public float NormalValue { get; set; }
        public float CurrentEguValue
        {
            get { return currentEguValue; }
            set
            {
                currentEguValue = value;
            }
        }
        public float EGU_Min { get; set; }
        public float EGU_Max { get; set; }
        public float ScalingFactor { get; set; }
        public float Deviation { get; set; }

        public int CurrentRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        public int MinRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        public int MaxRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }


        #region Conversions

        public float RawToEguValueConversion(int rawValue)
        {
            float eguValue = ((ScalingFactor * rawValue) + Deviation);

            if (eguValue > float.MaxValue || eguValue < float.MinValue)
            {
                throw new Exception($"Egu value: {eguValue} is out of float data type boundaries [{float.MinValue}, {float.MaxValue}]");
            }

            return eguValue;
        }

        public int EguToRawValueConversion(float eguValue)
        {
            if (ScalingFactor == 0)
            {
                throw new DivideByZeroException($"Scaling factor is zero.");
            }

            int rawValue = (int)((eguValue - Deviation) / ScalingFactor);

            return rawValue;
        }

        #endregion
    }
}
