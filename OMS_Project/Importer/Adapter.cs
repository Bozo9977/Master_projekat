﻿using CIM.Model;
using CIMParser;
using Common.GDA;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Importer
{
	public class Adapter
	{
		const string profileDLLName = ".\\OutageCIMProfile_Labs.dll";
		
		public Delta CreateDelta(Stream stream)
		{
			Assembly assembly;
			ConcreteModel concreteModel;

			if(!LoadModelFromFile(stream, out assembly, out concreteModel))
				return null;

			return new OutageImporter().CreateNMSDelta(concreteModel);
		}

		bool LoadModelFromFile(Stream extract, out Assembly assembly, out ConcreteModel concreteModel)
		{
			System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			concreteModel = null;

			try
			{
				assembly = Assembly.LoadFrom(profileDLLName);
				CIMModel cimModel = new CIMModel();
				CIMModelLoaderResult modelLoadResult = CIMModelLoader.LoadCIMXMLModel(extract, "FTN", out cimModel);

				if(!modelLoadResult.Success)
					return false;

				concreteModel = new ConcreteModel();
				ConcreteModelBuilder builder = new ConcreteModelBuilder();
				ConcreteModelBuildingResult modelBuildResult = builder.GenerateModel(cimModel, assembly, "FTN", ref concreteModel);

				if(!modelBuildResult.Success)
					return false;
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = culture;
			}

			return true;
		}
	}
}
