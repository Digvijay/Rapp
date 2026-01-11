# The Rapp Ghost Reader 👻

The **Ghost Reader** is a high-performance, Zero-Copy view over your raw data. It allows you to access properties of your objects directly from a binary buffer without deserializing the entire object or allocating a single byte of memory on the heap.

This pattern is ideal for high-throughput scenarios (gaming, trading, IoT) where garbage collection (GC) pauses are unacceptable.

## 🚀 Key Benefits

| Feature | Traditional (JSON/Protobuf) | Rapp Ghost Reader |
| :--- | :--- | :--- |
| **Allocation** | Creates new objects on Heap | **Zero Allocation** (Stack only) |
| **Parsing** | Parses full object on read | **Lazy / On-Demand** (O(1) access) |
| **GC Pressure** | High (creates garbage) | **None** (Invisible to GC) |
| **Speed** | Slow (Reflection + Allocations) | **Instant** (Pointer arithmetic) |

## 📦 How to Use

### 1. Opt-in with `[RappGhost]`

Mark your class or struct with the attribute. Rapp supports primitives (`int`, `bool`, `double`, etc.) and `string` (variable length).

```csharp
using Rapp;

[RappGhost]
public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public double Health { get; set; }
}
```

### 2. Write Data (Serialization)

Use the generated `.WriteTo()` extension method to serialize your object to a span of bytes.

```csharp
var player = new Player { Id = 1, Name = "Viking", IsActive = true, Health = 100.0 };
Span<byte> buffer = stackalloc byte[128]; // Stack allocate for speed!

int bytesWritten = player.WriteTo(buffer);
```

### 3. Read Data (The Ghost)

Wrap the buffer with the generated `*Ghost` struct. This looks and feels like your object, but it's just a view over the bytes.

```csharp
// Create the view (Zero allocation)
var ghost = new PlayerGhost(buffer.Slice(0, bytesWritten));

// READ properties instantly
if (ghost.IsActive) // Reads byte at offset X
{
    Console.WriteLine($"Player {ghost.Id} has {ghost.Health} health.");
}

// READ Strings
// Returns ReadOnlySpan<byte> to avoid allocation
ReadOnlySpan<byte> nameBytes = ghost.Name; 

// Or convert to string if you really need it (allocates)
Console.WriteLine(ghost.NameString); 
```

### 🎁 Convenience Methods

If you prefer ease of use over raw stack allocation, Rapp generates helper methods for you:

```csharp
var player = new Player { Name = "Convenient" };

// 1. Compute Exact Size
int size = player.ComputeSize();

// 2. Direct to Byte Array (Allocates, but easier)
byte[] data = player.ToBytes();
```

## 🧠 How It Works (The "Head/Tail" Layout)

Rapp uses a split binary layout to guarantee O(1) random access for fixed fields while supporting variable-length data (strings).

```text
[ ID (4b) ] [ Health (8b) ] [ Name Pointer (2b) ] ... [ Name Length (2b) ] [ "Viking" (bytes) ]
<---------------- HEAD (Fixed) ----------------->     <------------ TAIL (Variable) ---------->
```

1.  **Head Region**: Contains all fixed-size data (`int`, `double`, `bool`) and *pointers* (offsets) to variable data.
    *   This allows us to know exactly where `Id` is (Offset 0) without parsing anything else.
2.  **Tail Region**: Contains the variable-length data (strings).
    *   The "Pointer" in the Head tells the Ghost Reader exactly where to jump in the generic buffer to find the string.

## ⚠️ Constraints & Best Practices

1.  **Max String Size**: The current implementation uses `ushort` for pointers and lengths, limiting individual string lengths and the total buffer size to **64KB**. This is optimized for network packets (UDP/TCP).
2.  **Buffer Safety**: The Ghost Reader assumes the underlying buffer is valid for the lifetime of the struct. Since it is a `ref struct`, the compiler enforces that it cannot escape the stack frame of the buffer, providing memory safety.
3.  **Schema Evolution**: The Ghost Reader generates code based on *current* class definitions. If you change the class, you must recompile. It does not currently support backward/forward compatibility (schema versioning) like the main Rapp serializer.
