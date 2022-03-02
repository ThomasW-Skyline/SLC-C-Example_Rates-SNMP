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
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(10);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(-10000);   // -10s
			double rate = helper.Calculate(20);

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Invalid_TooLate()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(10);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(3660000);   // 1h 1m
			double rate = helper.Calculate(20);

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Invalid_TooSoon()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(0);   // 0s
			helper.Calculate(10);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(5000);   // 5s
			double rate = helper.Calculate(20);

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToOlderCounter()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(0);    // Old counter
			helper.Calculate(1);    // Old counter

			helper.Calculate(5);     // Counter to be used

			// Recent counters
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(90000);   // 1m30s
			helper.Calculate(10);   // 1m30s

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(1000);   // 1s
			helper.Calculate(20);    // 1m31s
			helper.Calculate(30);    // 1m32s
			helper.Calculate(40);    // 1m33s

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(7000);   // 7s
			double rate = helper.Calculate(50);  // 1m40s

			// Assert
			double expectedRate = (50.0 - 5.0) / (7 + 1 + 1 + 1 + 90);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToOlderCounter_WithTimeouts()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.BufferDelta();
			helper.Calculate(0);    // Old counter
			helper.BufferDelta();
			helper.Calculate(1);    // Old counter

			helper.BufferDelta();
			helper.Calculate(5);	// Counter to be used
			helper.BufferDelta();	// 10s

			// Recent counters
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(80000);   // 80s
			helper.Calculate(10);   // 1m30s

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(1000);   // 1s
			helper.Calculate(20);	// 1m31s
			helper.BufferDelta();	// 1m32s
			helper.Calculate(30);	// 1m33s
			helper.BufferDelta();   // 1m34s
			helper.Calculate(40);	// 1m35s
			helper.BufferDelta();   // 1m36s

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(4000);   // 4s
			double rate = helper.Calculate(50);  // 1m40s

			// Assert
			double expectedRate = (50.0 - 5.0) / (4 + 1 + 1 + 1 + 1 + 1 + 1 + 80 + 10);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(0);    // Old counters
			helper.Calculate(1);    // Old counters

			helper.Calculate(5);    // Counter to be used

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(100000);  // 100s
			double rate = helper.Calculate(50);

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_PerDay()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta, RateBase.Day);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(5);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(100000);   // 100s
			double rate = helper.Calculate(50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60 / 60 / 24);
			////Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
			rate.Should().BeApproximately(expectedRate, Math.Pow(10, -9));
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_PerHour()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta, RateBase.Hour);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(5);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(100000);   // 100s
			double rate = helper.Calculate(50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60 / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_PerMinute()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta, RateBase.Minute);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(5);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(100000);   // 100s
			double rate = helper.Calculate(50);

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter_WithTimeouts()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper.Calculate(0);    // Old counters
			helper.Calculate(1);    // Old counters

			helper.Calculate(5);    // Counter to be used

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(5000);   // 5s
			helper.BufferDelta();   // 5s
			helper.BufferDelta();   // 10s
			helper.BufferDelta();   // 15s
			helper.BufferDelta();   // 20s

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(100000);  // 100s
			double rate = helper.Calculate(50);     // 120s

			// Assert
			double expectedRate = (50.0 - 5.0) / 120d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_WithOverflow()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(0);   // 0s
			helper.Calculate(UInt32.MaxValue - 10);

			// Act
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(100000);   // 100s
			double rate = helper.Calculate(9);

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
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper1 = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper1.Calculate(5);
			helper1.Calculate(10);

			string serializedTemp = helper1.ToJsonString();
			var helper2 = SnmpRate32.FromJsonString(protocolMock.Object, serializedTemp, groupId, minDelta, maxDelta);

			// Different counter, same timing
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(9000);   // 9s
			helper1.Calculate(20);
			helper2.Calculate(21);

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Invalid_DifferentTimeSpan()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper1 = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper1.Calculate(5);
			helper1.Calculate(10);

			string serializedTemp = helper1.ToJsonString();
			var helper2 = SnmpRate32.FromJsonString(protocolMock.Object, serializedTemp, groupId, minDelta, maxDelta);

			// Same counter, different timing
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(9000);   // 9s
			helper1.Calculate(20);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(2000);   // 2s
			helper2.Calculate(20);

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Valid()
		{
			// Arrange
			Mock<SLProtocol> protocolMock = new Mock<SLProtocol>();
			var helper1 = SnmpRate32.FromJsonString(protocolMock.Object, "", groupId, minDelta, maxDelta);

			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns(10000);   // 10s
			helper1.Calculate(5);
			helper1.Calculate(10);

			string serializedTemp = helper1.ToJsonString();
			var helper2 = SnmpRate32.FromJsonString(protocolMock.Object, serializedTemp, groupId, minDelta, maxDelta);

			AddSameToBoth(protocolMock, helper1, helper2, 20, new TimeSpan(0, 0, 9));
			AddSameToBoth(protocolMock, helper1, helper2, 30, new TimeSpan(0, 0, 8));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().BeEquivalentTo(serialized2);
		}

		#endregion

		#region HelperMethods
		private static void AddSameToBoth(Mock<SLProtocol> protocolMock, SnmpRate32 helper1, SnmpRate32 helper2, uint newCounter, TimeSpan time)
		{
			protocolMock.Setup(p => p.NotifyProtocol(269, groupId, null)).Returns((int)time.TotalMilliseconds);
			helper1.Calculate(newCounter);
			helper2.Calculate(newCounter);
		}
		#endregion
	}
}