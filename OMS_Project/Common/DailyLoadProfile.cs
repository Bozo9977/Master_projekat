using Common.GDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Common
{
	public class DailyLoadProfilePoint : IComparable<DailyLoadProfilePoint>
	{
		public int Hours { get; private set; }
		public float Value { get; private set; }

		public DailyLoadProfilePoint(int h, float val)
		{
			Hours = h;
			Value = val;
		}

		public int CompareTo(DailyLoadProfilePoint other)
		{
			return Hours - other.Hours;
		}
	}

	public class DailyLoadProfile
	{
		public ConsumerClass ConsumerClass { get; private set; }
		List<DailyLoadProfilePoint> points;

		public DailyLoadProfile(ConsumerClass consumerClass, IEnumerable<DailyLoadProfilePoint> points)
		{
			this.ConsumerClass = consumerClass;
			this.points = new List<DailyLoadProfilePoint>(points);
			this.points.Sort();
		}

		public float Get(int h, int min)
		{
			if(points.Count <= 0 || h < 0 || h > 24 || min < 0 || min > 60)
				return float.NaN;

			int i;
			for(i = 0; i < points.Count && h > points[i].Hours; ++i)
			{ }

			DailyLoadProfilePoint p2;
			if(i >= points.Count)
			{
				p2 = points[0];
				p2 = new DailyLoadProfilePoint(p2.Hours + 24, p2.Value);
			}
			else
			{
				p2 = points[i];
			}

			DailyLoadProfilePoint p1;
			if(i == 0)
			{
				p1 = points[points.Count - 1];
				p1 = new DailyLoadProfilePoint(p1.Hours - 24, p1.Value);
			}
			else
			{
				p1 = points[i - 1];
			}

			return Interpolate(p1.Hours, p1.Value, p2.Hours, p2.Value, h + (float)min / 60);
		}

		float Interpolate(float x1, float y1, float x2, float y2, float x)
		{
			float alpha = Math.Abs(x - x2) / Math.Abs(x1 - x2);
			return alpha * y1 + (1 - alpha) * y2;
		}

		public static List<DailyLoadProfile> LoadFromXML(string filePath)
		{
			List<DailyLoadProfile> result = new List<DailyLoadProfile>();

			try
			{
				using(FileStream fs = File.OpenRead(filePath))
				using(XmlReader reader = XmlReader.Create(fs))
				{
					bool profileFound = false;
					List<DailyLoadProfilePoint> points = new List<DailyLoadProfilePoint>(24);
					ConsumerClass consumerClass = 0;

					while(reader.Read())
					{
						switch(reader.NodeType)
						{
							case XmlNodeType.Element:
								switch(reader.Name)
								{
									case "DailyLoadProfile":
										profileFound = Enum.TryParse<ConsumerClass>(reader.GetAttribute(nameof(consumerClass)), out consumerClass);
										break;

									case "Point":
										if(!profileFound)
											break;

										int h;
										float val;

										if(!int.TryParse(reader.GetAttribute("h"), out h) || !float.TryParse(reader.GetAttribute("p"), out val))
											continue;

										points.Add(new DailyLoadProfilePoint(h, val));
										break;
								}

								break;

							case XmlNodeType.EndElement:
								if(reader.Name != "DailyLoadProfile")
									continue;

								result.Add(new DailyLoadProfile(consumerClass, points));
								points.Clear();
								profileFound = false;
								break;
						}
					}
				}
			}
			catch(Exception e)
			{
				result = new List<DailyLoadProfile>();
			}

			return result;
		}
	}
}
