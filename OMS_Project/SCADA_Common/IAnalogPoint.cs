namespace SCADA_Common
{
    /// <summary>
    /// Interface representing analog point specific values.
    /// </summary>
    public interface IAnalogPoint : IPoint
    {
        /// <summary>
        /// Gets or sets the value in engineering units.
        /// </summary>
        double EguValue { get; set; }
    }
}
