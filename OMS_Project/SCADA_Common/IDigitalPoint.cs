namespace SCADA_Common
{
    /// <summary>
    /// Interface representing digital point specific values.
    /// </summary>
    public interface IDigitalPoint : IPoint
    {
        /// <summary>
        /// Gets or sets the state of the digital point.
        /// </summary>
  		DState State { get; set; }
    }
}
