using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    /// <inheritdoc />
    public class Block : IDataBlock
    {
        long? size;

        /// <inheritdoc />
        public Cid Id { get; set; }

        /// <inheritdoc />
        public byte[] DataBytes { get; set; }

        /// <inheritdoc />
        public Stream DataStream
        {
            get
            {
                return new MemoryStream(DataBytes, false);
            }
        }

        /// <inheritdoc />
        public long Size
        {
            get
            {
                if (size.HasValue)
                {
                    return size.Value;
                }
                return DataBytes.Length;
            }
            set
            {
                size = value;
            }
        }
        
    }

}
