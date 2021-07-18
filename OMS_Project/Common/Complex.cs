using System;

namespace Common
{
	public struct Complex
	{
		public double X { get; private set; }
		public double Y { get; private set; }

		public Complex(double x, double y)
		{
			X = x;
			Y = y;
		}

		public bool IsNaN()
		{
			return double.IsNaN(X) || double.IsNaN(Y);
		}

		public Complex Add(Complex other)
		{
			return new Complex(X + other.X, Y + other.Y);
		}

		public static Complex NaN()
		{
			return new Complex(double.NaN, double.NaN);
		}

		public Complex Divide(Complex other)
		{
			double divisor = 1 / (other.X * other.X + other.Y * other.Y);

			if(double.IsInfinity(divisor))
				return new Complex((X < 0 ? -1 : 1) * divisor, (Y < 0 ? -1 : 1) * divisor);

			double x = X * other.X + Y * other.Y;
			double y = Y * other.X - X * other.Y;

			return new Complex(x * divisor, y * divisor);
		}

		public Complex Scale(double c)
		{
			return new Complex(c * X, c * Y);
		}

		public Complex Multiply(Complex other)
		{
			return new Complex(X * other.X - Y * other.Y, X * other.Y + Y * other.X);
		}

		public Complex Subtract(Complex other)
		{
			return new Complex(X - other.X, Y - other.Y);
		}

		public double Magnitude()
		{
			return Math.Sqrt(X * X + Y * Y);
		}
	}
}
