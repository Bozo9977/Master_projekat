using System;
using System.Collections.Generic;

namespace SCADA_Common
{
    /// <summary>
    /// Structure representing the point identifier
    /// </summary>
    public struct PointIdentifier
    {
        public PointType PointType;
        public ushort Address;

        public PointIdentifier(PointType pointType, ushort address)
        {
            this.PointType = pointType;
            this.Address = address;
        }
    }

    /// <summary>
    /// Interface containing logic for reading points from storage.
    /// </summary>
    public interface IStorage
	{
        /// <summary>
        /// Returns the list of points in the requested order.
        /// </summary>
        /// <param name="pointIds">The identifiers of the points that are requested.</param>
        /// <returns>The list of poitns read from the storage.</returns>
		List<IPoint> GetPoints(List<PointIdentifier> pointIds);
	}
}
