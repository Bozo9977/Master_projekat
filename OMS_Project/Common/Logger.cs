using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public enum ELogLevel { INFO, WARNING, ERROR, FATAL }

	public class Logger
	{
		static readonly object l = new object();
		static Logger instance;
		ELogLevel level;
		StreamWriter sw;

		public static Logger Instance
		{
			get
			{
				if(instance == null)
				{
					lock(l)
					{
						if(instance == null)
						{
							instance = new Logger();
						}
					}
				}

				return instance;
			}
		}

		Logger()
		{
			level = ELogLevel.WARNING;

			try
			{
				FileStream fs = File.Open("Log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", FileMode.Append, FileAccess.Write, FileShare.Read);
				sw = new StreamWriter(fs) { AutoFlush = true };
			}
			catch(Exception e)
			{ }
		}

		public ELogLevel Level
		{
			get
			{
				return level;
			}
			set
			{
				level = value;
			}
		}

		public void Log(ELogLevel level, string message)
		{
			if(level < this.level || sw == null)
			{
				return;
			}

			sw.WriteLine(level + ": " + message);
		}
	}
}
