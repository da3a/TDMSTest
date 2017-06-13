using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMSTest
{
    /// <summary>
    /// Reference : http://www.ni.com/white-paper/5696/en/
    /// </summary>
    public class Reader: IDisposable
    {
        private readonly BinaryReader reader;

        public Reader(Stream stream)
        {
            reader = new BinaryReader(stream, System.Text.Encoding.UTF8);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public Segment ReadFirstSegment()
        {
            return ReadSegment(0);
        }

        public Segment ReadSegment(long offset)
        {
            Console.WriteLine($"Reading segment at offset: {offset}");

            if (offset < 0 || offset >= reader.BaseStream.Length) return null;
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var leadin = new Segment() { Offset = offset, MetadataOffset = offset + Segment.Length };
            leadin.Identifier = Encoding.UTF8.GetString(reader.ReadBytes(4));
            var tableOfContentsMask = reader.ReadUInt32();
            leadin.TableOfContents = new TableOfContents
            {
                ContainsNewObjects = ((tableOfContentsMask >> 2) & 1) == 1,
                HasDaqMxData = ((tableOfContentsMask >> 7) & 1) == 1,
                HasMetaData = ((tableOfContentsMask >> 1) & 1) == 1,
                HasRawData = ((tableOfContentsMask >> 3) & 1) == 1,
                NumbersAreBigEndian = ((tableOfContentsMask >> 6) & 1) == 1,
                RawDataIsInterleaved = ((tableOfContentsMask >> 5) & 1) == 1
            };
            leadin.Version = reader.ReadUInt32();
            Func<long, long> resetWhenEol = x => x < reader.BaseStream.Length ? x : -1;
            var remainingSegmentLength = reader.ReadInt64();
            leadin.NextSegmentOffset = resetWhenEol(offset + remainingSegmentLength + Segment.Length);
            var metatdataLength = reader.ReadInt64();
            leadin.RawDataOffset = offset + metatdataLength +  Segment.Length;
            return leadin;
        }
 
    }

    public class Segment
    {
        public const long Length = 28;
        public long Offset { get; set; }
        public long MetadataOffset { get; set; }
        public long RawDataOffset { get; set; }
        public long NextSegmentOffset { get; set; }
        public string Identifier { get; set; }
        public TableOfContents TableOfContents { get; set; }
        public uint Version { get; set; }
    }

    public class TableOfContents
    {
        public bool HasMetaData { get; set; }
        public bool HasRawData { get; set; }
        public bool HasDaqMxData { get; set; }
        public bool RawDataIsInterleaved { get; set; }
        public bool NumbersAreBigEndian { get; set; }
        public bool ContainsNewObjects { get; set; }
    }
}
