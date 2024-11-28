namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.Protocol.Extension;
	using Skyline.DataMiner.Utils.Rates.Protocol;
	using Skyline.DataMiner.Utils.SNMP;

	public class StreamsTimeoutProcessor
	{
		private const int GroupId = 1000;

		private readonly SLProtocol protocol;
		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		internal StreamsTimeoutProcessor(SLProtocol protocol)
		{
			this.protocol = protocol;

			getter = new StreamsGetter(protocol);
			getter.Load();

			setter = new StreamsSetter(protocol);
		}

		internal void ProcessTimeout()
		{
			SnmpDeltaHelper snmpDeltaHelper = new SnmpDeltaHelper(protocol, GroupId, Parameter.streamsratecalculationsmethod);

			for (int i = 0; i < getter.Keys.Length; i++)
			{
				string streamPK = Convert.ToString(getter.Keys[i]);
				string serializedHelper = Convert.ToString(getter.OctetsRateData[i]);

				SnmpRate32 snmpRate32Helper = SnmpRate32.FromJsonString(serializedHelper, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
				snmpRate32Helper.BufferDelta(snmpDeltaHelper, streamPK);

				setter.SetColumnsData[Parameter.Streams.tablePid].Add(streamPK);
				setter.SetColumnsData[Parameter.Streams.Pid.streamsbitratedata].Add(snmpRate32Helper.ToJsonString());
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetColumns();
		}

		private class StreamsGetter
		{
			private readonly SLProtocol protocol;

			internal StreamsGetter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			public object[] Keys { get; private set; }

			public object[] OctetsRateData { get; private set; }

			internal void Load()
			{
				uint[] columnsToGet = new uint[]
				{
					Parameter.Streams.Idx.streamsindex,
					Parameter.Streams.Idx.streamsbitratedata,
				};

				var tableData = protocol.GetColumns(Parameter.Streams.tablePid, columnsToGet);

				Keys = (object[])tableData[0];
				OctetsRateData = (object[])tableData[1];
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
				{ Parameter.Streams.Pid.streamsbitratedata, new List<object>() },
			};

			internal void SetColumns()
			{
				protocol.SetColumns(SetColumnsData);
			}
		}
	}
}
