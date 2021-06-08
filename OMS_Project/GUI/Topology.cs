﻿using Common.CalculationEngine;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GUI
{
	public class Topology
	{
		ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

		HashSet<Tuple<long, long>> unknownLines;
		HashSet<Tuple<long, long>> energizedLines;
		HashSet<long> unknownNodes;
		HashSet<long> energizedNodes;

		public Topology()
		{
			unknownLines = new HashSet<Tuple<long, long>>(0);
			energizedLines = new HashSet<Tuple<long, long>>(0);
			unknownNodes = new HashSet<long>(0);
			energizedNodes = new HashSet<long>(0);
		}

		public EEnergization GetNodeEnergization(long gid)
		{
			rwLock.EnterReadLock();

			try
			{
				if(unknownNodes.Contains(gid))
					return EEnergization.Unknown;

				if(energizedNodes.Contains(gid))
					return EEnergization.Energized;

				return EEnergization.NotEnergized;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public EEnergization GetLineEnergization(long gid1, long gid2)
		{
			if(gid1 > gid2)
			{
				long temp = gid1;
				gid1 = gid2;
				gid2 = temp;
			}

			rwLock.EnterReadLock();

			try
			{
				Tuple<long, long> tuple = new Tuple<long, long>(gid1, gid2);

				if(unknownLines.Contains(tuple))
					return EEnergization.Unknown;

				if(energizedLines.Contains(tuple))
					return EEnergization.Energized;

				return EEnergization.NotEnergized;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void Update(TopologyDownload download)
		{
			List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> topology = download.Topology;

			HashSet<Tuple<long, long>> unknownLines = new HashSet<Tuple<long, long>>();
			HashSet<Tuple<long, long>> energizedLines = new HashSet<Tuple<long, long>>();
			HashSet<long> unknownNodes = new HashSet<long>();
			HashSet<long> energizedNodes = new HashSet<long>();

			for(int i1 = 0; i1 < topology.Count; ++i1)
			{
				Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>> source = topology[i1];

				for(int i2 = 0; i2 < source.Item2.Count; ++i2)
				{
					Tuple<long, long> energizedLine = source.Item2[i2];

					energizedLines.Add(energizedLine);
					unknownLines.Remove(energizedLine);

					energizedNodes.Add(energizedLine.Item1);
					unknownNodes.Remove(energizedLine.Item1);

					energizedNodes.Add(energizedLine.Item2);
					unknownNodes.Remove(energizedLine.Item2);
				}

				for(int i3 = 0; i3 < source.Item3.Count; ++i3)
				{
					Tuple<long, long> unknownLine = source.Item3[i3];

					if(!energizedLines.Contains(unknownLine))
						unknownLines.Add(unknownLine);

					if(!energizedNodes.Contains(unknownLine.Item1))
						unknownNodes.Add(unknownLine.Item1);

					if(!energizedNodes.Contains(unknownLine.Item2))
						unknownNodes.Add(unknownLine.Item2);
				}
			}

			rwLock.EnterWriteLock();
			{
				this.unknownLines = unknownLines;
				this.energizedLines = energizedLines;
				this.unknownNodes = unknownNodes;
				this.energizedNodes = energizedNodes;
			}
			rwLock.ExitWriteLock();
		}
	}
}