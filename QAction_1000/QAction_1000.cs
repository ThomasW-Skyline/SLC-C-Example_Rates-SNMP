using System;

using Skyline.DataMiner.Scripting;
using Skyline.Protocol.Streams;

/// <summary>
/// DataMiner QAction Class: Fill Streams Table.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		protocol.Log("QA" + protocol.QActionID + "|Run|### Start of QAction", LogType.DebugInfo, LogLevel.NoLogging);

		try
		{
			StreamsHelper streamsHelper = new StreamsHelper(protocol);
			protocol.Log("QA" + protocol.QActionID + "|Run|StreamsHelper made", LogType.DebugInfo, LogLevel.NoLogging);

			streamsHelper.ProcessData();
			protocol.Log("QA" + protocol.QActionID + "|Run|ProcessData ran", LogType.DebugInfo, LogLevel.NoLogging);

			streamsHelper.UpdateProtocol();
			protocol.Log("QA" + protocol.QActionID + "|Run|UpdateProtocol ran", LogType.DebugInfo, LogLevel.NoLogging);
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}

		protocol.Log("QA" + protocol.QActionID + "|Run|### Start of QAction", LogType.DebugInfo, LogLevel.NoLogging);
	}
}