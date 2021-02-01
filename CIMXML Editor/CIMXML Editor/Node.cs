using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CIMXML_Editor
{
	public class Node
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Angle { get; set; }
		public double Scale { get; set; }
		public NodeModel Model { get; private set; }
		public Instance Instance { get; private set; }
		public bool Selected { get; set; }
		public string[] OutReferences { get; private set; }
		public List<string> InReferences { get; private set; }

		public Node(double x, double y, double angle, double scale, NodeModel model, Instance instance)
		{
			X = x;
			Y = y;
			Angle = angle;
			Scale = scale;
			Model = model;
			Instance = instance;
			Selected = false;
			OutReferences = new string[instance.Class.AllAttributes.Count(a => a.Value.Type == AttributeType.Reference)];
			InReferences = new List<string>();
			UpdateOutReferences();
		}

		public Node(Node n)
		{
			X = n.X;
			Y = n.Y;
			Angle = n.Angle;
			Scale = n.Scale;
			Model = n.Model;
			Instance = new Instance(n.Instance);
			Selected = false;
			OutReferences = (string[])n.OutReferences.Clone();
			InReferences = new List<string>(n.InReferences);
		}

		public Shape Draw()
		{
			Shape s = Model.Draw();
			TransformCollection tc = ((TransformGroup)s.RenderTransform).Children;
			tc.Add(new ScaleTransform(Scale, Scale));
			tc.Add(new RotateTransform(Angle));
			tc.Add(new TranslateTransform(X, Y));
			return s;
		}

		public Rect AABB
		{
			get
			{
				double r = Model.Radius * Scale;
				return new Rect(X - r, Y - r, r * 2, r * 2);
			}
		}

		public void UpdateOutReferences()
		{
			int i = 0;
			foreach(KeyValuePair<string, Attribute> kvp in Instance.Class.AllAttributes)
			{
				if(kvp.Value.Type != AttributeType.Reference)
					continue;

				OutReferences[i++] = Instance.GetProperty(kvp.Key);
			}
		}

		public void UpdateOutReference(string mrid, string newMRID)
		{
			foreach(KeyValuePair<string, string> kvp in Instance.GetAllProperties())
				if(kvp.Value == mrid)
					Instance.SetProperty(kvp.Key, newMRID);

			UpdateOutReferences();
		}

		public void RemoveInReference(string mrid)
		{
			InReferences.Remove(mrid);
		}

		public void AddInReference(string mrid)
		{
			InReferences.Add(mrid);
		}
	}
}
