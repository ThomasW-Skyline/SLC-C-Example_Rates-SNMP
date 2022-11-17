namespace Skyline.Protocol.Counter
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Library.Common.SafeConverters;
	using Skyline.DataMiner.Library.Protocol.Snmp.Rates;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;

	public class CounterProcessor
	{
		private const int GroupId = 500;
		private readonly SLProtocol protocol;

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
			SnmpDeltaHelper snmpDeltaHelper = new SnmpDeltaHelper(protocol, GroupId);

			SnmpRate32 snmpRateHelper;
			if (getter.IsSnmpAgentRestarted)
			{
				setter.SetParamsData[Parameter.countersnmpagentrestartflag] = 0;

				snmpRateHelper = SnmpRate32.FromJsonString(String.Empty, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
			}
			else
			{
				snmpRateHelper = SnmpRate32.FromJsonString(getter.CounterRateData, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
			}

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

			public bool IsSnmpAgentRestarted { get; private set; }

			internal void Load()
			{
				var counterData = (object[])protocol.GetParameters(new uint[]
				{
					Parameter.counter,
					Parameter.counterratedata,
					Parameter.countersnmpagentrestartflag,
				});

				Counter = SafeConvert.ToUInt32(Convert.ToDouble(counterData[0]));
				CounterRateData = Convert.ToString(counterData[1]);
				IsSnmpAgentRestarted = Convert.ToBoolean(Convert.ToInt16(counterData[2]));
			}
		}

		private class Setter
		{
			private readonly SLProtocol protocol;

			internal Setter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			internal Dictionary<int, object> SetParamsData { get; } = new Dictionary<int, object>();

			internal void SetParams()
			{
				protocol.SetParams(SetParamsData);
			}
		}
	}
}
