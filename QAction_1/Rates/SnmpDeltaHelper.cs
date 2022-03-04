namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Scripting;

	public class SnmpDeltaHelper
	{
		public enum CalculationMethod { Fast = 1, Accurate = 2 };

		private readonly SLProtocol protocol;
		private readonly int groupId;

		private readonly CalculationMethod calculationMethod;

		private bool deltaLoaded;
		private TimeSpan delta;
		private readonly Dictionary<string, TimeSpan> deltaPerInstance = new Dictionary<string, TimeSpan>();

		public SnmpDeltaHelper(SLProtocol protocol, int groupId, int calculationMethodPid = -1)
		{
			if (groupId < 0)
			{
				throw new ArgumentException("The group ID must not be negative.", "groupId");
			}

			this.protocol = protocol;
			this.groupId = groupId;

			if (calculationMethodPid < 0)
			{
				// Default behavior which will be used in following situations:
				// - For rates based on standalone SNMP counters
				// - For rates based on SNMP column counters if no 'Fast vs Accurate' user configuration is required (then Fast will be used)
				calculationMethod = CalculationMethod.Fast;
			}
			else
			{
				// Used for rates based on SNMP columns when provided the choice between 'Fast vs Accurate' to the end-user.
				calculationMethod = (CalculationMethod)Convert.ToInt32(protocol.GetParameter(calculationMethodPid));
				if (calculationMethod != CalculationMethod.Accurate && calculationMethod != CalculationMethod.Fast)
				{
					throw new NotSupportedException("Unexpected SNMP Rate Calculation Method value '" + protocol.GetParameter(calculationMethodPid) + "' retrieved from Param with PID '" + calculationMethodPid + "'.");
				}
			}
		}

		public void UpdateCalculationMethod()
		{
			switch (calculationMethod)
			{
				case CalculationMethod.Fast:
					protocol.NotifyProtocol(/*NT_SET_BITRATE_DELTA_INDEX_TRACKING*/ 448, groupId, false);
					break;
				case CalculationMethod.Accurate:
					protocol.NotifyProtocol(/*NT_SET_BITRATE_DELTA_INDEX_TRACKING*/ 448, groupId, true);
					break;
				default:
					// Unknown calculation method, do nothing (already handled in constructor)
					break;
			}
		}

		internal TimeSpan? GetDelta(string rowKey)
		{
			if (!deltaLoaded)
			{
				LoadDelta();
			}

			// Based on SNMP standalone
			if (rowKey == null)
			{
				return delta;
			}

			// Based on SNMP column
			switch (calculationMethod)
			{
				case CalculationMethod.Fast:
					return delta;
				case CalculationMethod.Accurate:
					if (deltaPerInstance.ContainsKey(rowKey))
					{
						return deltaPerInstance[rowKey];
					}
					else
					{
						return delta;
					}
				default:
					return null;
			}
		}

		private void LoadDelta()
		{
			switch (calculationMethod)
			{
				case CalculationMethod.Fast:
					LoadFastDelta();
					break;
				case CalculationMethod.Accurate:
					LoadAccurateDeltaValues();
					break;
				default:
					// Unknown calculation method, do nothing (already handled in constructor)
					break;
			}

			deltaLoaded = true;
		}

		private void LoadFastDelta()
		{
			int deltaInMilliseconds = Convert.ToInt32(protocol.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, groupId, null));
			delta = TimeSpan.FromMilliseconds(deltaInMilliseconds);
			////protocol.Log("QA" + protocol.QActionID + "|LoadFastDelta|deltaInMilliseconds '" + deltaInMilliseconds + "' - delta '" + delta + "'", LogType.DebugInfo, LogLevel.NoLogging);
		}

		private void LoadAccurateDeltaValues()
		{
			object deltaRaw = protocol.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, groupId, "");
			switch (deltaRaw)
			{
				case int deltaInMilliseconds:
					// In case of timeout, a single delta is returned.
					delta = TimeSpan.FromMilliseconds(deltaInMilliseconds);
					////protocol.Log("QA" + protocol.QActionID + "|LoadAccurateDeltaValues|deltaInMilliseconds '" + deltaInMilliseconds + "' - delta '" + delta + "'", LogType.DebugInfo, LogLevel.NoLogging);

					foreach (var key in deltaPerInstance.Keys)
					{
						deltaPerInstance[key] = delta;
					}

					break;
				case object[] deltaValues:
					// In case of successful group execution, a delta per instance is returned.
					for (int i = 0; i < deltaValues.Length; i++)
					{
						if (!(deltaValues[i] is object[] deltaKeyAndValue) || deltaKeyAndValue.Length != 2)
						{
							protocol.Log("QA" + protocol.QActionID + "|LoadSnmpGroupExecutionAccurateDeltas|Unexpected format for deltaValues[" + i + "]", LogType.Error, LogLevel.NoLogging);
							continue;
						}

						string deltaKey = Convert.ToString(deltaKeyAndValue[0]);
						int deltaInMilliseconds = Convert.ToInt32(deltaKeyAndValue[1]);

						deltaPerInstance[deltaKey] = TimeSpan.FromMilliseconds(deltaInMilliseconds);
						////protocol.Log("QA" + protocol.QActionID + "|LoadAccurateDeltaValues|deltaKey '" + deltaKey + "' - deltaInMilliseconds '" + deltaInMilliseconds + "' - delta '" + deltaPerInstance[deltaKey] + "'", LogType.DebugInfo, LogLevel.NoLogging);
					}

					break;
				default:
					protocol.Log("QA" + protocol.QActionID + "|LoadSnmpGroupExecutionAccurateDeltas|Unexpected format returned by NT_GET_BITRATE_DELTA.", LogType.Error, LogLevel.NoLogging);
					break;
			}
		}
	}
}
