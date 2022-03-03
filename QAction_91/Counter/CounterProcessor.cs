namespace Skyline.Protocol.Counter
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Rates;

	public class CounterProcessor
	{
		private readonly SLProtocol protocol;
		private const int groupId = 100;

		private readonly Getter getter;
		private readonly Setter setter;

		public CounterProcessor(SLProtocol protocol)
		{
			this.protocol = protocol;

			getter = new Getter(protocol);
			getter.Load();

			setter = new Setter(protocol);
		}

		internal void ProcessData()
		{
			SnmpDeltaHelper snmpDeltaHelper = new SnmpDeltaHelper(protocol, groupId);

			SnmpRate32 snmpRateHelper = SnmpRate32.FromJsonString(getter.CounterRateData, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
			double rate = snmpRateHelper.Calculate(snmpDeltaHelper, getter.Counter);

			setter.SetParamsData[Parameter.counterrate] = rate;
			setter.SetParamsData[Parameter.counterratedata] = snmpRateHelper.ToJsonString();
		}

		internal void UpdateProtocol()
		{
			setter.SetParams();
		}

		private class Getter
		{
			private readonly SLProtocol protocol;

			internal Getter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			public uint Counter { get; private set; }
			public string CounterRateData { get; private set; }

			internal void Load()
			{
				var counterData = (object[])protocol.GetParameters(new uint[]
				{
					Parameter.counter,
					Parameter.counterratedata,
				});

				Counter = Convert.ToUInt32(counterData[0]);
				CounterRateData = Convert.ToString(counterData[1]);
			}
		}

		private class Setter
		{
			private readonly SLProtocol protocol;

			internal readonly Dictionary<int, object> SetParamsData = new Dictionary<int, object>();

			internal Setter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			internal void SetParams()
			{
				protocol.SetParameters(SetParamsData.Keys.ToArray(), SetParamsData.Values.ToArray());
			}
		}
	}
}
