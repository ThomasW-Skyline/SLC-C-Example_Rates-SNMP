namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;
	using Skyline.Protocol.Rates;
	using Skyline.Protocol.SafeConverters;

	public class StreamsTimeoutProcessor
	{
		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		private readonly SLProtocol protocol;
		private const int groupId = 1000;

		internal StreamsTimeoutProcessor(SLProtocol protocol)
		{
			this.protocol = protocol;

			getter = new StreamsGetter(protocol);
			getter.Load();

			setter = new StreamsSetter(protocol);
		}

		internal void ProcessTimeout()
		{
			for (int i = 0; i < getter.Keys.Length; i++)
			{
				string serializedHelper = Convert.ToString(getter.OctetsRateData[i]);
				SnmpRate32 snmpRate32Helper = SnmpRate32.FromJsonString(protocol, serializedHelper, groupId, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));

				snmpRate32Helper.BufferDelta();

				setter.SetColumnsData[Parameter.Streams.tablePid].Add(Convert.ToString(getter.Keys[i]));
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
				var tableData = (object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, new uint[]
				{
					Parameter.Streams.Idx.streamsindex,
					Parameter.Streams.Idx.streamsbitratedata,
				});

				Keys = (object[])tableData[0];
				OctetsRateData = (object[])tableData[1];
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
