using System;

using Skyline.DataMiner.Scripting;
using Skyline.Protocol.Counter;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class Counter
{
	/// <summary>
	/// The QAction entry point in case of timeout.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void ProcessTimeout(SLProtocol protocol)
	{
		const string MethodName = "Counter.ProcessTimeout";
		////protocol.Log("QA" + protocol.QActionID + "|" + MethodName + "|### Start of QAction", LogType.DebugInfo, LogLevel.NoLogging);

		try
		{
			CounterTimeoutProcessor counterHelper = new CounterTimeoutProcessor(protocol);
			counterHelper.ProcessTimeout();
			counterHelper.UpdateProtocol();
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|" + MethodName + "|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}

		////protocol.Log("QA" + protocol.QActionID + "|" + MethodName + "|### End of QAction", LogType.DebugInfo, LogLevel.NoLogging);
	}

	/// <summary>
	/// The QAction entry point in case of successful group execution.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void ProcessCounter(SLProtocol protocol)
	{
		const string MethodName = "Counter.ProcessCounter";
		////protocol.Log("QA" + protocol.QActionID + "|" + MethodName + "|### Start of QAction", LogType.DebugInfo, LogLevel.NoLogging);

		try
		{
			CounterProcessor counterHelper = new CounterProcessor(protocol);
			counterHelper.ProcessData();
			counterHelper.UpdateProtocol();
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|" + MethodName + "|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}

		////protocol.Log("QA" + protocol.QActionID + "|" + MethodName + "|### End of QAction", LogType.DebugInfo, LogLevel.NoLogging);
	}
}
