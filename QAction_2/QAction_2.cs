using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Skyline.DataMiner.Scripting;
using Skyline.Protocol.Rates;

/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
        try
		{
			// Every restart of an element, the method is defaulted back to "Fast" by DataMiner so we only need to change it if we expect 'Accurate'
			SnmpDeltaHelper.CalculationMethod rateCalculationsMethod = (SnmpDeltaHelper.CalculationMethod)Convert.ToInt32(protocol.GetParameter(Parameter.streamsratecalculationsmethod));
			if (rateCalculationsMethod == SnmpDeltaHelper.CalculationMethod.Accurate)
			{
				protocol.NotifyProtocol(/*NT_SET_BITRATE_DELTA_INDEX_TRACKING*/ 448, 1000, true);
			}
		}
        catch (Exception ex)
        {
            protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
        }
    }
}
