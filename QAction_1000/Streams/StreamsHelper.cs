namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;
	using Skyline.Protocol.Rates;
	using Skyline.Protocol.SafeConverters;

	public class StreamsHelper
	{
		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		private readonly SLProtocol protocol;
		private const int groupId = 1000;

		internal StreamsHelper(SLProtocol protocol)
		{
			this.protocol = protocol;

			getter = new StreamsGetter(protocol);
			getter.Load();

			setter = new StreamsSetter(protocol);
		}

		internal void ProcessData()
		{
			for (int i = 0; i < getter.Keys.Length; i++)
			{
				setter.SetColumnsData[Parameter.Streams.tablePid].Add(Convert.ToString(getter.Keys[i]));

				ProcessBitRates(i, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetColumns();
		}

		private void ProcessBitRates(int getPosition, TimeSpan minDelta, TimeSpan maxDelta)
		{
			uint octets = SafeConvert.ToUInt32(Convert.ToDouble(getter.Octets[getPosition]));
			uint bytes = octets / 8;
			protocol.Log("QA" + protocol.QActionID + "|ProcessBitRates|getPosition '" + getPosition + "' - octets '" + octets + "' - bytes '" + bytes + "'", LogType.DebugInfo, LogLevel.NoLogging);

			SnmpRate32 snmpRate32Helper = SnmpRate32.FromJsonString(protocol, Convert.ToString(getter.OctetsRateData[getPosition]), groupId, minDelta, maxDelta);
			protocol.Log("QA" + protocol.QActionID + "|ProcessBitRates|Helper instance created", LogType.DebugInfo, LogLevel.NoLogging);

			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrate].Add(snmpRate32Helper.Calculate(bytes));
			protocol.Log("QA" + protocol.QActionID + "|ProcessBitRates|Calculation done", LogType.DebugInfo, LogLevel.NoLogging);

			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitratedata].Add(snmpRate32Helper.ToJsonString());
			protocol.Log("QA" + protocol.QActionID + "|ProcessBitRates|Serialization done", LogType.DebugInfo, LogLevel.NoLogging);
		}

		private class StreamsGetter
		{
			private readonly SLProtocol protocol;

			internal StreamsGetter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			public object[] Keys { get; private set; }

			public object[] Octets { get; private set; }

			public object[] OctetsRateData { get; private set; }

			internal void Load()
			{
				var tableData = (object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, new uint[]
				{
					Parameter.Streams.Idx.streamsindex,
					Parameter.Streams.Idx.streamsoctetscounter,
					Parameter.Streams.Idx.streamsbitratedata,
				});

				Keys = (object[])tableData[0];
				Octets = (object[])tableData[1];
				OctetsRateData = (object[])tableData[2];
			}
		}

		private class StreamsSetter
		{
			internal readonly Dictionary<object, List<object>> SetColumnsData;

			private readonly SLProtocol protocol;

			internal StreamsSetter(SLProtocol protocol)
			{
				this.protocol = protocol;

				SetColumnsData = new Dictionary<object, List<object>>
				{
					{ Parameter.Streams.tablePid, new List<object>() },
					{ Parameter.Streams.Pid.streamsbitrate, new List<object>() },
					{ Parameter.Streams.Pid.streamsbitratedata, new List<object>() },
				};
			}

			internal void SetColumns()
			{
				protocol.SetColumns(SetColumnsData);
			}
		}
	}
}
