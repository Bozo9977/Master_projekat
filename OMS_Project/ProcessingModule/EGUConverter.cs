using System;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for engineering unit conversion.
    /// </summary>
    public class EGUConverter
	{
        /// <summary>
        /// Converts the point value from raw to EGU form.
        /// </summary>
        /// <param name="scalingFactor">The scaling factor.</param>
        /// <param name="deviation">The deviation</param>
        /// <param name="rawValue">The raw value.</param>
        /// <returns>The value in engineering units.</returns>
		public double ConvertToEGU(double scalingFactor, double deviation, ushort rawValue)
		{
			//return scalingFactor * rawValue + deviation;
            return rawValue;
		}

        /// <summary>
        /// Converts the point value from EGU to raw form.
        /// </summary>
        /// <param name="scalingFactor">The scaling factor.</param>
        /// <param name="deviation">The deviation.</param>
        /// <param name="eguValue">The EGU value.</param>
        /// <returns>The raw value.</returns>
		public ushort ConvertToRaw(double scalingFactor, double deviation, double eguValue)
        {
            //return (ushort)Math.Round((eguValue - deviation) / scalingFactor);
            return (ushort)eguValue;
		}
	}
}
