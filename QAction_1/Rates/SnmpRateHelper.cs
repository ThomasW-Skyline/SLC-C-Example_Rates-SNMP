namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Scripting;

	//public class SnmpRate<T, U, V> where T : RateOnTimes<U, V>
	//{
	//	[JsonProperty]
	//	protected readonly int groupId;

	//	[JsonProperty]
	//	protected readonly RateOnTimes<U, V> rateOnTimes;

	//	[JsonConstructor]
	//	private SnmpRate(int groupId, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
	//	{
	//		this.groupId = groupId;
	//		this.rateOnTimes = new T(minDelta, maxDelta, rateBase);
	//	}

	//	public static SnmpRate FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, int groupId, RateBase rateBase = RateBase.Second)
	//	{
	//		// TODO: Rate32OnTimes.ValidateMinAndMaxDeltas(minDelta, maxDelta);

	//		return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
	//			JsonConvert.DeserializeObject<SnmpRate>(rateHelperSerialized) :
	//			new SnmpRate(groupId, minDelta, maxDelta, rateBase);
	//	}
	//}

	//public class SnmpRate32 : SnmpRate<SnmpRate32, Rate32OnTimes, uint>
	//{
	//	[JsonProperty]
	//	protected readonly int groupId;

	//	[JsonConstructor]
	//	private SnmpRate32(int groupId, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase) : base(groupId, minDelta, maxDelta, rateBase) { } 

	//	public static SnmpRate32 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, int groupId, RateBase rateBase = RateBase.Second)
	//	{
	//		// TODO: Rate32OnTimes.ValidateMinAndMaxDeltas(minDelta, maxDelta);

	//		return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
	//			JsonConvert.DeserializeObject<SnmpRate32>(rateHelperSerialized) :
	//			new SnmpRate32(groupId, minDelta, maxDelta, rateBase);
	//	}
	//}

	public class SnmpRate32
	{
		protected SLProtocol protocol;

		[JsonProperty]
		protected readonly int groupId;

		[JsonProperty]
		protected TimeSpan bufferedDelta;

		[JsonProperty]
		protected readonly Rate32OnTimes rateOnTimes;

		[JsonConstructor]
		private SnmpRate32(int groupId, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
		{
			if (groupId < 0)
			{
				throw new ArgumentException("The group ID must not be negative.", "groupId");
			}

			this.groupId = groupId;

			rateOnTimes = new Rate32OnTimes(minDelta, maxDelta, rateBase);
			bufferedDelta = TimeSpan.Zero;
		}

		public double Calculate(uint newCounter, double faultyReturn = -1)
		{
			var delta = GetSnmpGroupExecutionDelta();
			if (!delta.HasValue)
			{
				return faultyReturn;
			}

			var rate = rateOnTimes.Calculate(newCounter, bufferedDelta + delta.Value, faultyReturn);
			bufferedDelta = TimeSpan.Zero;

			return rate;
		}

		public void BufferDelta()
		{
			var delta = GetSnmpGroupExecutionDelta();
			if (!delta.HasValue)
			{
				return;
			}

			bufferedDelta += delta.Value;
		}

		private TimeSpan? GetSnmpGroupExecutionDelta()
		{
			int result = Convert.ToInt32(protocol.NotifyProtocol(269 /*NT_GET_BITRATE_DELTA*/, groupId, null));
			protocol.Log("QA" + protocol.QActionID + "|GetSnmpGroupExecutionDelta|result '" + result + "'", LogType.DebugInfo, LogLevel.NoLogging);

			if (result != -1)
			{
				return TimeSpan.FromMilliseconds(result);
			}

			return null;
		}

		public static SnmpRate32 FromJsonString(SLProtocol protocol, string rateHelperSerialized, int groupId, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			// TODO: Rate32OnTimes.ValidateMinAndMaxDeltas(minDelta, maxDelta);

			var instance = !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<SnmpRate32>(rateHelperSerialized) :
				new SnmpRate32(groupId, minDelta, maxDelta, rateBase);

			instance.protocol = protocol;
			//instance.groupId = groupId;	// TODO: chose between including the groupId in serialized version or add this line of code here.
			return instance;
		}

		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
