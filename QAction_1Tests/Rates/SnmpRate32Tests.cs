using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skyline.Protocol.Rates;
using Moq;
using Skyline.DataMiner.Scripting;
using FluentAssertions;

namespace Skyline.Protocol.Rates.Tests
{
	[TestClass()]
	public class SnmpRate32Tests
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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 10);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, -10));
			double rate = rateHelper.Calculate(deltaHelper, null, 20);

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 10);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(1, 1, 0));
			double rate = rateHelper.Calculate(deltaHelper, null, 20);

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 0));
			rateHelper.Calculate(deltaHelper, null, 10);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 5));
			double rate = rateHelper.Calculate(deltaHelper, null, 20);

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 0);    // Old counter
			rateHelper.Calculate(deltaHelper, null, 1);    // Old counter

			rateHelper.Calculate(deltaHelper, null, 5);     // Counter to be used

			// Recent counters
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 1, 30));
			rateHelper.Calculate(deltaHelper, null, 10);   // 1m30s

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 1));
			rateHelper.Calculate(deltaHelper, null, 20);    // 1m31s
			rateHelper.Calculate(deltaHelper, null, 30);    // 1m32s
			rateHelper.Calculate(deltaHelper, null, 40);    // 1m33s

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 7));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);  // 1m40s

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.BufferDelta(deltaHelper, null);
			rateHelper.Calculate(deltaHelper, null, 0);    // Old counter
			rateHelper.BufferDelta(deltaHelper, null);
			rateHelper.Calculate(deltaHelper, null, 1);    // Old counter

			rateHelper.BufferDelta(deltaHelper, null);
			rateHelper.Calculate(deltaHelper, null, 5);	// Counter to be used
			rateHelper.BufferDelta(deltaHelper, null);   // 10s

			// Recent counters
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 80));
			rateHelper.Calculate(deltaHelper, null, 10);   // 1m30s

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 1));
			rateHelper.Calculate(deltaHelper, null, 20);	// 1m31s
			rateHelper.BufferDelta(deltaHelper, null);	// 1m32s
			rateHelper.Calculate(deltaHelper, null, 30);	// 1m33s
			rateHelper.BufferDelta(deltaHelper, null);   // 1m34s
			rateHelper.Calculate(deltaHelper, null, 40);	// 1m35s
			rateHelper.BufferDelta(deltaHelper, null);   // 1m36s

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 4));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);  // 1m40s

			// Assert
			double expectedRate = (50.0 - 5.0) / (4 + 1 + 1 + 1 + 1 + 1 + 1 + 80 + 10);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 0);    // Old counters
			rateHelper.Calculate(deltaHelper, null, 1);    // Old counters

			rateHelper.Calculate(deltaHelper, null, 5);    // Counter to be used

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta, RateBase.Day);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 5);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta, RateBase.Hour);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 5);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta, RateBase.Minute);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 5);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_WithTimeouts()
		{
			// Arrange
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper.Calculate(deltaHelper, null, 0);    // Old counters
			rateHelper.Calculate(deltaHelper, null, 1);    // Old counters

			rateHelper.Calculate(deltaHelper, null, 5);    // Counter to be used

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 5));
			rateHelper.BufferDelta(deltaHelper, null);   // 5s
			rateHelper.BufferDelta(deltaHelper, null);   // 10s
			rateHelper.BufferDelta(deltaHelper, null);   // 15s
			rateHelper.BufferDelta(deltaHelper, null);   // 20s

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, null, 50);     // 120s

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
			var rateHelper = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 0));
			rateHelper.Calculate(deltaHelper, null, UInt32.MaxValue - 10);

			// Act
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 100));
			double rate = rateHelper.Calculate(deltaHelper, null, 9);

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
			var rateHelper1 = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper1.Calculate(deltaHelper, null, 5);
			rateHelper1.Calculate(deltaHelper, null, 10);

			string serializedTemp = rateHelper1.ToJsonString();
			var rateHelper2 = SnmpRate32.FromJsonString(serializedTemp, minDelta, maxDelta);

			// Different counter, same timing
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 9));
			rateHelper1.Calculate(deltaHelper, null, 20);
			rateHelper2.Calculate(deltaHelper, null, 21);

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
			var rateHelper1 = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper1.Calculate(deltaHelper, null, 5);
			rateHelper1.Calculate(deltaHelper, null, 10);

			string serializedTemp = rateHelper1.ToJsonString();
			var rateHelper2 = SnmpRate32.FromJsonString(serializedTemp, minDelta, maxDelta);

			// Same counter, different timing
			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 9));
			rateHelper1.Calculate(deltaHelper, null, 20);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 2));
			rateHelper2.Calculate(deltaHelper, null, 20);

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
			var rateHelper1 = SnmpRate32.FromJsonString("", minDelta, maxDelta);

			deltaHelper = ConfigureDelta(protocolMock, new TimeSpan(0, 0, 10));
			rateHelper1.Calculate(deltaHelper, null, 5);
			rateHelper1.Calculate(deltaHelper, null, 10);

			string serializedTemp = rateHelper1.ToJsonString();
			var helper2 = SnmpRate32.FromJsonString(serializedTemp, minDelta, maxDelta);

			AddSameToBoth(rateHelper1, helper2, 20, new TimeSpan(0, 0, 9));
			AddSameToBoth(rateHelper1, helper2, 30, new TimeSpan(0, 0, 8));

			// Act
			string serialized1 = rateHelper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().BeEquivalentTo(serialized2);
		}

		#endregion

		#region HelperMethods
		private static SnmpDeltaHelper ConfigureDelta(Mock<SLProtocol> protocolMock, TimeSpan delta)
		{
			SnmpDeltaHelper deltaHelper = new SnmpDeltaHelper(protocolMock.Object, groupId);
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns((int)delta.TotalMilliseconds);
			return deltaHelper;
		}

		private static void AddSameToBoth(SnmpRate32 helper1, SnmpRate32 helper2, uint newCounter, TimeSpan time)
		{
			SnmpDeltaHelper deltaHelper;
			var protocolMock = new Mock<SLProtocol>();

			deltaHelper = ConfigureDelta(protocolMock, time);
			helper1.Calculate(deltaHelper, null, newCounter);
			helper2.Calculate(deltaHelper, null, newCounter);
		}
		#endregion
	}
}