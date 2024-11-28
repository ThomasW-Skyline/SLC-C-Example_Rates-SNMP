namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.Protocol.Extension;
	using Skyline.DataMiner.Utils.Rates.Protocol;
	using Skyline.DataMiner.Utils.SafeConverters;
	using Skyline.DataMiner.Utils.SNMP;

	public class StreamsProcessor
	{
		private const int GroupId = 1000;
		private readonly SLProtocol protocol;

		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		internal StreamsProcessor(SLProtocol protocol)
		{
			this.protocol = protocol;

			getter = new StreamsGetter(protocol);
			getter.Load();

			setter = new StreamsSetter(protocol);
		}

		internal void ProcessData()
		{
			SnmpDeltaHelper snmpDeltaHelper = new SnmpDeltaHelper(protocol, GroupId, Parameter.streamsratecalculationsmethod);

			for (int i = 0; i < getter.Keys.Length; i++)
			{
				setter.SetColumnsData[Parameter.Streams.tablePid].Add(Convert.ToString(getter.Keys[i]));

				ProcessBitRates(i, snmpDeltaHelper, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetParams();
			setter.SetColumns();
		}

		private void ProcessBitRates(int getPosition, SnmpDeltaHelper snmpDeltaHelper, TimeSpan minDelta, TimeSpan maxDelta)
		{
			string streamPK = Convert.ToString(getter.Keys[getPosition]);
			uint octets = SafeConvert.ToUInt32(Convert.ToDouble(getter.Octets[getPosition]));

			SnmpRate32 snmpRate32Helper;
			if (getter.IsSnmpAgentRestarted)
			{
				setter.SetParamsData[Parameter.streamssnmpagentrestartflag] = 0;

				snmpRate32Helper = SnmpRate32.FromJsonString(String.Empty, minDelta, maxDelta);
			}
			else
			{
				string serializedHelper = Convert.ToString(getter.OctetsRateData[getPosition]);
				snmpRate32Helper = SnmpRate32.FromJsonString(serializedHelper, minDelta, maxDelta);
			}

			double octetRate = snmpRate32Helper.Calculate(snmpDeltaHelper, octets, streamPK);
			double bitRate = octetRate > 0 ? octetRate * 8 : octetRate;

			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrate].Add(bitRate);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitratedata].Add(snmpRate32Helper.ToJsonString());
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

			public bool IsSnmpAgentRestarted { get; private set; }

			internal void Load()
			{
				IsSnmpAgentRestarted = Convert.ToBoolean(Convert.ToInt16(protocol.GetParameter(Parameter.streamssnmpagentrestartflag)));

				List<uint> columnsToGet = new List<uint>
				{
					Parameter.Streams.Idx.streamsindex,
					Parameter.Streams.Idx.streamsoctetscounter,
				};

				if (!IsSnmpAgentRestarted)
				{
					columnsToGet.Add(Parameter.Streams.Idx.streamsbitratedata);
				}

				var tableData = protocol.GetColumns(Parameter.Streams.tablePid, columnsToGet.ToArray());

				Keys = (object[])tableData[0];
				Octets = (object[])tableData[1];

				if (!IsSnmpAgentRestarted)
				{
					OctetsRateData = (object[])tableData[2];
				}
			}
		}

		private class StreamsSetter
		{
			private readonly SLProtocol protocol;

			internal StreamsSetter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			internal Dictionary<int, List<object>> SetColumnsData { get; } = new Dictionary<int, List<object>>
			{
				{ Parameter.Streams.tablePid, new List<object>() },
				{ Parameter.Streams.Pid.streamsbitrate, new List<object>() },
				{ Parameter.Streams.Pid.streamsbitratedata, new List<object>() },
			};

			internal Dictionary<int, object> SetParamsData { get; } = new Dictionary<int, object>();

			internal void SetColumns()
			{
				protocol.SetColumns(SetColumnsData);
			}

			internal void SetParams()
			{
				protocol.SetParameters(SetParamsData);
			}
		}
	}
}
