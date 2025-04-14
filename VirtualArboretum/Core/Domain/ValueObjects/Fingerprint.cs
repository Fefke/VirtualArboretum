using System.Buffers.Binary;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using VirtualArboretum.Core.Domain.Entities;

namespace VirtualArboretum.Core.Domain.ValueObjects;


/// <summary>
/// A Mycorrhiza is a symbiotic Mechanism between a fungus and a plants root system. <br/>
/// Does provide nutritional access to all plants connected to the Mycelium and its Hyphae.
/// </summary>
public sealed class Fingerprint
{
    public Guid Pattern { get; init; }

    public Fingerprint(Guid pattern)
    {
        Pattern = pattern;
    }

    public Fingerprint() : this(Guid.CreateVersion7())
    { }

    public static Fingerprint? TryCreate(string serializedFingerprint)
    {
        if (Guid.TryParse(serializedFingerprint, out var id))
        {
            return new Fingerprint(id);
        }
        return null;
    }


    public DateTimeOffset GetCreationDateTime()
    {
        // Extract 48-bit UNIX timestamp from .Pattern : Guid
        // This is a heckmeck of bit manipulation, but it works,
        // as it reverse engineers the .CreateVersion7() method.
        var version7Guid = Pattern;
        Span<byte> bytes = stackalloc byte[16];
        if (!version7Guid.TryWriteBytes(bytes))
        {
            // Should normally not happen with a valid Guid
            throw new ArgumentException(
                @"Could not read bytes from the Guid.", nameof(version7Guid)
                );
        }

        // Guid.TryWriteBytes() returns the bytes in RFC 4122 order.
        // On a little-endian system, the bytes of the internal fields _a, _b, _c
        // are written in reverse order to comply with the standard.
        // Example: Internal _a (int, LE) = 0x11223344 -> bytes[0..3] = 44 33 22 11

        // To get the *original* values of _a, _b, _c, as they were written by the
        // unsafe CreateVersion7 method, we need to interpret the bytes
        // explicitly as little-endian. BinaryPrimitives works well for this.

        int originalA = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(0, 4));
        short originalB = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(4, 2));
        short originalC = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(6, 2));
        byte originalD = bytes[8]; // The byte for _d is copied directly (no endianness issue)

        // ## Validation (based on the reconstructed values of _c and _d)
        // Check version in originalC (the reconstructed little-endian short)
        // The version is stored in the upper 4 bits. Should be 0x7.
        if ((originalC & 0xF000) != 0x7000)
        {
            throw new ArgumentException(
                @"Guid does not contain the Version 7 marker.", nameof(version7Guid)
                );
        }

        // Check variant in original_d (the reconstructed byte)
        // The variant is stored in the upper 2 bits. Should be 0b10 (corresponds to 0x80 mask).
        if ((originalD & 0xC0) != 0x80)
        {
            throw new ArgumentException(
                @"Guid does not contain the RFC 4122 variant marker.", nameof(version7Guid)
                );
        }

        // ## Timestamp Reconstruction
        // Bits 16-47 of the original timestamp are in the reconstructed originalA.
        // Bits 0-15 of the original timestamp are in the reconstructed originalB.

        // Convert to unsigned types before combining into long to avoid sign extension.
        long highPart = (uint)originalA;
        long lowPart = (ushort)originalB;

        // Combine the parts: Shift the high part left by 16 bits
        // and add the low part using bitwise OR.
        long timeMilliseconds = (highPart << 16) | lowPart;

        // Convert the milliseconds back to DateTimeOffset
        return DateTimeOffset.FromUnixTimeMilliseconds(timeMilliseconds);
    }

    public static ImmutableArray<Fingerprint> SortedFingerprintsChronologically(IEnumerable<Fingerprint> signatures)
    {
        return [
            ..signatures.OrderBy(x => x.Pattern)
        ];
    }


    private const string Alphanumericals = "VA0123456789BCDEFGHIJKLMNOPQRSTU";

    /// <summary>
    /// ~~Deterministicly~~ converts the Fingerprint to a alphanumeric string.<br/>
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
/*    public string ToAlphanumericString()
    {
        var output = new StringBuilder();
        output.Append(Alphanumericals[0]);  // V
        output.Append(Alphanumericals[1]);  // A

        var serialPattern = this.Pattern.ToByteArray();  // always 16 Byte

        if (serialPattern == null)
        {
            throw new ArgumentException("Fingerprint must be 16 bytes long.");
        }

        foreach (var oktett in serialPattern)
        {
            output.Append(Alphanumericals[oktett % Alphanumericals.Length]);
        }

        return output.ToString();
    }
*/
    public override string ToString()
    {
        return this.Pattern.ToString();
    }
}