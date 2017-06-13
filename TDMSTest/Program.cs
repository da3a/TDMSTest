using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMSTest
{
    class Program
    {

        static string fileName = @"C:\Users\dawa\Downloads\01_04_2017-115521_Log_7.tdms";

        static void Main(string[] args)
        {

            Console.WriteLine("TDMS Test App");

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                using (var r = new Reader(fs))
                {
                    LoadMetadata(r);
                }
            };

        }

        static void LoadMetadata(Reader reader)
        {
            var segments = GetSegments(reader).ToList();
            

        }

        static IEnumerable<Segment>  GetSegments(Reader reader)
        {
            var segment = reader.ReadFirstSegment();

            while (segment != null)
            {
                yield return segment;
                segment = reader.ReadSegment(segment.NextSegmentOffset);
            }
        }
    }

}
