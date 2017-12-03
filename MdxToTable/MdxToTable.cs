using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices.AdomdClient;

namespace AdomdMdx2017
{
  class Program
  {
    static void Main(string[] args)
    {
      if(args.Length < 3)
      {
        Console.WriteLine(usage());
        return;
      }

      string server = args[0];
      string db = args[1];
      List<Tuple<string, string>> input = new List<Tuple<string, string>>();
      for (int i = 2; i < args.Length; i++)
      {
        string filename = args[i];
        if (!File.Exists(filename) || string.Compare(Path.GetExtension(filename),".mdx", StringComparison.OrdinalIgnoreCase)!=0)
        {
          Console.WriteLine(">>  WARNING: File doesn't exist or not an MDX type file: {0}", filename);
          continue;
        }

        input.Add(new Tuple<string, string>(Path.GetFileNameWithoutExtension(filename), File.ReadAllText(filename)));
      }

      foreach (Tuple<string,string> tpl in input)
      {
        string mdx = tpl.Item2;
        string queryname = tpl.Item1;
        try
        {
          MdxQuery query = new MdxQuery(server, db, mdx, queryname);
          File.WriteAllText(queryname + ".txt", query[queryname]);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error processing: '{0}.mdx'", queryname);
          Console.Error.WriteLine("\t{0}", ex.Message);
          Console.Error.WriteLine("\t{0}", ex.Source);
          Console.Error.WriteLine("\t{0}", ex.StackTrace);
          if (ex.InnerException != null)
          {
            Console.Error.WriteLine("\t{0}", ex.InnerException.Message);
            Console.Error.WriteLine("\t{0}", ex.InnerException.Source);
            Console.Error.WriteLine("\t{0}", ex.InnerException.StackTrace);
          }
        }
      }
    }

    static string usage()
    {
      return
        "Usage:" + Environment.NewLine +
        Environment.NewLine +
        "\tMdxToTable <analysis-services-server> <analysis-services-databse> <mdx-query>.mdx [<mdx-query>.mdx ... <mdx-query>.mdx]" + Environment.NewLine +
        Environment.NewLine +
        "\t\t<analysis-services-server>    The name of the AS server or instance, ie. '<servername>[\\<instancename>]'" + Environment.NewLine +
        "\t\t<analysis-services-database>  The name of the AS database" + Environment.NewLine +
        "\t\t<mdx-query>                   The file name to an MDX query file" + Environment.NewLine;
    }
  }
}
