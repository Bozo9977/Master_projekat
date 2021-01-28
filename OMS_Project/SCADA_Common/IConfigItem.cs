namespace SCADA_Common
{
    /// <summary>
    /// Interface containing properties of the configuration.
    /// </summary>
	public interface IConfigItem
	{
        /// <summary>
        /// Gets a value indicating the registry type.
        /// </summary>
		PointType RegistryType { get; }

        /// <summary>
        /// Gets the number of registers.
        /// </summary>
        ushort NumberOfRegisters { get; }

        /// <summary>
        /// Gets the start address.
        /// </summary>
        ushort StartAddress { get; }

        /// <summary>
        /// Gets the decimal separator`s place.
        /// </summary>
        ushort DecimalSeparatorPlace { get; }

        /// <summary>
        /// Gets the minimal value.
        /// </summary>
        ushort MinValue { get; }

        /// <summary>
        /// Getsthe maximal value.
        /// </summary>
        ushort MaxValue { get; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        ushort DefaultValue { get; }

        /// <summary>
        /// Gets the processing type.
        /// </summary>
        string ProcessingType { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the acquisition interval.
        /// </summary>
        int AcquisitionInterval { get; }

        /// <summary>
        /// Gets the scale factor.
        /// </summary>
        double ScaleFactor { get; }

        /// <summary>
        /// Gets the deviation.
        /// </summary>
        double Deviation { get; }

        /// <summary>
        /// Gets the minimal value in engineering units.
        /// </summary>
        double EGU_Min { get; }

        /// <summary>
        /// Gets the maximal value in engineering units.
        /// </summary>
        double EGU_Max { get; }

        /// <summary>
        /// Gets the abnormal value.
        /// </summary>
		ushort AbnormalValue { get; }

        /// <summary>
        /// Gets high alarm limit.
        /// </summary>
		double HighLimit { get; }

        /// <summary>
        /// Gets the low alarm limit.
        /// </summary>
        double LowLimit { get; }

        /// <summary>
        /// Gets or sets the time passed since last poll was issued.
        /// </summary>
        int SecondsPassedSinceLastPoll { get; set; }
    }
}
