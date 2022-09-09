using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
			CounterTimeoutProcessor streamsHelper = new CounterTimeoutProcessor(protocol);
			streamsHelper.ProcessTimeout();
			streamsHelper.UpdateProtocol();
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
			CounterProcessor streamsHelper = new CounterProcessor(protocol);
			streamsHelper.ProcessData();
			streamsHelper.UpdateProtocol();
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|" + MethodName + "|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}

		////protocol.Log("QA" + protocol.QActionID + "|" + MethodName + "|### End of QAction", LogType.DebugInfo, LogLevel.NoLogging);
	}
}
