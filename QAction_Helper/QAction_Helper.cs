// --- auto-generated code --- do not modify ---
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skyline.DataMiner.Scripting
{
public static class Parameter
{
	/// <summary>PID: 992 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int streamstimeoutafterretriesflag_992 = 992;
	/// <summary>PID: 992 | Type: read</summary>
	public const int streamstimeoutafterretriesflag = 992;
	/// <summary>PID: 995 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public const int streamsratecalculationsmethod_995 = 995;
	/// <summary>PID: 995 | Type: read</summary>
	public const int streamsratecalculationsmethod = 995;
	public class Write
	{
		/// <summary>PID: 996 | Type: write</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public const int streamsratecalculationsmethod_996 = 996;
		/// <summary>PID: 996 | Type: write</summary>
		public const int streamsratecalculationsmethod = 996;
	}
	public class Streams
	{
		/// <summary>PID: 1000</summary>
		public const int tablePid = 1000;
		/// <summary>IDX: 0</summary>
		public const int indexColumn = 0;
		/// <summary>PID: 1001</summary>
		public const int indexColumnPid = 1001;
		public class Pid
		{
			/// <summary>PID: 1001 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsindex_1001 = 1001;
			/// <summary>PID: 1001 | Type: read</summary>
			public const int streamsindex = 1001;
			/// <summary>PID: 1002 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsdrescription_1002 = 1002;
			/// <summary>PID: 1002 | Type: read</summary>
			public const int streamsdrescription = 1002;
			/// <summary>PID: 1003 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsoctetscounter_1003 = 1003;
			/// <summary>PID: 1003 | Type: read</summary>
			public const int streamsoctetscounter = 1003;
			/// <summary>PID: 1004 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrate_1004 = 1004;
			/// <summary>PID: 1004 | Type: read</summary>
			public const int streamsbitrate = 1004;
			/// <summary>PID: 1005 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitratedata_1005 = 1005;
			/// <summary>PID: 1005 | Type: read</summary>
			public const int streamsbitratedata = 1005;
			public class Write
			{
			}
		}
		public class Idx
		{
			/// <summary>IDX: 0 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsindex_1001 = 0;
			/// <summary>IDX: 0 | Type: read</summary>
			public const int streamsindex = 0;
			/// <summary>IDX: 1 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsdrescription_1002 = 1;
			/// <summary>IDX: 1 | Type: read</summary>
			public const int streamsdrescription = 1;
			/// <summary>IDX: 2 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsoctetscounter_1003 = 2;
			/// <summary>IDX: 2 | Type: read</summary>
			public const int streamsoctetscounter = 2;
			/// <summary>IDX: 3 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitrate_1004 = 3;
			/// <summary>IDX: 3 | Type: read</summary>
			public const int streamsbitrate = 3;
			/// <summary>IDX: 4 | Type: read</summary>
			[EditorBrowsable(EditorBrowsableState.Never)]
			public const int streamsbitratedata_1005 = 4;
			/// <summary>IDX: 4 | Type: read</summary>
			public const int streamsbitratedata = 4;
		}
	}
}
public class WriteParameters
{
	/// <summary>PID: 996  | Type: write | DISCREETS: Fast = 1, Accurate = 2</summary>
	public System.Object Streamsratecalculationsmethod {get { return Protocol.GetParameter(996); }set { Protocol.SetParameter(996, value); }}
	public SLProtocolExt Protocol;
	public WriteParameters(SLProtocolExt protocol)
	{
		Protocol = protocol;
	}
}
public interface SLProtocolExt : SLProtocol
{
	/// <summary>PID: 1000</summary>
	StreamsQActionTable streams { get; set; }
	object Afterstartup_dummy { get; set; }
	object Dummyforqa3_dummy { get; set; }
	object Fixed_0__fixed { get; set; }
	object Fixed_1__fixed { get; set; }
	object Streamstimeouttrigger_dummy { get; set; }
	object Streamstimeoutafterretriesflag_992 { get; set; }
	object Streamstimeoutafterretriesflag { get; set; }
	object Streamsaftergrouptrigger_dummy { get; set; }
	object Streamsratecalculationsmethod_995 { get; set; }
	object Streamsratecalculationsmethod { get; set; }
	object Streamsratecalculationsmethod_996 { get; set; }
	object Streamsindex_1001 { get; set; }
	object Streamsindex { get; set; }
	object Streamsdrescription_1002 { get; set; }
	object Streamsdrescription { get; set; }
	object Streamsoctetscounter_1003 { get; set; }
	object Streamsoctetscounter { get; set; }
	object Streamsbitrate_1004 { get; set; }
	object Streamsbitrate { get; set; }
	object Streamsbitratedata_1005 { get; set; }
	object Streamsbitratedata { get; set; }
	WriteParameters Write { get; set; }
}
public class ConcreteSLProtocolExt : ConcreteSLProtocol, SLProtocolExt
{
	/// <summary>PID: 1000</summary>
	public StreamsQActionTable streams { get; set; }
	/// <summary>PID: 2  | Type: dummy</summary>
	public System.Object Afterstartup_dummy {get { return GetParameter(2); }set { SetParameter(2, value); }}
	/// <summary>PID: 3  | Type: dummy</summary>
	public System.Object Dummyforqa3_dummy {get { return GetParameter(3); }set { SetParameter(3, value); }}
	/// <summary>PID: 10  | Type: fixed</summary>
	public System.Object Fixed_0__fixed {get { return GetParameter(10); }set { SetParameter(10, value); }}
	/// <summary>PID: 11  | Type: fixed</summary>
	public System.Object Fixed_1__fixed {get { return GetParameter(11); }set { SetParameter(11, value); }}
	/// <summary>PID: 991  | Type: dummy</summary>
	public System.Object Streamstimeouttrigger_dummy {get { return GetParameter(991); }set { SetParameter(991, value); }}
	/// <summary>PID: 992  | Type: read | DISCREETS: False = 0, True = 1</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamstimeoutafterretriesflag_992 {get { return GetParameter(992); }set { SetParameter(992, value); }}
	/// <summary>PID: 992  | Type: read | DISCREETS: False = 0, True = 1</summary>
	public System.Object Streamstimeoutafterretriesflag {get { return GetParameter(992); }set { SetParameter(992, value); }}
	/// <summary>PID: 993  | Type: dummy</summary>
	public System.Object Streamsaftergrouptrigger_dummy {get { return GetParameter(993); }set { SetParameter(993, value); }}
	/// <summary>PID: 995  | Type: read | DISCREETS: Fast = 1, Accurate = 2</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsratecalculationsmethod_995 {get { return GetParameter(995); }set { SetParameter(995, value); }}
	/// <summary>PID: 995  | Type: read | DISCREETS: Fast = 1, Accurate = 2</summary>
	public System.Object Streamsratecalculationsmethod {get { return GetParameter(995); }set { SetParameter(995, value); }}
	/// <summary>PID: 996  | Type: write | DISCREETS: Fast = 1, Accurate = 2</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsratecalculationsmethod_996 {get { return GetParameter(996); }set { SetParameter(996, value); }}
	/// <summary>PID: 1001  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsindex_1001 {get { return GetParameter(1001); }set { SetParameter(1001, value); }}
	/// <summary>PID: 1001  | Type: read</summary>
	public System.Object Streamsindex {get { return GetParameter(1001); }set { SetParameter(1001, value); }}
	/// <summary>PID: 1002  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsdrescription_1002 {get { return GetParameter(1002); }set { SetParameter(1002, value); }}
	/// <summary>PID: 1002  | Type: read</summary>
	public System.Object Streamsdrescription {get { return GetParameter(1002); }set { SetParameter(1002, value); }}
	/// <summary>PID: 1003  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsoctetscounter_1003 {get { return GetParameter(1003); }set { SetParameter(1003, value); }}
	/// <summary>PID: 1003  | Type: read</summary>
	public System.Object Streamsoctetscounter {get { return GetParameter(1003); }set { SetParameter(1003, value); }}
	/// <summary>PID: 1004  | Type: read | EXCEPTIONS: N/A = -1</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrate_1004 {get { return GetParameter(1004); }set { SetParameter(1004, value); }}
	/// <summary>PID: 1004  | Type: read | EXCEPTIONS: N/A = -1</summary>
	public System.Object Streamsbitrate {get { return GetParameter(1004); }set { SetParameter(1004, value); }}
	/// <summary>PID: 1005  | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitratedata_1005 {get { return GetParameter(1005); }set { SetParameter(1005, value); }}
	/// <summary>PID: 1005  | Type: read</summary>
	public System.Object Streamsbitratedata {get { return GetParameter(1005); }set { SetParameter(1005, value); }}
	public WriteParameters Write { get; set; }
	public ConcreteSLProtocolExt()
	{
		streams = new StreamsQActionTable(this, 1000, "streams");
		Write = new WriteParameters(this);
	}
}
/// <summary>IDX: 0</summary>
public class StreamsQActionTable : QActionTable, IEnumerable<StreamsQActionRow>
{
	public StreamsQActionTable(SLProtocol protocol, int tableId, string tableName) : base(protocol, tableId, tableName) { }
	IEnumerator IEnumerable.GetEnumerator() { return (IEnumerator) GetEnumerator(); }
	public IEnumerator<StreamsQActionRow> GetEnumerator() { return new QActionTableEnumerator<StreamsQActionRow>(this); }
}
/// <summary>IDX: 0</summary>
public class StreamsQActionRow : QActionTableRow
{
	/// <summary>PID: 1001 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsindex_1001 { get { if (base.Columns.ContainsKey(0)) { return base.Columns[0]; } else { return null; } } set { if (base.Columns.ContainsKey(0)) { base.Columns[0] = value; } else { base.Columns.Add(0, value); } } }
	/// <summary>PID: 1001 | Type: read</summary>
	public System.Object Streamsindex { get { if (base.Columns.ContainsKey(0)) { return base.Columns[0]; } else { return null; } } set { if (base.Columns.ContainsKey(0)) { base.Columns[0] = value; } else { base.Columns.Add(0, value); } } }
	/// <summary>PID: 1002 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsdrescription_1002 { get { if (base.Columns.ContainsKey(1)) { return base.Columns[1]; } else { return null; } } set { if (base.Columns.ContainsKey(1)) { base.Columns[1] = value; } else { base.Columns.Add(1, value); } } }
	/// <summary>PID: 1002 | Type: read</summary>
	public System.Object Streamsdrescription { get { if (base.Columns.ContainsKey(1)) { return base.Columns[1]; } else { return null; } } set { if (base.Columns.ContainsKey(1)) { base.Columns[1] = value; } else { base.Columns.Add(1, value); } } }
	/// <summary>PID: 1003 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsoctetscounter_1003 { get { if (base.Columns.ContainsKey(2)) { return base.Columns[2]; } else { return null; } } set { if (base.Columns.ContainsKey(2)) { base.Columns[2] = value; } else { base.Columns.Add(2, value); } } }
	/// <summary>PID: 1003 | Type: read</summary>
	public System.Object Streamsoctetscounter { get { if (base.Columns.ContainsKey(2)) { return base.Columns[2]; } else { return null; } } set { if (base.Columns.ContainsKey(2)) { base.Columns[2] = value; } else { base.Columns.Add(2, value); } } }
	/// <summary>PID: 1004 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitrate_1004 { get { if (base.Columns.ContainsKey(3)) { return base.Columns[3]; } else { return null; } } set { if (base.Columns.ContainsKey(3)) { base.Columns[3] = value; } else { base.Columns.Add(3, value); } } }
	/// <summary>PID: 1004 | Type: read</summary>
	public System.Object Streamsbitrate { get { if (base.Columns.ContainsKey(3)) { return base.Columns[3]; } else { return null; } } set { if (base.Columns.ContainsKey(3)) { base.Columns[3] = value; } else { base.Columns.Add(3, value); } } }
	/// <summary>PID: 1005 | Type: read</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public System.Object Streamsbitratedata_1005 { get { if (base.Columns.ContainsKey(4)) { return base.Columns[4]; } else { return null; } } set { if (base.Columns.ContainsKey(4)) { base.Columns[4] = value; } else { base.Columns.Add(4, value); } } }
	/// <summary>PID: 1005 | Type: read</summary>
	public System.Object Streamsbitratedata { get { if (base.Columns.ContainsKey(4)) { return base.Columns[4]; } else { return null; } } set { if (base.Columns.ContainsKey(4)) { base.Columns[4] = value; } else { base.Columns.Add(4, value); } } }
	public StreamsQActionRow() : base(0, 5) { }
	public StreamsQActionRow(System.Object[] oRow) : base(0, 5, oRow) { }
	public static implicit operator StreamsQActionRow(System.Object[] source) { return new StreamsQActionRow(source); }
	public static implicit operator System.Object[](StreamsQActionRow source) { return source.ToObjectArray(); }
}
}
