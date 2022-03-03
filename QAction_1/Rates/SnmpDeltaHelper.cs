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

		public SnmpDeltaHelper(SLProtocol protocol, int groupId, int calculationMethodPid)
		{
			if (groupId < 0)
			{
				throw new ArgumentException("The group ID must not be negative.", "groupId");
			}

			this.protocol = protocol;
			this.groupId = groupId;

			calculationMethod = (CalculationMethod)Convert.ToInt32(protocol.GetParameter(calculationMethodPid));
			if (calculationMethod != CalculationMethod.Accurate && calculationMethod != CalculationMethod.Fast)
			{
				throw new NotSupportedException("Unexpected SNMP Rate Calculation Method value '" + protocol.GetParameter(calculationMethodPid) + "' retrieved from Param with PID '" + calculationMethodPid + "'.");
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

		internal TimeSpan? GetDelta(string key)
		{
			if (!deltaLoaded)
			{
				LoadDelta();
			}

			switch (calculationMethod)
			{
				case CalculationMethod.Fast:
					return delta;
				case CalculationMethod.Accurate:
					return deltaPerInstance[key];
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
		}

		private void LoadAccurateDeltaValues()
		{
			if (!(protocol.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, groupId, "") is object[] deltaValues))
			{
				protocol.Log("QA" + protocol.QActionID + "|LoadSnmpGroupExecutionAccurateDeltas|Unexpected format returned by NT_GET_BITRATE_DELTA.", LogType.Error, LogLevel.NoLogging);
				return;
			}

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
			}
		}
	}
}
