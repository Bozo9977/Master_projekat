using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CIMXML_Editor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		enum EKey : byte { Up, Down, Left, Right, MoveUp, MoveDown, MoveLeft, MoveRight, In, Out, Rotate, DeselectAll, Add, Deselect, Edit, Delete, ShowRefs, Copy, Paste, Save, Load, Export, Count }
		uint keys;
		const byte repeatedKeysCount = 11;
		Action[] keyActions;
		byte keyCount;
		DispatcherTimer timer;
		Rect canvasPos;
		const double moveDelta = 0.02;
		const double zoomDelta = 1.05;
		Dictionary<string, Node> nodes;
		List<Node> selectedNodes;
		Vector canvasSize;
		double aspectRatio;
		double zoom;
		bool loaded;
		Profile profile;
		Dictionary<string, NodeModel> classToModel;
		const double angleDelta = 2;
		Dictionary<Class, int> typeCounts;
		Dictionary<Class, bool> showRefs;
		List<Node> copiedNodes;

		public MainWindow()
		{
			InitializeComponent();

			keyActions = new Action[(byte)EKey.Count] { Up, Down, Left, Right, MoveUp, MoveDown, MoveLeft, MoveRight, In, Out, Rotate, DeselectAll, Add, Deselect, Edit, Delete, ShowRefs, Copy, Paste, Save, Load, Export };
			
			profile = new Profile();
			classToModel = new Dictionary<string, NodeModel>(profile.ConcreteClasses.Count);
			classToModel["ConnectivityNode"] = new NodeModels.ConnectivityNode();
			classToModel["BaseVoltage"] = new NodeModels.BaseVoltage();
			classToModel["EnergyConsumer"] = new NodeModels.EnergyConsumer();
			classToModel["ACLineSegment"] = new NodeModels.ACLineSegment();
			classToModel["Disconnector"] = new NodeModels.Disconnector();
			classToModel["Breaker"] = new NodeModels.Breaker();
			classToModel["Recloser"] = new NodeModels.Recloser();
			classToModel["DistributionGenerator"] = new NodeModels.DistributionGenerator();
			classToModel["PowerTransformer"] = new NodeModels.PowerTransformer();
			classToModel["TransformerWinding"] = new NodeModels.TransformerWinding();
			classToModel["RatioTapChanger"] = new NodeModels.RatioTapChanger();
			classToModel["EnergySource"] = new NodeModels.EnergySource();
			classToModel["Terminal"] = new NodeModels.Terminal();
			classToModel["Analog"] = new NodeModels.Analog();
			classToModel["Discrete"] = new NodeModels.Discrete();
			Init();

			timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(32) };
			timer.Tick += Timer_Tick;
		}

		private void Export()
		{
			SaveFileDialog w = new SaveFileDialog() { CheckFileExists = false, ValidateNames = true };

			if(w.ShowDialog() != true)
				return;

			try
			{
				using(StreamWriter sw = new StreamWriter(File.Open(w.FileName, FileMode.Create, FileAccess.Write)))
				{
					sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
					sw.WriteLine("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:cim=\"http://iec.ch/TC57/2010/CIM-schema-cim15#\">");

					foreach(KeyValuePair<string, Node> kvp in nodes)
					{
						Node n = kvp.Value;

						sw.WriteLine("\t<cim:" + n.Instance.Class.Name + " rdf:ID=\"" + kvp.Key + "\">");

						for(Class c = n.Instance.Class; c != null; c = c.Base)
						{
							string cimClassName = "cim:" + c.Name;

							foreach(KeyValuePair<string, Attribute> akvp in c.Attributes)
							{
								Attribute a = akvp.Value;
								string value = n.Instance.GetProperty(a.Name);

								if(string.IsNullOrWhiteSpace(value))
									continue;

								string cimPropertyName = cimClassName + "." + a.Name;

								if(a.Type == AttributeType.Reference)
								{
									sw.WriteLine("\t\t<" + cimPropertyName + " rdf:resource=\"#" + value + "\"/>");
								}
								else
								{
									sw.WriteLine("\t\t<" + cimPropertyName + ">" + value + "</" + cimPropertyName + ">");
								}
							}
						}

						sw.WriteLine("\t</cim:" + n.Instance.Class.Name + ">");
					}

					sw.WriteLine("</rdf:RDF>");
				}
			}
			catch(Exception e)
			{ }
		}

		private void InitView()
		{
			zoom = 0.08;
			canvasPos = new Rect(aspectRatio / zoom * -0.5, -0.5 / zoom, aspectRatio / zoom, 1 / zoom);
		}

		void Init()
		{
			nodes = new Dictionary<string, Node>();
			selectedNodes = new List<Node>();
			typeCounts = new Dictionary<Class, int>(profile.ConcreteClasses.Count);
			showRefs = new Dictionary<Class, bool>(profile.ConcreteClasses.Count);

			foreach(Class c in profile.ConcreteClasses)
			{
				typeCounts.Add(c, 0);
				showRefs.Add(c, true);
			}

			copiedNodes = new List<Node>();
		}

		private void Load()
		{
			MessageBoxResult mbr = MessageBox.Show("Lose unsaved changes and load from file?", "Load", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

			if(mbr != MessageBoxResult.Yes)
				return;

			OpenFileDialog w = new OpenFileDialog() { CheckFileExists = true, Multiselect = false };

			if(w.ShowDialog() != true)
				return;

			Init();
			InitView();

			try
			{
				using(FileStream fs = File.Open(w.FileName, FileMode.Open, FileAccess.Read))
				{
					FileProxy f = new FileProxy(fs);
					int count = f.ReadInt();

					for(int i = 0; i < count; ++i)
					{
						string key = f.ReadString();
						int value = f.ReadInt();
						Class c;

						if(profile.Classes.TryGetValue(key, out c) && profile.ConcreteClasses.Contains(c))
							typeCounts[c] = value;
					}

					count = f.ReadInt();
					nodes = new Dictionary<string, Node>(count);

					for(int i = 0; i < count; ++i)
					{
						double x = f.ReadDouble();
						double y = f.ReadDouble();
						double angle = f.ReadDouble();
						double scale = f.ReadDouble();
						string className = f.ReadString();
						Class c;
						NodeModel nodeModel;

						if(!profile.Classes.TryGetValue(className, out c) || !profile.ConcreteClasses.Contains(c) || !classToModel.TryGetValue(className, out nodeModel))
							continue;

						Instance instance = new Instance(c);
						int fieldCount = f.ReadInt();

						for(int j = 0; j < fieldCount; ++j)
						{
							string key = f.ReadString();
							string value = f.ReadString();
							instance.SetProperty(key, value);
						}

						string mrid = instance.GetProperty("mRID");

						if(string.IsNullOrWhiteSpace(mrid))
							continue;

						nodes.Add(mrid, new Node(x, y, angle, scale, nodeModel, instance));
					}
				}
			}
			catch(Exception e)
			{ }

			foreach(KeyValuePair<string, Node> kvp in nodes)
			{
				Node n = kvp.Value;
				n.UpdateOutReferences();

				foreach(string target in n.OutReferences)
				{
					Node targetNode;

					if(!nodes.TryGetValue(target, out targetNode))
						continue;

					targetNode.AddInReference(kvp.Key);
				}
			}

			Redraw();
		}

		private void Save()
		{
			SaveFileDialog w = new SaveFileDialog() { CheckFileExists = false, ValidateNames = true };

			if(w.ShowDialog() != true)
				return;

			try
			{
				using(FileStream fs = File.Open(w.FileName, FileMode.Create, FileAccess.Write))
				{
					FileProxy f = new FileProxy(fs);
					f.WriteInt(typeCounts.Count);

					foreach(KeyValuePair<Class, int> kvp in typeCounts)
					{
						f.WriteString(kvp.Key.Name);
						f.WriteInt(kvp.Value);
					}

					f.WriteInt(nodes.Count);

					foreach(KeyValuePair<string, Node> kvp in nodes)
					{
						Node n = kvp.Value;
						f.WriteDouble(n.X);
						f.WriteDouble(n.Y);
						f.WriteDouble(n.Angle);
						f.WriteDouble(n.Scale);
						f.WriteString(n.Instance.Class.Name);

						Dictionary<string, string> fields = n.Instance.GetAllProperties();
						f.WriteInt(fields.Count);

						foreach(KeyValuePair<string, string> kvpair in fields)
						{
							f.WriteString(kvpair.Key);
							f.WriteString(kvpair.Value);
						}
					}
				}
			}
			catch(Exception e)
			{ }
		}

		private void Paste()
		{
			if(copiedNodes.Count <= 0)
				return;

			List<Node> copiedNodesCopy = new List<Node>(copiedNodes.Count);
			Dictionary<string, string> idMapping = new Dictionary<string, string>(copiedNodes.Count);

			foreach(Node n in copiedNodes)
			{
				Node newNode = new Node(n);

				string newMRID = newNode.Instance.Class.Name + "_" + typeCounts[newNode.Instance.Class]++;
				idMapping[newNode.Instance.GetProperty("mRID")] = newMRID;
				newNode.Instance.SetProperty("mRID", newMRID);
				newNode.Instance.SetProperty("name", newMRID);
				newNode.Selected = true;
				nodes.Add(newMRID, newNode);
				newNode.InReferences.Clear();

				copiedNodesCopy.Add(newNode);
			}

			foreach(Node n in copiedNodesCopy)
			{
				string mrid = n.Instance.GetProperty("mRID");

				foreach(KeyValuePair<string, Attribute> kvp in n.Instance.Class.AllAttributes)
				{
					if(kvp.Value.Type != AttributeType.Reference)
						continue;
					
					string oldRef = n.Instance.GetProperty(kvp.Key);
					string newRef;

					if(idMapping.TryGetValue(oldRef, out newRef))
					{
						n.Instance.SetProperty(kvp.Key, newRef);
					}
					else
					{
						newRef = oldRef;
					}

					Node targetNode;

					if(!nodes.TryGetValue(newRef, out targetNode))
						continue;

					targetNode.AddInReference(mrid);
				}

				n.UpdateOutReferences();
			}

			foreach(Node n in selectedNodes)
				n.Selected = false;

			selectedNodes.Clear();
			selectedNodes = new List<Node>(copiedNodesCopy);
			Redraw();
		}

		private void Copy()
		{
			copiedNodes = new List<Node>(selectedNodes.Count);

			foreach(Node n in selectedNodes)
				copiedNodes.Add(new Node(n));
		}

		private void DeselectAll()
		{
			if(selectedNodes.Count <= 0)
				return;

			foreach(Node n in selectedNodes)
				n.Selected = false;

			selectedNodes.Clear();
			Redraw();
		}

		private void ShowRefs()
		{
			List<Tuple<bool, string>> param = new List<Tuple<bool, string>>(showRefs.Count);

			foreach(KeyValuePair<Class, bool> kvp in showRefs)
				param.Add(new Tuple<bool, string>(kvp.Value, kvp.Key.Name));

			SelectWindow w = new SelectWindow((IReadOnlyList<Tuple<bool, string>>)param) { Title = "Show references" };
			w.ShowDialog();

			List<bool> result = w.Result;

			if(result == null)
				return;

			for(int i = 0; i < result.Count; ++i)
				showRefs[profile.Classes[param[i].Item2]] = result[i];

			Redraw();
		}

		private void MoveRight()
		{
			if(selectedNodes.Count <= 0)
				return;

			foreach(Node n in selectedNodes)
				n.X += moveDelta / zoom / 4;

			Redraw();
		}

		private void MoveLeft()
		{
			if(selectedNodes.Count <= 0)
				return;

			foreach(Node n in selectedNodes)
				n.X -= moveDelta / zoom / 4;

			Redraw();
		}

		private void MoveDown()
		{
			if(selectedNodes.Count <= 0)
				return;

			foreach(Node n in selectedNodes)
				n.Y += moveDelta / zoom / 4;

			Redraw();
		}

		private void MoveUp()
		{
			if(selectedNodes.Count <= 0)
				return;

			foreach(Node n in selectedNodes)
				n.Y -= moveDelta / zoom / 4;

			Redraw();
		}

		private void Rotate()
		{
			foreach(Node n in selectedNodes)
			{
				if((n.Angle += angleDelta) >= 360)
					n.Angle -= 360;
			}

			Redraw();
		}

		private void Delete()
		{
			if(selectedNodes.Count <= 0)
				return;

			MessageBoxResult mbr = MessageBox.Show("Delete " + selectedNodes.Count + " element" + (selectedNodes.Count == 1 ? "": "s") + "?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

			if(mbr != MessageBoxResult.Yes)
				return;

			foreach(Node n in selectedNodes)
			{
				string mrid = n.Instance.GetProperty("mRID");
				nodes.Remove(mrid);

				foreach(string target in n.OutReferences)
				{
					Node targetNode;

					if(!nodes.TryGetValue(target, out targetNode))
						continue;

					targetNode.RemoveInReference(mrid);
				}

				foreach(string source in n.InReferences)
				{
					Node sourceNode;

					if(!nodes.TryGetValue(source, out sourceNode))
						continue;

					sourceNode.UpdateOutReference(mrid, "");
				}
			}

			selectedNodes.Clear();
			Redraw();
		}

		private void Edit()
		{
			if(selectedNodes.Count <= 0)
				return;

			if(selectedNodes.Count == 1)
			{
				EditNode(selectedNodes[0]);
				return;
			}

			PickWindow w = new PickWindow(selectedNodes.Select(x => x.Instance.GetProperty("mRID") + " : " + x.Instance.Class.Name).ToList());
			w.ShowDialog();

			int i = w.Result;

			if(i < 0)
				return;

			EditNode(selectedNodes[i]);
		}

		void EditNode(Node node)
		{
			List<Input> inputs = new List<Input>();

			foreach(KeyValuePair<string, Attribute> kvp in node.Instance.Class.AllAttributes)
			{
				Input input = new Input() { Name = kvp.Key, Value = node.Instance.GetProperty(kvp.Key) };

				switch(kvp.Value.Type)
				{
					case AttributeType.Bool:
						input.OfferedValues = new List<string>() { "false", "true" };
						break;

					case AttributeType.Enum:
						input.OfferedValues = kvp.Value.TargetType.AllAttributes.Keys.ToList();
						break;

					case AttributeType.Reference:
						input.OfferedValues = GetNearestTargets(node.X, node.Y, kvp.Value.TargetType);
						break;
				}

				inputs.Add(input);
			}

			EditWindow w = new EditWindow(inputs);
			w.ShowDialog();
			Dictionary<string, string> fields = w.Result;

			if(fields == null)
				return;

			string mrid = node.Instance.GetProperty("mRID");
			string newMRID = fields["mRID"];		

			if(newMRID != mrid)
			{
				if(string.IsNullOrWhiteSpace(newMRID))
					return;

				if(nodes.ContainsKey(newMRID))
					return;

				nodes.Remove(mrid);
				nodes.Add(newMRID, node);

				foreach(string source in node.InReferences)
				{
					Node sourceNode;

					if(!nodes.TryGetValue(source, out sourceNode))
						continue;

					sourceNode.UpdateOutReference(mrid, newMRID);
				}
			}

			foreach(string target in node.OutReferences)
			{
				Node targetNode;

				if(!nodes.TryGetValue(target, out targetNode))
					continue;

				targetNode.RemoveInReference(mrid);
			}

			foreach(KeyValuePair<string, string> kvp in fields)
				node.Instance.SetProperty(kvp.Key, kvp.Value);

			node.UpdateOutReferences();

			foreach(string target in node.OutReferences)
			{
				Node targetNode;

				if(!nodes.TryGetValue(target, out targetNode))
					continue;

				targetNode.AddInReference(newMRID);
			}

			Redraw();
		}

		double SquaredDistance(double x1, double y1, double x2, double y2)
		{
			return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
		}

		private List<string> GetNearestTargets(double x, double y, Class targetType)
		{
			return nodes.Where(a => a.Value.Instance.Class.IsSubtypeOf(targetType)).OrderBy(a => SquaredDistance(a.Value.X, a.Value.Y, x, y)).Select(a => a.Key).ToList();
		}

		bool GetKey(byte i)
		{
			return (keys & (1u << i)) != 0;
		}

		bool SetKey(byte i)
		{
			uint old = keys;
			keys |= (1u << i);
			return old == keys;
		}

		bool ClearKey(byte i)
		{
			uint old = keys;
			keys &= ~(1u << i);
			return old != keys;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			byte i = 0;
			uint k = keys;

			while(k != 0 && i < repeatedKeysCount)
			{
				if((k & 1u) != 0)
					keyActions[i]();

				k >>= 1;
				++i;
			}
		}

		private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			timer.Stop();
			keys = 0;
			keyCount = 0;
		}

		EKey MapKey(Key k)
		{
			switch(k)
			{
				case Key.Up:
					return EKey.Up;

				case Key.Down:
					return EKey.Down;

				case Key.Left:
					return EKey.Left;

				case Key.Right:
					return EKey.Right;

				case Key.W:
					return EKey.MoveUp;

				case Key.S:
					return EKey.MoveDown;

				case Key.A:
					return EKey.MoveLeft;

				case Key.D:
					return EKey.MoveRight;

				case Key.LeftShift:
					return EKey.In;

				case Key.LeftCtrl:
					return EKey.Out;

				case Key.Insert:
					return EKey.Add;

				case Key.Escape:
					return EKey.DeselectAll;

				case Key.Space:
					return EKey.Deselect;

				case Key.E:
					return EKey.Edit;

				case Key.Delete:
					return EKey.Delete;

				case Key.R:
					return EKey.Rotate;

				case Key.Q:
					return EKey.ShowRefs;

				case Key.C:
					return EKey.Copy;

				case Key.V:
					return EKey.Paste;

				case Key.O:
					return EKey.Save;

				case Key.I:
					return EKey.Load;

				case Key.X:
					return EKey.Export;
			}

			return EKey.Count;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if(!loaded)
				return;

			EKey k = MapKey(e.Key);

			if(k == EKey.Count)
				return;

			if(!SetKey((byte)k))
			{
				keyActions[(byte)k]();

				if((byte)k < repeatedKeysCount && keyCount++ == 0)
					timer.Start();
			}
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if(!loaded)
				return;

			EKey k = MapKey(e.Key);

			if(k == EKey.Count)
				return;

			if(ClearKey((byte)k))
			{
				if((byte)k < repeatedKeysCount && --keyCount == 0)
					timer.Stop();
			}
		}

		void Up()
		{
			canvasPos = new Rect(canvasPos.Left, canvasPos.Top - moveDelta / zoom, canvasPos.Width, canvasPos.Height);
			Redraw();
		}

		void Down()
		{
			canvasPos = new Rect(canvasPos.Left, canvasPos.Top + moveDelta / zoom, canvasPos.Width, canvasPos.Height);
			Redraw();
		}

		void Left()
		{
			canvasPos = new Rect(canvasPos.Left - moveDelta / zoom, canvasPos.Top, canvasPos.Width, canvasPos.Height);
			Redraw();
		}

		void Right()
		{
			canvasPos = new Rect(canvasPos.Left + moveDelta / zoom, canvasPos.Top, canvasPos.Width, canvasPos.Height);
			Redraw();
		}

		void In()
		{
			zoom *= zoomDelta;
			Vector center = GetCenter();
			Vector newSize = new Vector(aspectRatio / zoom, 1.0 / zoom);
			canvasPos = new Rect(center.X - newSize.X * 0.5, center.Y - newSize.Y * 0.5, newSize.X, newSize.Y);
			Redraw();
		}

		void Out()
		{
			zoom /= zoomDelta;
			Vector center = GetCenter();
			Vector newSize = new Vector(aspectRatio / zoom, 1.0 / zoom);
			canvasPos = new Rect(center.X - newSize.X * 0.5, center.Y - newSize.Y * 0.5, newSize.X, newSize.Y);
			Redraw();
		}

		Vector GetCenter()
		{
			return new Vector(canvasPos.Left * 0.5 + canvasPos.Right * 0.5, canvasPos.Top * 0.5 + canvasPos.Bottom * 0.5);
		}

		private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(!loaded)
				return;

			zoom *= canvasSize.Y / e.NewSize.Height;
			canvasPos = new Rect(canvasPos.Left, canvasPos.Top, canvasPos.Width * (e.NewSize.Width / canvasSize.X), canvasPos.Height * (e.NewSize.Height / canvasSize.Y));
			canvasSize = new Vector(e.NewSize.Width, e.NewSize.Height);
			aspectRatio = canvasSize.X / canvasSize.Y;
			Redraw();
		}

		private void Redraw()
		{
			TranslateTransform tt = new TranslateTransform(-canvasPos.Left, -canvasPos.Top);
			ScaleTransform st = new ScaleTransform(canvasSize.Y * zoom, canvasSize.Y * zoom);
			TransformGroup tg = new TransformGroup() { Children = new TransformCollection() { tt, st } };
			List<Shape> shapes = new List<Shape>();
			List<Polyline> selections = new List<Polyline>();
			HashSet<string> addedNodes = new HashSet<string>();

			canvas.Children.Clear();

			foreach(KeyValuePair<string, Node> kvp in nodes)
			{
				Node node = kvp.Value;
				Rect aabb = node.AABB;

				if(!aabb.IntersectsWith(canvasPos))
					continue;

				Shape s = node.Draw();
				TransformCollection tc = ((TransformGroup)s.RenderTransform).Children;
				tc.Add(tt);
				tc.Add(st);
				shapes.Add(s);
				addedNodes.Add(kvp.Key);

				if(node.Selected)
				{
					Polyline p = new Polyline() { Stroke = Brushes.Red, StrokeThickness = 2, Points = new PointCollection() { tg.Transform(aabb.TopLeft), tg.Transform(aabb.TopRight), tg.Transform(aabb.BottomRight), tg.Transform(aabb.BottomLeft), tg.Transform(aabb.TopLeft) } };
					selections.Add(p);
				}

				if(!showRefs[node.Instance.Class])
					continue;

				Point p1 = tg.Transform(new Point(node.X, node.Y));

				foreach(string target in node.OutReferences)
				{
					Node targetNode;

					if(addedNodes.Contains(target) || !nodes.TryGetValue(target, out targetNode) || !showRefs[targetNode.Instance.Class])
						continue;

					Point p2 = tg.Transform(new Point(targetNode.X, targetNode.Y));

					Line line = new Line() { Stroke = Brushes.Black, StrokeThickness = 2, X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
					canvas.Children.Add(line);
				}

				foreach(string target in node.InReferences)
				{
					Node targetNode;

					if(addedNodes.Contains(target) || !nodes.TryGetValue(target, out targetNode) || !showRefs[targetNode.Instance.Class])
						continue;

					Point p2 = tg.Transform(new Point(targetNode.X, targetNode.Y));

					Line line = new Line() { Stroke = Brushes.Black, StrokeThickness = 2, X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
					canvas.Children.Add(line);
				}
			}

			foreach(Shape shape in shapes)
				canvas.Children.Add(shape);

			foreach(Polyline p in selections)
				canvas.Children.Add(p);
		}

		private void canvas_Loaded(object sender, RoutedEventArgs e)
		{
			loaded = true;
			canvasSize = new Vector(canvas.ActualWidth, canvas.ActualHeight);
			aspectRatio = canvasSize.X / canvasSize.Y;
			InitView();
			Redraw();
		}

		private void Add()
		{
			PickWindow pw = new PickWindow(profile.ConcreteClasses.Select(x => x.Name).ToList());
			pw.ShowDialog();

			if(pw.Result < 0)
				return;

			Class type = profile.ConcreteClasses[pw.Result];
			List<Input> inputs = new List<Input>();
			Vector center = GetCenter();

			foreach(KeyValuePair<string, Attribute> kvp in type.AllAttributes)
			{
				Input input = new Input() { Name = kvp.Key };

				switch(kvp.Key)
				{
					case "mRID":
					case "name":
						input.Value = type.Name + "_" + typeCounts[type];
						break;

					default:
						input.Value = "";
						break;
				}

				switch(kvp.Value.Type)
				{
					case AttributeType.Bool:
						input.OfferedValues = new List<string>() { "false", "true" };
						break;

					case AttributeType.Enum:
						input.OfferedValues = kvp.Value.TargetType.AllAttributes.Keys.ToList();
						break;

					case AttributeType.Reference:
						input.OfferedValues = GetNearestTargets(center.X, center.Y, kvp.Value.TargetType);
						break;
				}

				inputs.Add(input);
			}

			EditWindow w = new EditWindow(inputs) { Title = "Add " + type.Name };
			w.ShowDialog();
			Dictionary<string, string> fields = w.Result;

			if(fields == null)
				return;

			Instance instance = new Instance(type);

			foreach(KeyValuePair<string, string> kvp in fields)
				instance.SetProperty(kvp.Key, kvp.Value);
			
			NodeModel model;

			if(!classToModel.TryGetValue(type.Name, out model))
				return;

			string mrid = instance.GetProperty("mRID");

			if(string.IsNullOrWhiteSpace(mrid))
				return;

			if(nodes.ContainsKey(mrid))
				return;

			Node node = new Node(center.X, center.Y, 0, 1, model, instance);

			nodes.Add(mrid, node);
			++typeCounts[type];

			foreach(string target in node.OutReferences)
			{
				Node targetNode;

				if(!nodes.TryGetValue(target, out targetNode))
					continue;

				targetNode.AddInReference(mrid);
			}

			foreach(Node n in selectedNodes)
				n.Selected = false;

			selectedNodes.Clear();

			node.Selected = true;
			selectedNodes.Add(node);

			Redraw();
		}

		private List<Node> HitTest(Point point)
		{
			Point globalPoint = new Point((point.X / canvasSize.X) * canvasPos.Width + canvasPos.Left, (point.Y / canvasSize.Y) * canvasPos.Height + canvasPos.Top);
			List<Node> ns = new List<Node>();

			foreach(KeyValuePair<string, Node> kvp in nodes)
			{
				Node n = kvp.Value;

				if(n.AABB.Contains(globalPoint))
					ns.Add(n);
			}

			return ns;
		}

		private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(!loaded)
				return;

			List<Node> ns = HitTest(e.GetPosition(canvas));

			if(ns.Count <= 0)
				return;

			if(ns.Count == 1)
			{
				Node n = ns[0];

				if(n.Selected = !n.Selected)
				{
					selectedNodes.Add(n);
				}
				else
				{
					selectedNodes.Remove(n);
				}

				Redraw();
				return;
			}

			List<Tuple<bool, string>> param = new List<Tuple<bool, string>>(ns.Count);

			foreach(Node n in ns)
				param.Add(new Tuple<bool, string>(n.Selected, n.Instance.GetProperty("mRID") + " : " + n.Instance.Class.Name));

			SelectWindow w = new SelectWindow(param) { Title = "Select" };
			w.ShowDialog();

			List<bool> result = w.Result;

			if(result == null)
				return;

			bool change = false;

			for(int i = 0; i < ns.Count; ++i)
			{
				Node n = ns[i];

				if(result[i])
				{
					if(!n.Selected)
					{
						n.Selected = true;
						selectedNodes.Add(n);
						change = true;
					}
				}
				else
				{
					if(n.Selected)
					{
						n.Selected = false;
						selectedNodes.Remove(n);
						change = true;
					}
				}
			}

			if(change)
				Redraw();
		}

		private void Deselect()
		{
			if(selectedNodes.Count <= 0)
				return;

			List<Tuple<bool, string>> param = new List<Tuple<bool, string>>(selectedNodes.Count);

			foreach(Node n in selectedNodes)
				param.Add(new Tuple<bool, string>(true, n.Instance.GetProperty("mRID") + " : " + n.Instance.Class.Name));

			SelectWindow w = new SelectWindow(param) { Title = "Deselect" };
			w.ShowDialog();
			List<bool> result = w.Result;

			if(result == null)
				return;

			bool change = false;

			int i = 0;
			foreach(bool keep in result)
			{
				if(keep)
				{
					++i;
					continue;
				}

				selectedNodes[i].Selected = false;
				selectedNodes.RemoveAt(i);
				change = true;
			}

			if(change)
				Redraw();
		}
	}

	public class Input
	{
		public string Name;
		public string Value;
		public List<string> OfferedValues;
	}

	class FileProxy
	{
		FileStream fs;

		public FileProxy(FileStream fs)
		{
			this.fs = fs;
		}

		public void WriteBytes(byte[] bs)
		{
			fs.Write(bs, 0, bs.Length);
		}

		public byte[] ReadBytes(int count)
		{
			byte[] bs = new byte[count];
			fs.Read(bs, 0, count);
			return bs;
		}

		public void WriteBool(bool b)
		{
			WriteBytes(BitConverter.GetBytes(b));
		}

		public bool ReadBool()
		{
			byte[] bs = ReadBytes(1);
			return BitConverter.ToBoolean(bs, 0);
		}

		public void WriteString(string s)
		{
			byte[] bs = Encoding.UTF8.GetBytes(s);
			WriteInt(bs.Length);
			WriteBytes(bs);
		}

		public string ReadString()
		{
			int len = ReadInt();
			byte[] bs = ReadBytes(len);
			return Encoding.UTF8.GetString(bs);
		}

		public void WriteDouble(double f)
		{
			WriteBytes(BitConverter.GetBytes(f));
		}

		public double ReadDouble()
		{
			byte[] bs = ReadBytes(8);
			return BitConverter.ToDouble(bs, 0);
		}

		public void WriteInt(int n)
		{
			WriteBytes(BitConverter.GetBytes(n));
		}

		public int ReadInt()
		{
			byte[] bs = ReadBytes(4);
			return BitConverter.ToInt32(bs, 0);
		}
	}
}
