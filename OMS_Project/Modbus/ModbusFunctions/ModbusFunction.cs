using SCADA_Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
	public abstract class ModbusFunction : IModbusFunction
	{
		private ModbusCommandParameters commandParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="ModbusFunction"/> class.
		/// </summary>
		/// <param name="commandParameters">The modbus command parameters.</param>
		public ModbusFunction(ModbusCommandParameters commandParameters)
		{
			this.commandParameters = commandParameters;
		}
		/// <summary>
		/// Gets or sets the command parameters.
		/// </summary>
		public ModbusCommandParameters CommandParameters
		{
			get
			{
				return commandParameters;
			}

			set
			{
				commandParameters = value;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Transaction: {commandParameters.TransactionId}, command {commandParameters.FunctionCode}";
		}

		/// <summary>
		/// Checks if the command parameters are valid.
		/// </summary>
		/// <param name="m">The methodbase.</param>
		/// <param name="t">Type of the command parameters</param>
		protected void CheckArguments(MethodBase m, Type t)
		{
			if (commandParameters.GetType() != t)
			{
				string message = $"{m.ReflectedType.Name}{m.Name} has invalid argument {nameof(commandParameters)} of type {commandParameters.GetType().Name}.{Environment.NewLine}Argumet type should be {t.Name}";
				throw new ArgumentException(message);
			}
		}

		/// <summary>
		/// Converts command parameters to byte array.
		/// </summary>
		/// <returns>Command parameters in form of byte array</returns>
		public abstract byte[] PackRequest();

		/// <summary>
		/// Converts received message to key-value pairs.
		/// <param name="response">Message read form socket</param>
		/// <returns>
		///		Dictionary that maps tuple to received value from MdbSim:
		///		Key: Tuple<PointType, ushort> - complex key of point. Points unique identifier
		///				- PointType - type of point
		///				- Point address
		///		Value: Value received from MdbSim
		/// </returns>
		public abstract Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response);

		/// <summary>
		/// Handles the exception
		/// </summary>
		/// <param name="exceptionCode">The exception code.</param>
		protected void HandeException(byte exceptionCode)
		{
			string message = string.Empty;

			switch (exceptionCode)
			{
				case 1:
					message = "Illegal Function";
					break;
				case 2:
					message = "Illegal Data Address";
					break;
				case 3:
					message = "Illegal Data Value";
					break;
				case 4:
					message = "Slave Device Failure";
					break;
				case 5:
					message = "Acknowledge";
					break;
				case 6:
					message = "Slave Device Busy";
					break;
				case 7:
					message = "Negative Acknowledge";
					break;
				case 8:
					message = "Memory Parity Error";
					break;
				case 10:
					message = "Illegal Function";
					break;
				case 11:
					message = "Gateway Target Device Failed to Respond";
					break;
				default: break;
			}

			throw new Exception(message);
		}
	}
}
