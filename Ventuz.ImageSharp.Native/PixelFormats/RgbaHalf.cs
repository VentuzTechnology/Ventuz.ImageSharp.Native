﻿/*
 * Ventuz.ImageSharp.Native
 * Copyright (c) 2024 Ventuz Technology <https://ventuz.com>
 *
 * This file is part of Ventuz.ImageSharp.Native
 *
 * Ventuz.ImageSharp.Native is free software: you can redistribute 
 * it and/or modify it under the terms of the GNU Lesser General 
 * Public License as published by the Free Software Foundation, 
 * either version 3 of the License, or (at your option) any later 
 * version.
 *
 * Ventuz.ImageSharp.Native is distributed in the hope that it will 
 * be useful, but WITHOUT ANY WARRANTY; without even the implied 
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Ventuz.ImageSharp.Native.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Numerics;
using System.Runtime.CompilerServices;

using SixLabors.ImageSharp.PixelFormats;

namespace Ventuz.ImageSharp.Native.PixelFormats;

/// <summary>
/// Packed pixel type containing four 16-bit floating-point values.
/// <para>
/// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
/// </para>
/// </summary>
public partial struct RgbaHalf : IPixel<RgbaHalf>, IPackedVector<ulong>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RgbaHalf"/> struct.
    /// </summary>
    /// <param name="x">The x-component.</param>
    /// <param name="y">The y-component.</param>
    /// <param name="z">The z-component.</param>
    /// <param name="w">The w-component.</param>
    public RgbaHalf(float x, float y, float z, float w)
        : this(new Vector4(x, y, z, w))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RgbaHalf"/> struct.
    /// </summary>
    /// <param name="vector">A vector containing the initial values for the components</param>
    public RgbaHalf(Vector4 vector) => this.PackedValue = Pack(ref vector);

    /// <inheritdoc/>
    public ulong PackedValue { get; set; }

    /// <summary>
    /// Compares two <see cref="RgbaHalf"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="RgbaHalf"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="RgbaHalf"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RgbaHalf left, RgbaHalf right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="RgbaHalf"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="RgbaHalf"/> on the left side of the operand.</param>
    /// <param name="right">The <see cref="RgbaHalf"/> on the right side of the operand.</param>
    /// <returns>
    /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RgbaHalf left, RgbaHalf right) => !left.Equals(right);

    /// <inheritdoc />
    public readonly PixelOperations<RgbaHalf> CreatePixelOperations() => new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToScaledVector4() => this.ToVector4();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromVector4(Vector4 vector) => this.PackedValue = Pack(ref vector);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToVector4() => new(
            (float)BitConverter.UInt16BitsToHalf((ushort)this.PackedValue),
            (float)BitConverter.UInt16BitsToHalf((ushort)(this.PackedValue >> 0x10)),
            (float)BitConverter.UInt16BitsToHalf((ushort)(this.PackedValue >> 0x20)),
            (float)BitConverter.UInt16BitsToHalf((ushort)(this.PackedValue >> 0x30)));

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

    /// <inheritdoc />
    public override readonly bool Equals(object? obj) => obj is RgbaHalf other && this.Equals(other);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(RgbaHalf other) => this.PackedValue.Equals(other.PackedValue);

    /// <inheritdoc />
    public override readonly string ToString()
    {
        var vector = this.ToVector4();
        return FormattableString.Invariant($"RgbaHalf({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => this.PackedValue.GetHashCode();

    /// <summary>
    /// Packs a <see cref="Vector4"/> into a <see cref="ulong"/>.
    /// </summary>
    /// <param name="vector">The vector containing the values to pack.</param>
    /// <returns>The <see cref="ulong"/> containing the packed values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Pack(ref Vector4 vector) =>
        BitConverter.HalfToUInt16Bits((Half)vector.X) |
        ((ulong)BitConverter.HalfToUInt16Bits((Half)vector.Y) << 0x10) |
        ((ulong)BitConverter.HalfToUInt16Bits((Half)vector.Z) << 0x20) |
        ((ulong)BitConverter.HalfToUInt16Bits((Half)vector.W) << 0x30);
}
