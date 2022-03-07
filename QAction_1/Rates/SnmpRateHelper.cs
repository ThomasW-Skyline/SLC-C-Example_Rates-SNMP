namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Class <see cref="SnmpRate"/> helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters polled over SNMP.
	/// This class is meant to be used as base class for more specific SnmpRate helpers depending on the range of counters (<see cref="System.UInt32"/>, <see cref="System.UInt64"/>, etc).
	/// </summary>
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

		protected double Calculate(SnmpDeltaHelper deltaHelper, T newCounter, string rowKey = null, double faultyReturn = -1)
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

		/// <summary>
		/// Used to buffer the delta (TimeSpan) between 2 executions of the same group whenever the group execution times-out.
		/// </summary>
		/// <param name="deltaHelper">An instance of the <see cref="SnmpDeltaHelper"/> class which will take care of fetching the delta from DataMiner.</param>
		/// <param name="rowKey">Optional argument to be used when buffering delta for an SNMP table. In that case, the primary key of the table row is to be provided.</param>
		public void BufferDelta(SnmpDeltaHelper deltaHelper, string rowKey = null)
		{
			var delta = deltaHelper.GetDelta(rowKey);
			if (!delta.HasValue)
			{
				return;
			}

			bufferedDelta += delta.Value;
		}

		/// <summary>
		/// Serializes the currently buffered data of this <see cref="SnmpRate"/> instance.
		/// </summary>
		/// <returns>A JSON string containing the serialized data of this <see cref="SnmpRate"/> instance.</returns>
		public string ToJsonString()
		{
			//var settings = new JsonSerializerSettings
			//{
			//	TypeNameHandling = TypeNameHandling.All
			//};

			return JsonConvert.SerializeObject(this/*, settings*/);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt32"/> counters polled over SNMP.
	/// </summary>
	public class SnmpRate32 : SnmpRate<Counter32WithTime, uint>
	{
		[JsonConstructor]
		private SnmpRate32(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
		{
			rateOnTimes = new Rate32OnTimes(minDelta, maxDelta, rateBase);
		}

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="SnmpRate32"/> instance.
		/// </summary>
		/// <param name="deltaHelper">An instance of the <see cref="SnmpDeltaHelper"/> class which will take care of fetching the delta from DataMiner.</param>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="rowKey">The primary key of the table row for which a rate should be calculated.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(SnmpDeltaHelper deltaHelper, uint newCounter, string rowKey = null, double faultyReturn = -1)
		{
			var rateCounter = new Counter32WithTime(newCounter, TimeSpan.Zero);
			return Calculate(deltaHelper, rateCounter, rowKey, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="SnmpRate32"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="SnmpRate32"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="maxDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <param name="rateBase">Choose whether the rate should be calculated per second, minute, hour or day.</param>
		/// <returns>A new instance of the <see cref="SnmpRate32"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
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

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt64"/> counters polled over SNMP.
	/// </summary>
	public class SnmpRate64 : SnmpRate<Counter64WithTime, ulong>
	{
		[JsonConstructor]
		private SnmpRate64(TimeSpan minDelta, TimeSpan maxDelta, RateBase rateBase)
		{
			rateOnTimes = new Rate64OnTimes(minDelta, maxDelta, rateBase);
		}

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="SnmpRate64"/> instance.
		/// </summary>
		/// <param name="deltaHelper">An instance of the <see cref="SnmpDeltaHelper"/> class which will take care of fetching the delta from DataMiner.</param>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="rowKey">The primary key of the table row for which a rate should be calculated.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(SnmpDeltaHelper deltaHelper, ulong newCounter, string rowKey = null, double faultyReturn = -1)
		{
			var rateCounter = new Counter64WithTime(newCounter, TimeSpan.Zero);
			return Calculate(deltaHelper, rateCounter, rowKey, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="SnmpRate64"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="SnmpRate64"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="maxDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <param name="rateBase">Choose whether the rate should be calculated per second, minute, hour or day.</param>
		/// <returns>A new instance of the <see cref="SnmpRate64"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
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
