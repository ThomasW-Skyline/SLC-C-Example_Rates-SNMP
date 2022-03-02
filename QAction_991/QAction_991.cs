using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class Streams
{
    /// <summary>
    /// The QAction entry point in case of timeout.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void ProcessTimeout(SLProtocol protocol)
    {
        try
        {

        }
        catch (Exception ex)
        {
            protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|ProcessTimeout|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
        }
    }

	/// <summary>
	/// The QAction entry point in case of successful group execution.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void ProcessTable(SLProtocol protocol)
	{
		try
		{

		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|ProcessTable|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}
}
