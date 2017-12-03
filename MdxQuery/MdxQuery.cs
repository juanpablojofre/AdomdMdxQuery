using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices.AdomdClient;

namespace AdomdMdx2017
{
  public class MdxQuery
  {
    private AdomdConnection cnx;
    private Dictionary<string, CellSet> cellsets = new Dictionary<string, CellSet>();
    private Dictionary<string, string> queries = new Dictionary<string, string>();
    private Dictionary<string, string> tables = new Dictionary<string, string>();
    private Dictionary<string, Dictionary<int, string[]>> axesheadercaptions = new Dictionary<string, Dictionary<int, string[]>>();

    public IReadOnlyDictionary<string, CellSet> CellSets
    {
      get { return (IReadOnlyDictionary<string, CellSet>)cellsets; }
    }

    public IReadOnlyDictionary<string, string> Queries
    {
      get { return (IReadOnlyDictionary<string, string>)queries; }
    }

    public string this[string queryname]
    {
      get { return tables[queryname]; }
    }

    public MdxQuery(string instance, string database, string query, string queryname = "default")
    {
      cnx = new AdomdConnection(string.Format("Data Source={0};Catalog={1}", instance, database));
      cnx.Open();
      AddQuery(query, queryname, loadtable: true);
    }

    public CellSet AddQuery(string query, string queryname, bool loadtable)
    {
      queries[queryname] = query;
      cellsets[queryname] = Query(query);
      if(loadtable) tables[queryname] = GetSimpleHeadersTable(queryname);
      return cellsets[queryname];
    }

    public CellSet Query(string query)
    {
      AdomdCommand cmd = cnx.CreateCommand();
      cmd.CommandText = query;
      return cmd.ExecuteCellSet();
    }

    public void DeleteQuery(string queryname)
    {
      if (cellsets.ContainsKey(queryname))
      {
        cellsets.Remove(queryname);
        queries.Remove(queryname);
        if (tables.ContainsKey(queryname)) tables.Remove(queryname);
      }
    }

    public string GetSimpleHeadersTable(string queryname)
    {
      if (!queries.ContainsKey(queryname)) throw new ArgumentException(nameof(queryname));
      StringBuilder results = new StringBuilder();
      CellSet cs = cellsets[queryname];

      // Create Table Headers
      List<string> axesheaders = new List<string>();
      for (int i = cs.Axes.Count-1; i > 0; i--)
      {
        axesheaders.Add(cs.Axes[i].Name);
      }

      results.AppendLine(string.Join("\t", axesheaders.Concat(GetColumnHeaders(queryname))));

      // Add data to table, row by row
      int cellindex = 0;
      int datacolumns = cs.Axes[0].Positions.Count;
      int totalcells = cs.Cells.Count;
      while (cellindex < totalcells)
      {
        List<string> rowdata = new List<string>();
        for (int i = cs.Axes.Count - 1; i > 0; i--)
        {
          rowdata.Add(GetAxisHeaderCaption(queryname,i,cellindex));
        }

        for (int i = 0; i < datacolumns; i++)
        {
            rowdata.Add(cs.Cells[cellindex + i].Value != null ? cs.Cells[cellindex + i].Value.ToString() : string.Empty);
        }

        results.AppendLine(string.Join("\t",rowdata));
        cellindex += datacolumns;
      }

      return results.ToString();
    }

    public List<string> GetColumnHeaders(string queryname)
    {
      List<string> columnheaders = new List<string>();
      CellSet cs = cellsets[queryname];
      foreach (Position position in cs.Axes[0].Positions)
      {
        StringBuilder columnheader = new StringBuilder();
        foreach (Member member in position.Members)
        {
          columnheader.AppendFormat("{0} | ", member.Caption);
        }

        columnheader.Remove(columnheader.Length - 3, 3);
        columnheaders.Add(columnheader.ToString());
      }

      return columnheaders;
    }

    public string GetAxisHeaderCaption(string queryname, int axis, int cellindex)
    {
      if(!queries.ContainsKey(queryname)) throw new ArgumentException(nameof(queryname));

      CellSet cs = cellsets[queryname];
      AxisCollection axes = cs.Axes;
      if (axis < 0 || axis >= axes.Count) throw new ArgumentOutOfRangeException(nameof(axis));
      if (cellindex < 0 || cellindex >= cs.Cells.Count) throw new ArgumentOutOfRangeException(nameof(cellindex));

      if (!axesheadercaptions.ContainsKey(queryname))
      { 
        axesheadercaptions.Add(
          queryname, 
          new Dictionary<int, string[]>()
        );
      }

      if (!axesheadercaptions[queryname].ContainsKey(axis))
      {
        axesheadercaptions[queryname].Add(axis, new string[axes[axis].Positions.Count]);
      }

      int tile = 1;
      for (int i = 0; i < axis; i++) tile *= axes[i].Positions.Count;
      int axisheaderindex = (cellindex / tile) % axes[axis].Positions.Count;
      try
      {
        if (axesheadercaptions[queryname][axis][axisheaderindex] == null)
        {
          axesheadercaptions[queryname][axis][axisheaderindex] = string.Join(" | ", axes[axis].Positions[axisheaderindex].Members.Cast<Member>().Select(m => m.Caption));
        }
      }
      catch (Exception ex)
      {

        throw ex;
      }

      return axesheadercaptions[queryname][axis][axisheaderindex];
    }
  }
}
