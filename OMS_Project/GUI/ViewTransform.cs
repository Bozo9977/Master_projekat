using System.Windows.Media;

namespace GUI
{
	public class ViewTransform
	{
		public Transform Transform { get; private set; }

		public ViewTransform(double x, double y, double zoom)
		{
			TranslateTransform tt = new TranslateTransform(-x, -y);
			ScaleTransform st = new ScaleTransform(zoom, zoom);
			Transform = new TransformGroup() { Children = new TransformCollection() { tt, st } };
		}
	}
}
