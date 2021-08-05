using System.Windows.Media;

namespace GUI
{
	public class ViewTransform
	{
		public double X { get; private set; }
		public double Y { get; private set; }
		public double Zoom { get; private set; }
		public Transform Transform { get; private set; }

		public ViewTransform(double x, double y, double zoom)
		{
			X = x;
			Y = y;
			Zoom = zoom;
			TranslateTransform tt = new TranslateTransform(-x, -y);
			ScaleTransform st = new ScaleTransform(zoom, zoom);
			Transform = new TransformGroup() { Children = new TransformCollection() { tt, st } };
		}
	}
}
