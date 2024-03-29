﻿using Common;
using Common.GDA;
using GUI.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, Common.IObserver<ObservableMessage>
	{
		enum EKey : byte { Up, Down, Left, Right, In, Out, Count }
		uint keys;
		const byte repeatedKeysCount = 6;
		Action[] keyActions;
		byte keyCount;
		DispatcherTimer timer;
		Rect canvasPos;
		const double moveDelta = 0.02;
		const double zoomDelta = 1.08;
		Vector canvasSize;
		double aspectRatio;
		double zoom;
		bool loaded;
		bool shouldRedraw;

		NetworkModelDrawing drawing;
		PubSubClient pubSub;
		Sequence<IGraphicsElement> elements;

		public MainWindow()
		{
			InitializeComponent();

			Logger.Instance.Level = ELogLevel.INFO;

			keyActions = new Action[(byte)EKey.Count] { Up, Down, Left, Right, In, Out };
			timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(40) };
			timer.Tick += Timer_Tick;
			
			pubSub = new PubSubClient();
			drawing = new NetworkModelDrawing() { NetworkModel = pubSub.Model, Topology = pubSub.Topology, Measurements = pubSub.Measurements };
			pubSub.Subscribe(this);
			pubSub.Reconnect();
			pubSub.Download();

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
			byte i = 0;
			uint k = keys;

			while(k != 0 && i < repeatedKeysCount)
			{
				if((k & 1u) != 0)
					keyActions[i]();

				k >>= 1;
				++i;
			}

			if(shouldRedraw)
			{
				Redraw();
				shouldRedraw = false;
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
			shouldRedraw = true;
		}

		void Down()
		{
			canvasPos = new Rect(canvasPos.Left, canvasPos.Top + moveDelta / zoom, canvasPos.Width, canvasPos.Height);
			shouldRedraw = true;
		}

		void Left()
		{
			canvasPos = new Rect(canvasPos.Left - moveDelta / zoom, canvasPos.Top, canvasPos.Width, canvasPos.Height);
			shouldRedraw = true;
		}

		void Right()
		{
			canvasPos = new Rect(canvasPos.Left + moveDelta / zoom, canvasPos.Top, canvasPos.Width, canvasPos.Height);
			shouldRedraw = true;
		}

		void In()
		{
			zoom *= zoomDelta;
			Vector center = GetCenter();
			Vector newSize = new Vector(aspectRatio / zoom, 1.0 / zoom);
			canvasPos = new Rect(center.X - newSize.X * 0.5, center.Y - newSize.Y * 0.5, newSize.X, newSize.Y);
			shouldRedraw = true;
		}

		void Out()
		{
			zoom /= zoomDelta;
			Vector center = GetCenter();
			Vector newSize = new Vector(aspectRatio / zoom, 1.0 / zoom);
			canvasPos = new Rect(center.X - newSize.X * 0.5, center.Y - newSize.Y * 0.5, newSize.X, newSize.Y);
			shouldRedraw = true;
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
			Sequence<IGraphicsElement> elements = drawing.Draw();

			if(elements == null)
				return;

			canvas.Children.Clear();
			ViewTransform vt = new ViewTransform(canvasPos.Left, canvasPos.Top, canvasSize.Y * zoom);
			IGraphicsElement element;

			elements.Reset();
			while(elements.Next(out element))
			{
				if(element.MinZoom > zoom || !element.AABB.IntersectsWith(canvasPos))
					continue;

				UIElement[] shapes = element.Draw(vt);

				for(int j = 0; j < shapes.Length; ++j)
				{
					canvas.Children.Add(shapes[j]);
				}
			}

			this.elements = elements;
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
			pubSub.Disconnect();
		}

		public void Notify(ObservableMessage message)
		{
			switch(message.Type)
			{
				case EObservableMessageType.NetworkModelChanged:
					Dispatcher.BeginInvoke(new Action(UpdateModel));
					break;

				case EObservableMessageType.TopologyChanged:
					Dispatcher.BeginInvoke(new Action(UpdateTopology));
					break;

				case EObservableMessageType.SwitchStatusChanged:
					Dispatcher.BeginInvoke(new Action(UpdateTopology));
					break;

				case EObservableMessageType.LoadFlowChanged:
					Dispatcher.BeginInvoke(new Action(UpdateLoadFlow));
					break;
			}
		}

		private void menuItemRefresh_Click(object sender, RoutedEventArgs e)
		{
			pubSub.Reconnect();
			pubSub.Download();
		}

		void UpdateModel()
		{
			drawing.NetworkModel = pubSub.Model;
			Redraw();
			Logger.Instance.Log(ELogLevel.INFO, "Model updated.");
		}

		void UpdateTopology()
		{
			drawing.Topology = pubSub.Topology;
			Redraw();
			Logger.Instance.Log(ELogLevel.INFO, "Topology updated.");
		}

		void UpdateLoadFlow()
		{
			drawing.UpdateLoadFlow();
			Redraw();
			Logger.Instance.Log(ELogLevel.INFO, "Load flow updated.");
		}

		const double DX = 1;
		const double DY = 1;

		private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point mousePos = e.GetPosition(canvas);
			Point globalPoint = new Point((mousePos.X / canvasSize.X) * canvasPos.Width + canvasPos.Left, (mousePos.Y / canvasSize.Y) * canvasPos.Height + canvasPos.Top);

			IGraphicsElement selected = null;

			double x = globalPoint.X - DX / 2;
			double y = globalPoint.Y - DY / 2;
			IGraphicsElement element;
			elements.Reset();

			while(elements.Next(out element))
			{
				if(element.IO == null)
					continue;

				Rect aabb = element.AABB;
				Point center = new Point(aabb.Left + aabb.Width / 2, aabb.Top + aabb.Height / 2);
				double dx = center.X - x;
				double dy = center.Y - y;

				if(dx >= 0 && dx < DX && dy >= 0 && dy < DY)
				{
					selected = element;
					break;
				}
			}

			if(selected == null)
				return;

			new ElementWindow(selected.IO.GID, pubSub).Show();
		}

		private void menuItemSwitchingSchedules_Click(object sender, RoutedEventArgs e)
		{
			new ElementsWindow(DMSType.SwitchingSchedule, pubSub).Show();
		}
	}
}
