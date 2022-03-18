using System;

namespace Skyline.Protocol.Rates
{
	#region Base Classes
	public class RateCounter<U>
	{
		public U Counter { get; set; }

		private protected RateCounter() { }     // Default constructor for Deserializer

		private protected RateCounter(U counter)
		{
			Counter = counter;
		}
	}

	public class CounterWithTimeStamp<U> : RateCounter<U>
	{
		public DateTime DateTime { get; set; }

		private protected CounterWithTimeStamp() { }     // Default constructor for Deserializer

		internal CounterWithTimeStamp(U counter, DateTime dateTime) : base(counter)
		{
			DateTime = dateTime;
		}
	}

	public class CounterWithTimeSpan<U> : RateCounter<U>
	{
		public TimeSpan TimeSpan { get; set; }

		private protected CounterWithTimeSpan() { }     // Default constructor for Deserializer

		internal CounterWithTimeSpan(U counter, TimeSpan timeSpan) : base(counter)
		{
			TimeSpan = timeSpan;
		}
	}
	#endregion

	#region Real Classes
	public class Counter32WithTimeStamp : CounterWithTimeStamp<uint>
	{
		private Counter32WithTimeStamp() { }     // Default constructor for Deserializer

		internal Counter32WithTimeStamp(uint counter, DateTime dateTime) : base(counter, dateTime) { }
	}

	public class Counter32WithTimeSpan : CounterWithTimeSpan<uint>
	{
		private Counter32WithTimeSpan() { }     // Default constructor for Deserializer

		internal Counter32WithTimeSpan(uint counter, TimeSpan timeSpan) : base(counter, timeSpan) { }
	}

	public class Counter64WithTimeStamp : CounterWithTimeStamp<ulong>
	{
		private Counter64WithTimeStamp() { }     // Default constructor for Deserializer

		internal Counter64WithTimeStamp(ulong counter, DateTime dateTime) : base(counter, dateTime) { }
	}

	public class Counter64WithTimeSpan : CounterWithTimeSpan<ulong>
	{
		private Counter64WithTimeSpan() { }     // Default constructor for Deserializer

		internal Counter64WithTimeSpan(ulong counter, TimeSpan timeSpan) : base(counter, timeSpan) { }
	}
	#endregion
}