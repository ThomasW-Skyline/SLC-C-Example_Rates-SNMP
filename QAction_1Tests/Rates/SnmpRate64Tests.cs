using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skyline.Protocol.Rates;
using Moq;
using Skyline.DataMiner.Scripting;
using FluentAssertions;
using static Skyline.Protocol.Rates.SnmpDeltaHelper;

namespace Skyline.Protocol.Rates.Tests
{
	[TestClass()]
	public class SnmpRate64Tests
	{
		private readonly TimeSpan minDelta = new TimeSpan(0, 1, 0);
		private readonly TimeSpan maxDelta = new TimeSpan(1, 0, 0);

		private const int groupId = 100;
		private const double faultyReturn = -1;

		#region Calculate

		[TestMethod()]
		public void Calculate_Invalid_BackInTime()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 10);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, -10));
			double rate = rateHelper.Calculate(deltaHelper, 20);

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Invalid_TooLate()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 10);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(1, 1, 0));
			double rate = rateHelper.Calculate(deltaHelper, 20);

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Invalid_TooSoon()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 0));
			rateHelper.Calculate(deltaHelper, 10);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 5));
			double rate = rateHelper.Calculate(deltaHelper, 20);

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToOlderCounter()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 0);    // Old counter
			rateHelper.Calculate(deltaHelper, 1);    // Old counter

			rateHelper.Calculate(deltaHelper, 5);     // Counter to be used

			// Recent counters
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 1, 30));
			rateHelper.Calculate(deltaHelper, 10);   // 1m30s

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 1));
			rateHelper.Calculate(deltaHelper, 20);    // 1m31s
			rateHelper.Calculate(deltaHelper, 30);    // 1m64s
			rateHelper.Calculate(deltaHelper, 40);    // 1m33s

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 7));
			double rate = rateHelper.Calculate(deltaHelper, 50);  // 1m40s

			// Assert
			double expectedRate = (50.0 - 5.0) / (7 + 1 + 1 + 1 + 90);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToOlderCounter_WithTimeouts()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.BufferDelta(deltaHelper);
			rateHelper.Calculate(deltaHelper, 0);    // Old counter
			rateHelper.BufferDelta(deltaHelper);
			rateHelper.Calculate(deltaHelper, 1);    // Old counter

			rateHelper.BufferDelta(deltaHelper);
			rateHelper.Calculate(deltaHelper, 5);   // Counter to be used
			rateHelper.BufferDelta(deltaHelper);   // 10s

			// Recent counters
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 80));
			rateHelper.Calculate(deltaHelper, 10);   // 1m30s

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 1));
			rateHelper.Calculate(deltaHelper, 20);  // 1m31s
			rateHelper.BufferDelta(deltaHelper);    // 1m64s
			rateHelper.Calculate(deltaHelper, 30);  // 1m33s
			rateHelper.BufferDelta(deltaHelper);   // 1m34s
			rateHelper.Calculate(deltaHelper, 40);  // 1m35s
			rateHelper.BufferDelta(deltaHelper);   // 1m36s

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 4));
			double rate = rateHelper.Calculate(deltaHelper, 50);  // 1m40s

			// Assert
			double expectedRate = (50.0 - 5.0) / (4 + 1 + 1 + 1 + 1 + 1 + 1 + 80 + 10);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_Accurate()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureAccurateDelta(protocolMock, new object[] { new object[] { "PK_1", -1 }, new object[] { "PK_2", 10000 }, new object[] { "PK_3", -1 } });  // 10s
			rateHelper.Calculate(deltaHelper, 0, "PK_2");    // Old counters
			rateHelper.Calculate(deltaHelper, 1, "PK_2");    // Old counters

			rateHelper.Calculate(deltaHelper, 5, "PK_2");    // Counter to be used

			// Act
			deltaHelper = ConfigureAccurateDelta(protocolMock, new object[] { new object[] { "PK_1", -1 }, new object[] { "PK_2", 100000 }, new object[] { "PK_3", -1 } }); // 100s
			double rate = rateHelper.Calculate(deltaHelper, 50, "PK_2");

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_Fast()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 0);    // Old counters
			rateHelper.Calculate(deltaHelper, 1);    // Old counters

			rateHelper.Calculate(deltaHelper, 5);    // Counter to be used

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, 50);

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_PerDay()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta, RateBase.Day);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 5);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, 50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60 / 60 / 24);
			////Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
			rate.Should().BeApproximately(expectedRate, Math.Pow(10, -9));
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_PerHour()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta, RateBase.Hour);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 5);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, 50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60 / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_PerMinute()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta, RateBase.Minute);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 5);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, 50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_WithTimeouts_Accurate()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureAccurateDelta(protocolMock, new object[] { new object[] { "PK_1", -1 }, new object[] { "PK_2", 10000 }, new object[] { "PK_3", -1 } });  // 10s
			rateHelper.Calculate(deltaHelper, 0, "PK_2");    // Old counters
			rateHelper.Calculate(deltaHelper, 1, "PK_2");    // Old counters

			rateHelper.Calculate(deltaHelper, 5, "PK_2");    // Counter to be used

			deltaHelper = ConfigureAccurateTimeoutDelta(protocolMock, new TimeSpan(0, 0, 5));  // 5s
			rateHelper.BufferDelta(deltaHelper, "PK_2");   // 5s
			rateHelper.BufferDelta(deltaHelper, "PK_2");   // 10s
			rateHelper.BufferDelta(deltaHelper, "PK_2");   // 15s
			rateHelper.BufferDelta(deltaHelper, "PK_2");   // 20s

			// Act
			deltaHelper = ConfigureAccurateDelta(protocolMock, new object[] { new object[] { "PK_1", -1 }, new object[] { "PK_2", 100000 }, new object[] { "PK_3", -1 } });  // 100s
			double rate = rateHelper.Calculate(deltaHelper, 50, "PK_2");     // 120s

			// Assert
			double expectedRate = (50.0 - 5.0) / 120d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_WithTimeouts_Fast()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, 0);    // Old counters
			rateHelper.Calculate(deltaHelper, 1);    // Old counters

			rateHelper.Calculate(deltaHelper, 5);    // Counter to be used

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 5));
			rateHelper.BufferDelta(deltaHelper);   // 5s
			rateHelper.BufferDelta(deltaHelper);   // 10s
			rateHelper.BufferDelta(deltaHelper);   // 15s
			rateHelper.BufferDelta(deltaHelper);   // 20s

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, 50);     // 120s

			// Assert
			double expectedRate = (50.0 - 5.0) / 120d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_WithOverflow()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 0));
			rateHelper.Calculate(deltaHelper, UInt64.MaxValue - 10);

			// Act
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, 9);

			// Assert
			double expectedRate = 20 / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		#endregion

		#region SerializeTests

		[TestMethod()]
		public void Serialize_Invalid_DifferentCounter()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper1 = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper1.Calculate(deltaHelper, 5);
			rateHelper1.Calculate(deltaHelper, 10);

			string serializedTemp = rateHelper1.ToJsonString();
			var rateHelper2 = SnmpRate64.FromJsonString(serializedTemp, minDelta, maxDelta);

			// Different counter, same timing
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 9));
			rateHelper1.Calculate(deltaHelper, 20);
			rateHelper2.Calculate(deltaHelper, 21);

			// Act
			string serialized1 = rateHelper1.ToJsonString();
			string serialized2 = rateHelper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Invalid_DifferentTimeSpan()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper1 = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper1.Calculate(deltaHelper, 5);
			rateHelper1.Calculate(deltaHelper, 10);

			string serializedTemp = rateHelper1.ToJsonString();
			var rateHelper2 = SnmpRate64.FromJsonString(serializedTemp, minDelta, maxDelta);

			// Same counter, different timing
			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 9));
			rateHelper1.Calculate(deltaHelper, 20);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 2));
			rateHelper2.Calculate(deltaHelper, 20);

			// Act
			string serialized1 = rateHelper1.ToJsonString();
			string serialized2 = rateHelper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Valid()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper1 = SnmpRate64.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureFastDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper1.Calculate(deltaHelper, 5);
			rateHelper1.Calculate(deltaHelper, 10);

			string serializedTemp = rateHelper1.ToJsonString();
			var rateHelper2 = SnmpRate64.FromJsonString(serializedTemp, minDelta, maxDelta);

			AddSameToBoth(rateHelper1, rateHelper2, 20, new TimeSpan(0, 0, 9));
			AddSameToBoth(rateHelper1, rateHelper2, 30, new TimeSpan(0, 0, 8));

			// Act
			string serialized1 = rateHelper1.ToJsonString();
			string serialized2 = rateHelper2.ToJsonString();

			// Assert
			serialized1.Should().BeEquivalentTo(serialized2);
		}

		#endregion

		#region HelperMethods
		private static SnmpDeltaHelper ConfigureFastDelta(Mock<SLProtocol> protocolMock, TimeSpan delta)
		{
			SnmpDeltaHelper deltaHelper = new SnmpDeltaHelper(protocolMock.Object, groupId);
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns((int)delta.TotalMilliseconds);
			return deltaHelper;
		}

		private static SnmpDeltaHelper ConfigureAccurateDelta(Mock<SLProtocol> protocolMock, object[] deltaValues, uint calculationMethodPid = 100)
		{
			protocolMock.Setup(p => p.GetParameter((int)calculationMethodPid)).Returns((int)CalculationMethod.Accurate);

			SnmpDeltaHelper deltaHelper = new SnmpDeltaHelper(protocolMock.Object, groupId, calculationMethodPid);
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, "")).Returns(deltaValues);
			return deltaHelper;
		}

		private static SnmpDeltaHelper ConfigureAccurateTimeoutDelta(Mock<SLProtocol> protocolMock, TimeSpan delta, uint calculationMethodPid = 100)
		{
			protocolMock.Setup(p => p.GetParameter((int)calculationMethodPid)).Returns((int)CalculationMethod.Accurate);

			SnmpDeltaHelper deltaHelper = new SnmpDeltaHelper(protocolMock.Object, groupId, calculationMethodPid);
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, "")).Returns((int)delta.TotalMilliseconds);
			return deltaHelper;
		}

		private static void AddSameToBoth(SnmpRate64 helper1, SnmpRate64 helper2, uint newCounter, TimeSpan time)
		{
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();

			deltaHelper = ConfigureFastDelta(protocolMock, time);
			helper1.Calculate(deltaHelper, newCounter);
			helper2.Calculate(deltaHelper, newCounter);
		}
		#endregion
	}
}