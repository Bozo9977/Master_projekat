using Common;
using Common.PubSub;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IObserver<ObservableMessage>
	{
		enum EKey : byte { Up, Down, Left, Right, In, Out, Count }
		uint keys;
		const byte repeatedKeysCount = 10;
		Action[] keyActions;
		byte keyCount;
		DispatcherTimer timer;
		Rect canvasPos;
		const double moveDelta = 0.01;
		const double zoomDelta = 1.05;
		Vector canvasSize;
		double aspectRatio;
		double zoom;
		bool loaded;
		bool drawn;

		NetworkModelDrawing drawing;
		PubSubClient client;
		Tuple<List<GraphicsElement>, List<GraphicsLine>> elements;

		public MainWindow()
		{
			InitializeComponent();

			Logger.Instance.Level = ELogLevel.INFO;

			keyActions = new Action[(byte)EKey.Count] { Up, Down, Left, Right, In, Out };
			timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(48) };
			timer.Tick += Timer_Tick;
			
			client = new PubSubClient();
			client.Subscribe(this);
			client.Connect();
			client.Download();

			elements = new Tuple<List<GraphicsElement>, List<GraphicsLine>>(new List<GraphicsElement>(), new List<GraphicsLine>());
			canvas.Focus();
		}

		private void InitView()
		{
			zoom = 0.08;
			canvasPos = new Rect(aspectRatio / zoom * -0.5, -0.5 / zoom, aspectRatio / zoom, 1 / zoom);
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
			drawn = false;
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
				case Key.W:
					return EKey.Up;
				case Key.S:
					return EKey.Down;
				case Key.A:
					return EKey.Left;
				case Key.D:
					return EKey.Right;
				case Key.LeftShift:
					return EKey.In;
				case Key.LeftCtrl:
					return EKey.Out;
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
				{
					timer.Start();
				}
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
			Redraw(true);
		}

		private void Redraw(bool force = false)
		{
			if(!force && drawn)
				return;

			if(drawing == null)
				return;

			Tuple<List<GraphicsElement>, List<GraphicsLine>> result = drawing.Draw();
			canvas.Children.Clear();

			TranslateTransform tt = new TranslateTransform(-canvasPos.Left, -canvasPos.Top);
			ScaleTransform st = new ScaleTransform(canvasSize.Y * zoom, canvasSize.Y * zoom);
			TransformGroup tg = new TransformGroup() { Children = new TransformCollection() { tt, st } };

			foreach(GraphicsLine line in result.Item2)
			{
				Rect aabb = line.AABB;

				if(!aabb.IntersectsWith(canvasPos))
					continue;

				Point p1 = tg.Transform(new Point(line.X1, line.Y1));
				Point p2 = tg.Transform(new Point(line.X2, line.Y2));

				Line l = new Line() { Stroke = Brushes.Black, StrokeThickness = line.Thickness, X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
				canvas.Children.Add(l);
			}

			foreach(GraphicsElement element in result.Item1)
			{
				Rect aabb = element.AABB;

				if(!aabb.IntersectsWith(canvasPos))
					continue;

				Shape[] shapes = element.Draw();

				foreach(Shape shape in shapes)
				{
					TransformCollection tc = ((TransformGroup)shape.RenderTransform).Children;
					tc.Add(tt);
					tc.Add(st);
					canvas.Children.Add(shape);
				}
			}

			elements = result;
			drawn = true;
		}

		private void canvas_Loaded(object sender, RoutedEventArgs e)
		{
			loaded = true;
			canvasSize = new Vector(canvas.ActualWidth, canvas.ActualHeight);
			aspectRatio = canvasSize.X / canvasSize.Y;
			InitView();
			Redraw();
		}

		private void menuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Exit();
			base.OnClosing(e);
		}

		void Exit()
		{
			client.Disconnect();
		}

		public void Notify(ObservableMessage message)
		{
			switch(message.Type)
			{
				case EObservableMessageType.NetworkModelChanged:
					Dispatcher.BeginInvoke(new Action(UpdateModel));
					break;
			}
		}

		private void menuItemRefresh_Click(object sender, RoutedEventArgs e)
		{
			client.Download();
		}

		void UpdateModel()
		{
			drawing = new NetworkModelDrawing(client.Model);
			Redraw(true);
			Logger.Instance.Log(ELogLevel.INFO, "Model updated.");
		}

		const double DX = 1;
		const double DY = 1;

		private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point mousePos = e.GetPosition(canvas);
			Point globalPoint = new Point((mousePos.X / canvasSize.X) * canvasPos.Width + canvasPos.Left, (mousePos.Y / canvasSize.Y) * canvasPos.Height + canvasPos.Top);

			List<GraphicsElement> selected = new List<GraphicsElement>();

			double x = globalPoint.X - DX / 2;
			double y = globalPoint.Y - DY / 2;

			foreach(GraphicsElement element in elements.Item1)
			{
				double dx = element.X - x;
				double dy = element.Y - y;

				if(dx >= 0 && dx < DX && dy >= 0 && dy < DY)
				{
					selected.Add(element);
				}
			}

			if(selected.Count != 1)
			{
				return;
			}

			new ElementWindow(selected[0].Element.IO.GID, client).ShowDialog();
		}
	}
}
