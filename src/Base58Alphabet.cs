﻿// <copyright file="Base58Alphabet.cs" company="Sedat Kapanoglu">
// Copyright (c) 2014-2019 Sedat Kapanoglu
// Licensed under Apache-2.0 License (see LICENSE.txt file for details)
// </copyright>

namespace SimpleBase
{
    using System.Threading;

    /// <summary>
    /// Base58 alphabet
    /// </summary>
    public sealed class Base58Alphabet : EncodingAlphabet
    {
        private static Base58Alphabet bitcoin;
        private static Base58Alphabet ripple;
        private static Base58Alphabet flickr;

        /// <summary>
        /// Initializes a new instance of the <see cref="Base58Alphabet"/> class
        /// using a custom alphabet.
        /// </summary>
        /// <param name="alphabet">Alphabet to use</param>
        public Base58Alphabet(string alphabet)
            : base(58, alphabet)
        {
        }

        /// <summary>
        /// Gets Bitcoin alphabet
        /// </summary>
        public static Base58Alphabet Bitcoin => LazyInitializer.EnsureInitialized(
            ref bitcoin,
            () => new Base58Alphabet("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"));

        /// <summary>
        /// Gets Base58 alphabet
        /// </summary>
        public static Base58Alphabet Ripple => LazyInitializer.EnsureInitialized(
            ref ripple,
            () => new Base58Alphabet("rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz"));

        /// <summary>
        /// Gets Flickr alphabet
        /// </summary>
        public static Base58Alphabet Flickr => LazyInitializer.EnsureInitialized(
            ref flickr,
            () => new Base58Alphabet("123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ"));
    }
}