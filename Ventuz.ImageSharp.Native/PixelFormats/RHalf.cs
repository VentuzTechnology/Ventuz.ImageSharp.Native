using System.Numerics;
using System.Runtime.CompilerServices;

using SixLabors.ImageSharp.PixelFormats;

namespace Ventuz.ImageSharp.Native.PixelFormats;

/// <summary>
/// Packed pixel type containing a single 16 bit floating point value.
/// <para>
/// Ranges from [0, 0, 0, 0] to [1, 0, 0, 1] in vector form.
/// </para>
/// </summary>
public partial struct RHalf : IPixel<RHalf>, IPackedVector<ushort>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RHalf"/> struct.
    /// </summary>
    /// <param name="value">The single component value.</param>
    public RHalf(float value) => this.PackedValue = BitConverter.HalfToUInt16Bits((Half)value);

    /// <inheritdoc/>
    public ushort PackedValue { get; set; }

    /// <summary>
    /// Compares two <see cref="RHalf"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="RHalf"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="RHalf"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RHalf left, RHalf right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="RHalf"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="RHalf"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="RHalf"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RHalf left, RHalf right) => !left.Equals(right);

    /// <inheritdoc />
    public readonly PixelOperations<RHalf> CreatePixelOperations() => new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromScaledVector4(Vector4 vector) => this.PackedValue = BitConverter.HalfToUInt16Bits((Half)vector.X);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToScaledVector4() => new(this.ToSingle(), 0, 0, 1F);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromVector4(Vector4 vector) => this.PackedValue = BitConverter.HalfToUInt16Bits((Half)vector.X);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToVector4() => new(this.ToSingle(), 0, 0, 1F);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromArgb32(Argb32 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromBgr24(Bgr24 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromBgra32(Bgra32 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromAbgr32(Abgr32 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromBgra5551(Bgra5551 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromL8(L8 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromL16(L16 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromLa16(La16 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromLa32(La32 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromRgb24(Rgb24 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromRgba32(Rgba32 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ToRgba32(ref Rgba32 dest) => dest.FromScaledVector4(this.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromRgb48(Rgb48 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

    /// <summary>
    /// Expands the packed representation into a <see cref="float"/>.
    /// </summary>
    /// <returns>The <see cref="float"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ToSingle() => (float)BitConverter.UInt16BitsToHalf((ushort)this.PackedValue);

    /// <inheritdoc />
    public override readonly bool Equals(object? obj) => obj is RHalf other && this.Equals(other);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(RHalf other) => this.PackedValue.Equals(other.PackedValue);

    /// <inheritdoc />
    public override readonly string ToString() => FormattableString.Invariant($"RHalf({this.ToSingle():#0.##})");

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => this.PackedValue.GetHashCode();
}
