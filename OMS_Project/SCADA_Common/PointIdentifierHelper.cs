namespace SCADA_Common
{
    /// <summary>
    /// Helper class that converts the pointidentifier to integer.
    /// </summary>
    public static class PointIdentifierHelper
	{
        /// <summary>
        /// Converts the point identifier to integer.
        /// </summary>
        /// <param name="pointIdentifier">The point identifier.</param>
        /// <returns>The point identifier in integer form.</returns>
		public static int GetNewPointId(PointIdentifier pointIdentifier)
		{
			return (ushort)pointIdentifier.PointType << 16 ^ pointIdentifier.Address;
		}
	}
}
