using System;
using System.Buffers.Binary;
using System.Text;

namespace Rapp
{
    /// <summary>
    /// Helper struct for writing data in the Rapp Ghost format (Fixed Head + Variable Tail).
    /// </summary>
    public ref struct RappWriter
    {
        private readonly Span<byte> _buffer;
        private int _headCursor; // Tracks fixed fields
        private int _tailCursor; // Tracks variable heap

        /// <summary>
        /// Initializes a new instance of the <see cref="RappWriter"/> struct.
        /// </summary>
        /// <param name="buffer">The target buffer to write to.</param>
        /// <param name="headSize">The pre-calculated size of the fixed "Head" region.</param>
        public RappWriter(Span<byte> buffer, int headSize)
        {
            _buffer = buffer;
            _headCursor = 0;
            _tailCursor = headSize; // Tail starts where Head ends
        }

        // --- PRIMITIVES (Write to Head) ---

        /// <summary>
        /// Writes a 32-bit integer to the fixed head region.
        /// </summary>
        public void WriteInt32(int value)
        {
            // Write value directly to the current fixed slot
            BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_headCursor), value);
            _headCursor += 4;
        }

        /// <summary>
        /// Writes a 64-bit integer to the fixed head region.
        /// </summary>
        public void WriteInt64(long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_headCursor), value);
            _headCursor += 8;
        }

        /// <summary>
        /// Writes a 16-bit integer to the fixed head region.
        /// </summary>
        public void WriteInt16(short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(_headCursor), value);
            _headCursor += 2;
        }
        
        /// <summary>
        /// Writes a byte to the fixed head region.
        /// </summary>
        public void WriteByte(byte value)
        {
            _buffer[_headCursor] = value;
            _headCursor += 1;
        }

        /// <summary>
        /// Writes a boolean to the fixed head region.
        /// </summary>
        public void WriteBool(bool value)
        {
            _buffer[_headCursor] = value ? (byte)1 : (byte)0;
            _headCursor += 1;
        }
        
        /// <summary>
        /// Writes a double-precision floating-point number to the fixed head region.
        /// </summary>
        public void WriteDouble(double value)
        {
            long longVal = BitConverter.DoubleToInt64Bits(value);
            BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(_headCursor), longVal);
            _headCursor += 8;
        }
        
        /// <summary>
        /// Writes a single-precision floating-point number to the fixed head region.
        /// </summary>
        public void WriteSingle(float value) // float
        {
             int intVal = BitConverter.SingleToInt32Bits(value);
             BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(_headCursor), intVal);
             _headCursor += 4;
        }

        // --- VARIABLE DATA (Write to Tail, Point from Head) ---

        /// <summary>
        /// Writes a string to the variable tail region and updates the pointer in the head region.
        /// Throws <see cref="InvalidOperationException"/> if buffer size or string length exceeds 65,535 bytes.
        /// </summary>
        public void WriteString(string? value)
        {
            if (value == null)
            {
                 // Write 0 pointer
                BinaryPrimitives.WriteUInt16LittleEndian(
                    _buffer.Slice(_headCursor), 
                    0
                );
                _headCursor += 2;
                return;
            }

            // Safety Check: BUFFER SIZE
            if (_tailCursor > ushort.MaxValue)
            {
                 throw new InvalidOperationException($"Rapp Ghost Reader limit exceeded: Buffer offset {_tailCursor} > 65535.");
            }

            // 1. POINTER: Write the current Tail position into the Head
            BinaryPrimitives.WriteUInt16LittleEndian(
                _buffer.Slice(_headCursor), 
                (ushort)_tailCursor
            );
            _headCursor += 2; 

            // 2. LENGTH: Write the string byte count at the start of the Tail
            int byteCount = Encoding.UTF8.GetByteCount(value);
            
            // Safety Check: STRING SIZE
            if (byteCount > ushort.MaxValue)
            {
                throw new InvalidOperationException($"Rapp Ghost Reader limit exceeded: String length {byteCount} > 65535 bytes.");
            }

            BinaryPrimitives.WriteUInt16LittleEndian(
                _buffer.Slice(_tailCursor), 
                (ushort)byteCount
            );
            
            // 3. DATA: Write the actual characters after the length
            Encoding.UTF8.GetBytes(value, _buffer.Slice(_tailCursor + 2));

            // 4. UPDATE: Move the Tail cursor forward
            _tailCursor += (2 + byteCount);
        }
        
        /// <summary>
        /// Gets the total number of bytes written to the buffer.
        /// </summary>
        public int Length => _tailCursor; 
    }
}
