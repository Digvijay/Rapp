using System;
using System.Text;
using FluentAssertions;
using Xunit;
using Rapp;

namespace Rapp.Tests
{
    [RappGhost]
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public double Score { get; set; }
    }

    [RappGhost]
    public class Complex
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Description { get; set; } // For unicode
        public long Timestamp { get; set; }
    }

    public class GhostTests
    {
        [Fact]
        public void GhostReader_MultipleStrings_CorrectOffsets()
        {
            var data = new Complex 
            { 
                Id = 1, 
                FirstName = "Alice", 
                LastName = "Wonderland", 
                Timestamp = 123456789 
            };
            
            Span<byte> buffer = stackalloc byte[512];
            int written = data.WriteTo(buffer);
            
            var ghost = new ComplexGhost(buffer.Slice(0, written));
            
            ghost.Id.Should().Be(1);
            ghost.FirstNameString.Should().Be("Alice");
            ghost.LastNameString.Should().Be("Wonderland");
            ghost.Timestamp.Should().Be(123456789);
            
            // Description was null, should be empty
            ghost.Description.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void GhostReader_UnicodeAndEmojis_HandlesByteLengthCorrectly()
        {
            // "🚀" is 4 bytes in UTF-8, but 2 chars in C# (surrogate pair)
            // "Rapp" is 4 bytes
            var text = "Rapp 🚀 Ghost"; 
            
            var data = new Complex 
            { 
                Description = text 
            };
            
            Span<byte> buffer = stackalloc byte[512];
            int written = data.WriteTo(buffer);
            
            var ghost = new ComplexGhost(buffer.Slice(0, written));
            
            // Verify full string roundtrip
            ghost.DescriptionString.Should().Be(text);
            
            // Verify raw byte length (Rapp=4 + space=1 + Rocket=4 + space=1 + Ghost=5 = 15 bytes)
            ghost.Description.Length.Should().Be(Encoding.UTF8.GetByteCount(text));
        }

        [Fact]
        public void GhostReader_EmptyStrings_ZeroLength()
        {
            var data = new Complex { FirstName = "", LastName = "" };
            Span<byte> buffer = stackalloc byte[256];
            int written = data.WriteTo(buffer);
            
            var ghost = new ComplexGhost(buffer.Slice(0, written));
            
            ghost.FirstNameString.Should().BeEmpty();
            ghost.LastNameString.Should().BeEmpty();
            ghost.FirstName.Length.Should().Be(0);
        }

        [Fact]
        public void GhostReader_ReadWrite_Success()
        {
            var user = new User
            {
                Id = 12345,
                Name = "RappGhost",
                IsActive = true,
                Score = 99.9
            };

            Span<byte> buffer = stackalloc byte[256];
            int written = user.WriteTo(buffer);

            var ghost = new UserGhost(buffer.Slice(0, written));

            ghost.Id.Should().Be(12345);
            ghost.IsActive.Should().BeTrue();
            ghost.Score.Should().Be(99.9);
            ghost.NameString.Should().Be("RappGhost");
        }

        [Fact]
        public void GhostReader_VariableLength_Check()
        {
            var user = new User { Id = 1, Name = "A very long string for testing", IsActive = false, Score = 1.0 };
            Span<byte> buffer = new byte[1024];
            int written = user.WriteTo(buffer);

            var ghost = new UserGhost(buffer.Slice(0, written));
            ghost.NameString.Should().Be("A very long string for testing");
        }

        [Fact]
        public void GhostReader_NullString_Check()
        {
            var user = new User { Id = 2, Name = null, IsActive = true, Score = 0.0 };
            Span<byte> buffer = new byte[128];
            int written = user.WriteTo(buffer);

            var ghost = new UserGhost(buffer.Slice(0, written));
            
            ghost.Name.IsEmpty.Should().BeTrue();
            ghost.NameString.Should().BeEmpty();
        }

        [Fact]
        public void GhostReader_ConvenienceMethods_Work()
        {
            var user = new User { Id = 5, Name = "Convenience", IsActive = true, Score = 5.5 };
            
            // 1. Easy Mode: ToBytes()
            byte[] bytes = user.ToBytes(); // codeql[cs/empty-collection] - False positive, ToBytes returns populated array
            
            // 2. Verify Size
            int expectedSize = 
                4 + // Id
                2 + // Name Ptr
                1 + // IsActive
                8 + // Score
                2 + // Name Length
                11; // "Convenience" bytes
            
            bytes.Length.Should().Be(expectedSize);
            user.ComputeSize().Should().Be(expectedSize);
            
            // 3. Read it back
            var ghost = new UserGhost(bytes);
            ghost.NameString.Should().Be("Convenience");
        }

        [Fact]
        public void GhostReader_ThrowsOnTooLargeString()
        {
            var hugeString = new string('A', 65536);
            var user = new User { Name = hugeString };
            
            var buffer = new byte[70000];

            // Use simple try-catch to avoid any FluentAssertions complexity with wildcards 
            // or lambda capture issues if any remained (though strictly User is a class, so lambda is fine for User, but consistency is good).
            Action act = () => user.WriteTo(buffer);
            
            act.Should().Throw<InvalidOperationException>()
               .Where(e => e.Message.Contains("String length"));
        }

        [Fact]
        public void GhostReader_MultipleStrings_OverflowsBufferLimit()
        {
            // We need to push the _tailCursor past 65535.
            // 1. FirstName: 30000 chars (~30002 bytes). Tail ~30020.
            // 2. LastName:  30000 chars (~30002 bytes). Tail ~60040.
            // 3. Description: 6000 chars. 
            //    Start of Description write is 60040. OK.
            //    Tail becomes 66040.
            // 4. We need a FOURTH string to fail? 
            //    ComplexGhost has 3 strings: FirstName, LastName, Description.
            //    Wait, we check `if (_tailCursor > ushort.MaxValue)` at START of write.
            
            // If we want Description (3rd string) to fail, we need [1] + [2] > 65535.
            // Let's make [1] = 40000. [2] = 30000.
            // [1] writes 40000+. Tail -> 40000.
            // [2] writes 30000+. Start is 40000 (OK). Tail -> 70000.
            // [3] Description. Start is 70000. FAIL!
            
            var big1 = new string('X', 40000);
            var big2 = new string('Y', 30000); // Only needs to be large enough to push tail
            
            var complex = new Complex 
            { 
                FirstName = big1, 
                LastName = big2,
                Description = "This should fail to write because ptr is > 65535"
            };
            
            var buffer = new byte[80000]; // Plenty of space
            
            // Should fail because Description's start offset (pointer) would be > 65535
            Action act = () => complex.WriteTo(buffer);
            
            act.Should().Throw<InvalidOperationException>()
                .Where(e => e.Message.Contains("Buffer offset"));
        }

        [Fact]
        public void GhostReader_Read_BufferTooSmall_Throws()
        {
             var user = new User { Id = 1, Score = 1.0 };
             byte[] validBytes = user.ToBytes();
             
             // Create a truncated view
             // We cannot capture 'ghost' (ref struct) in a lambda, so we test differently.
             var truncatedSpan = validBytes.AsSpan(0, 5);
             var ghost = new UserGhost(truncatedSpan);
             
             // Accessing ID (Offset 0, Size 4) -> Should work (0..4 fits in 5)
             _ = ghost.Id;
             
             // Manually check for ArgumentOutOfRangeException since we can't use lambdas with ref structs
             bool scoreThrow = false;
             try { _ = ghost.Score; }
             catch (ArgumentOutOfRangeException) { scoreThrow = true; }
             scoreThrow.Should().BeTrue("accessing Score beyond buffer should throw");

             bool nameThrow = false;
             try { _ = ghost.Name; }
             catch (ArgumentOutOfRangeException) { nameThrow = true; }
             nameThrow.Should().BeTrue("accessing Name pointer beyond buffer should throw");
        }
    }
}
