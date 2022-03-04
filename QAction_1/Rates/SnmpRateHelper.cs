namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Scripting;

	public class SnmpRate<T, U> where T : CounterWithTime<U>
	{
		[JsonProperty]
		protected TimeSpan bufferedDelta;

		[JsonProperty]
		protected RateOnTimes<T, U> rateOnTimes;

		[JsonConstructor]
		private protected SnmpRate()
		{
			bufferedDelta = TimeSpan.Zero;
		}

		public double Calculate(SnmpDeltaHelper deltaHelper, T newCounter, string rowKey = null, double faultyReturn = -1)
		{
			var delta = deltaHelper.GetDelta(rowKey);
			if (!delta.HasValue)
			{
				return faultyReturn;
			}

			newCounter.TimeSpan = bufferedDelta + delta.Value;
			var rate = rateOnTimes.Calculate(newCounter, faultyReturn);
			bufferedDelta = TimeSpan.Zero;

			return rate;
		}

		public void BufferDelta(SnmpDeltaHelper deltaHelper, string rowKey = null)
		{
			var delta = deltaHelper.GetDelta(rowKey);
			if (!delta.HasValue)
			{
				return;
			}

			bufferedDelta += delta.Value;
		}

		public string ToJsonString()
		{
			//var settings = new JsonSerializerSettings
			//{
			//	TypeNameHandling = TypeNameHandling.All
			//};

			return JsonConvert.SerializeObject(this/*, settings*/);
		}
	}

	public class SnmpRate32 : SnmpRate<Counter32WithTime, uint>
	{
		[JsonConstructor]
		public SnmpRate32(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
		{
			rateOnTimes = new Rate32OnTimes(minDelta, maxDelta, rateBase);
		}

		public double Calculate(SnmpDeltaHelper deltaHelper, uint newCounter, string rowKey = null, double faultyReturn = -1)
		{
			var rateCounter = new Counter32WithTime(newCounter, TimeSpan.Zero);
			return Calculate(deltaHelper, rateCounter, rowKey, faultyReturn);
		}

		public static SnmpRate32 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			Rate32OnTimes.ValidateMinAndMaxDeltas(minDelta, maxDelta);

			//var settings = new JsonSerializerSettings
			//{
			//	TypeNameHandling = TypeNameHandling.All
			//};

			var instance = !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<SnmpRate32>(rateHelperSerialized/*, settings*/) :
				new SnmpRate32(minDelta, maxDelta, rateBase);

			return instance;
		}
	}

	public class SnmpRate64 : SnmpRate<Counter64WithTime, ulong>
	{
		[JsonConstructor]
		public SnmpRate64(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
		{
			rateOnTimes = new Rate64OnTimes(minDelta, maxDelta, rateBase);
		}

		public double Calculate(SnmpDeltaHelper deltaHelper, ulong newCounter, string rowKey = null, double faultyReturn = -1)
		{
			var rateCounter = new Counter64WithTime(newCounter, TimeSpan.Zero);
			return Calculate(deltaHelper, rateCounter, rowKey, faultyReturn);
		}

		public static SnmpRate64 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase = RateBase.Second)
		{
			Rate64OnTimes.ValidateMinAndMaxDeltas(minDelta, maxDelta);

			//var settings = new JsonSerializerSettings
			//{
			//	TypeNameHandling = TypeNameHandling.All
			//};

			var instance = !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<SnmpRate64>(rateHelperSerialized/*, settings*/) :
				new SnmpRate64(minDelta, maxDelta, rateBase);

			return instance;
		}
	}
}
