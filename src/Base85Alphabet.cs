﻿// <copyright file="Base85Alphabet.cs" company="Sedat Kapanoglu">
// Copyright (c) 2014-2019 Sedat Kapanoglu
// Licensed under Apache-2.0 License (see LICENSE.txt file for details)
// </copyright>

namespace SimpleBase
{
    using System.Threading;

    /// <summary>
    /// Base85 Alphabet
    /// </summary>
    public sealed class Base85Alphabet : EncodingAlphabet
    {
        private static Base85Alphabet z85;
        private static Base85Alphabet ascii85;

        /// <summary>
        /// Initializes a new instance of the <see cref="Base85Alphabet"/> class
        /// using custom settings.
        /// </summary>
        /// <param name="alphabet">Alphabet to use</param>
        /// <param name="allZeroShortcut">Character to substitute for all zeros</param>
        /// <param name="allSpaceShortcut">Character to substitute for all spaces</param>
        public Base85Alphabet(
            string alphabet,
            char? allZeroShortcut = null,
            char? allSpaceShortcut = null)
            : base(85, alphabet)
        {
            this.AllZeroShortcut = allZeroShortcut;
            this.AllSpaceShortcut = allSpaceShortcut;
        }

        /// <summary>
        /// Gets ZeroMQ Z85 Alphabet
        /// </summary>
        public static Base85Alphabet Z85 => LazyInitializer.EnsureInitialized(
            ref z85,
            () => new Base85Alphabet(
                "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#"));

        /// <summary>
        /// Gets Adobe Ascii85 Alphabet (each character is directly produced by raw value + 33)
        /// </summary>
        public static Base85Alphabet Ascii85 => LazyInitializer.EnsureInitialized(
            ref ascii85,
            () => new Base85Alphabet(
                "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstu",
                allZeroShortcut: 'z',
                allSpaceShortcut: 'y'));

        /// <summary>
        /// Gets the character to be used for "all zeros"
        /// </summary>
        public char? AllZeroShortcut { get; }

        /// <summary>
        /// Gets the character to be used for "all spaces"
        /// </summary>
        public char? AllSpaceShortcut { get; }

        /// <summary>
        /// Gets a value indicating whether the alphabet uses one of shortcut characters for all spaces
        /// or all zeros.
        /// </summary>
        public bool HasShortcut => AllSpaceShortcut.HasValue || AllZeroShortcut.HasValue;
    }
}
