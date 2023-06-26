using System;

using Skyline.DataMiner.Scripting;
using Skyline.Protocol.Streams;

/// <summary>
/// DataMiner QAction Class: Streams Processors.
/// </summary>
public static class Streams
{
	/// <summary>
	/// The QAction entry point in case of timeout.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void ProcessTimeout(SLProtocol protocol)
	{
		const string MethodName = "Streams.ProcessTimeout";
		////protocol.Log($"QA{protocol.QActionID}|{MethodName}|### Start of QAction", LogType.DebugInfo, LogLevel.NoLogging);

		try
		{
			StreamsTimeoutProcessor streamsHelper = new StreamsTimeoutProcessor(protocol);
			streamsHelper.ProcessTimeout();
			streamsHelper.UpdateProtocol();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|{MethodName}|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}

		////protocol.Log($"QA{protocol.QActionID}|{MethodName}|### End of QAction", LogType.DebugInfo, LogLevel.NoLogging);
	}

	/// <summary>
	/// The QAction entry point in case of successful group execution.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void ProcessTable(SLProtocol protocol)
	{
		const string MethodName = "Streams.ProcessTable";
		////protocol.Log($"QA{protocol.QActionID}|{MethodName}|### Start of QAction", LogType.DebugInfo, LogLevel.NoLogging);

		try
		{
			StreamsProcessor streamsHelper = new StreamsProcessor(protocol);
			streamsHelper.ProcessData();
			streamsHelper.UpdateProtocol();
		}
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|{MethodName}|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}

		////protocol.Log($"QA{protocol.QActionID}|{MethodName}|### End of QAction", LogType.DebugInfo, LogLevel.NoLogging);
	}
}