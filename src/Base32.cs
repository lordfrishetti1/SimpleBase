﻿// <copyright file="Base32.cs" company="Sedat Kapanoglu">
// Copyright (c) 2014-2019 Sedat Kapanoglu
// Licensed under Apache-2.0 License (see LICENSE.txt file for details)
// </copyright>

namespace SimpleBase
{
    using System;
    using System.IO;

    /// <summary>
    /// Base32 encoding/decoding functions
    /// </summary>
    public sealed class Base32
    {
        /// <summary>
        /// Douglas Crockford's Base32 flavor with substitution characters.
        /// </summary>
        public static readonly Base32 Crockford = new Base32(Base32Alphabet.Crockford);

        /// <summary>
        /// RFC 4648 variant of Base32 coder
        /// </summary>
        public static readonly Base32 Rfc4648 = new Base32(Base32Alphabet.Rfc4648);

        /// <summary>
        /// Extended Hex variant of Base32 coder
        /// </summary>
        /// <remarks>Also from RFC 4648</remarks>
        public static readonly Base32 ExtendedHex = new Base32(Base32Alphabet.ExtendedHex);

        /// <summary>
        /// z-base-32 variant of Base32 coder
        /// </summary>
        /// <remarks>This variant is used in Mnet, ZRTP and Tahoe-LAFS</remarks>
        public static readonly Base32 ZBase32 = new Base32(Base32Alphabet.ZBase32);

        /// <summary>
        /// Geohash variant of Base32 coder
        /// </summary>
        public static readonly Base32 Geohash = new Base32(Base32Alphabet.Geohash);

        private const int bitsPerByte = 8;
        private const int bitsPerChar = 5;
        private const char paddingChar = '=';

        private readonly Base32Alphabet alphabet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Base32"/> class with a
        /// custom alphabet.
        /// </summary>
        /// <param name="alphabet">Alphabet to use</param>
        public Base32(Base32Alphabet alphabet)
        {
            this.alphabet = alphabet;
        }

        /// <summary>
        /// Encode a byte array into a Base32 string
        /// </summary>
        /// <param name="bytes">Buffer to be encoded</param>
        /// <param name="padding">Append padding characters in the output</param>
        /// <returns>Encoded string</returns>
        public unsafe string Encode(ReadOnlySpan<byte> bytes, bool padding)
        {
            int bytesLen = bytes.Length;
            if (bytesLen == 0)
            {
                return string.Empty;
            }

            // we are ok with slightly larger buffer since the output string will always
            // have the exact length of the output produced.
            int outputLen = (((bytesLen - 1) / bitsPerChar) + 1) * bitsPerByte;
            string output = new string('\0', outputLen);

            fixed (byte* inputPtr = bytes)
            fixed (char* outputPtr = output)
            fixed (char* tablePtr = this.alphabet.Value)
            {
                char* pOutput = outputPtr;
                byte* pInput = inputPtr;
                byte* pEnd = pInput + bytesLen;

                for (int bitsLeft = bitsPerByte, currentByte = *pInput, outputPad = 0;  pInput != pEnd;)
                {
                    if (bitsLeft > bitsPerChar)
                    {
                        bitsLeft -= bitsPerChar;
                        outputPad = currentByte >> bitsLeft;
                        *pOutput++ = tablePtr[outputPad];
                        currentByte &= (1 << bitsLeft) - 1;
                    }

                    int nextBits = bitsPerChar - bitsLeft;
                    bitsLeft = bitsPerByte - nextBits;
                    outputPad = currentByte << nextBits;
                    if (++pInput != pEnd)
                    {
                        currentByte = *pInput;
                        outputPad |= currentByte >> bitsLeft;
                        currentByte &= (1 << bitsLeft) - 1;
                    }

                    *pOutput++ = tablePtr[outputPad];
                }

                if (padding)
                {
                    for (char* pOutputEnd = outputPtr + outputLen;  pOutput != pOutputEnd; pOutput++)
                    {
                        *pOutput = paddingChar;
                    }
                }

                int finalOutputLen = (int)(pOutput - outputPtr);
                if (finalOutputLen == outputLen)
                {
                    return output; // avoid unnecessary copying
                }

                return new string(outputPtr, 0, finalOutputLen);
            }
        }

        /// <summary>
        /// Decode a Base32 encoded string into a byte array.
        /// </summary>
        /// <param name="text">Encoded Base32 characters</param>
        /// <returns>Decoded byte array</returns>
        public Span<byte> Decode(string text)
        {
            Require.NotNull(text, nameof(text));
            return this.Decode(text.AsSpan());
        }

        /// <summary>
        /// Decode a Base32 encoded string into a byte array.
        /// </summary>
        /// <param name="text">Encoded Base32 string</param>
        /// <returns>Decoded byte array</returns>
        public unsafe Span<byte> Decode(ReadOnlySpan<char> text)
        {
            int textLen = text.Length;

            // ignore trailing padding chars and whitespace
            while (textLen > 0)
            {
                char c = text[textLen - 1];
                if (c != paddingChar && !char.IsWhiteSpace(text[textLen - 1]))
                {
                    break;
                }

                textLen--;
            }

            if (textLen == 0)
            {
                return Array.Empty<byte>();
            }

            int bitsLeft = bitsPerByte;
            int outputLen = textLen * bitsPerChar / bitsPerByte;
            var outputBuffer = new byte[outputLen];
            int outputPad = 0;
            var table = this.alphabet.ReverseLookupTable;

            fixed (byte* outputPtr = outputBuffer)
            fixed (char* inputPtr = text)
            {
                byte* pOutput = outputPtr;
                char* pInput = inputPtr;
                char* pEnd = inputPtr + textLen;
                while (pInput != pEnd)
                {
                    char c = *pInput++;
                    int b = table[c] - 1;
                    if (b < 0)
                    {
                        throw EncodingAlphabet.InvalidCharacter(c);
                    }

                    if (bitsLeft > bitsPerChar)
                    {
                        bitsLeft -= bitsPerChar;
                        outputPad |= b << bitsLeft;
                        continue;
                    }

                    int shiftBits = bitsPerChar - bitsLeft;
                    outputPad |= b >> shiftBits;
                    *pOutput++ = (byte)outputPad;
                    b &= (1 << shiftBits) - 1;
                    bitsLeft = bitsPerByte - shiftBits;
                    outputPad = b << bitsLeft;
                }
            }

            return outputBuffer;
        }

        /// <summary>
        /// Encode a binary stream to a Base32 text stream
        /// </summary>
        /// <param name="input">Input bytes</param>
        /// <param name="output">The writer the output is written to</param>
        /// <param name="padding">Whether to use padding at the end of the output</param>
        public void Encode(Stream input, TextWriter output, bool padding)
        {
            Require.NotNull(input, nameof(input));
            Require.NotNull(output, nameof(output));
            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];
            while (true)
            {
                int bytesRead = input.Read(buffer, 0, bufferSize);
                if (bytesRead < 1)
                {
                    break;
                }

                bool usePadding = bytesRead < bufferSize ? padding : false;
                var result = this.Encode(buffer.AsSpan(0, bytesRead), usePadding);
                output.Write(result);
            }
        }

        /// <summary>
        /// Decode a text stream into a binary stream
        /// </summary>
        /// <param name="input">TextReader open on the stream</param>
        /// <param name="output">Binary output stream</param>
        public void Decode(TextReader input, Stream output)
        {
            Require.NotNull(input, nameof(input));
            Require.NotNull(output, nameof(output));
            const int bufferSize = 6400;
            var buffer = new char[bufferSize];
            while (true)
            {
                int bytesRead = input.Read(buffer, 0, bufferSize);
                if (bytesRead < 1)
                {
                    break;
                }

                var result = this.Decode(buffer.AsSpan(0, bytesRead));
                output.Write(result.ToArray(), 0, result.Length);
            }
        }
    }
}