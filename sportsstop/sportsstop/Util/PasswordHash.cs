using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

//derived from https://github.com/aspnet/Identity/blob/master/src/Core/PasswordHasher.cs
//the original is more flexible in that it supports other random number generators and 
//multiple formats and algorithms. This one is hard coded to one format.

namespace sportsstop.Util
{
    static public class PasswordHash
    {
        /* ======================
         * HASHED PASSWORD FORMAT
         * ======================
         * 
         * PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.
         * Format: { prf (UInt32), iter count (UInt32), salt length (UInt32), salt, subkey }
         * (All UInt32s are stored big-endian.)
         */

        #region constants
        private const int ITERATION_COUNT = 10000;
        private const int SALT_SIZE = 128 / 8;
        private const int HASH_SIZE = 256 / 8;
        private const int ALGORITHM_OFFSET = 0;
        private const int ITERATION_SIZE_OFFSET = 4;
        private const int SALT_SIZE_OFFSET = 8;
        private const int HEADER_SIZE = 12;
        #endregion

        public static string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            return Convert.ToBase64String(createHash(password));
        }

        public static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            var success = false;

            if (hashedPassword == null)
                throw new ArgumentNullException(nameof(hashedPassword));

            if (providedPassword == null)
                throw new ArgumentNullException(nameof(providedPassword));

            var decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            if (decodedHashedPassword.Length > 0)
                success = verifyHash(decodedHashedPassword, providedPassword);

            return success;
        }

        private static byte[] createHash(string password)
        {
            var outputBytes = new byte[HEADER_SIZE + SALT_SIZE + HASH_SIZE];
            var salt = new byte[SALT_SIZE];

            RandomNumberGenerator.Create().GetBytes(salt, 0, SALT_SIZE);
            var subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, ITERATION_COUNT, HASH_SIZE);

            WriteNetworkByteOrder(outputBytes, ALGORITHM_OFFSET, (uint)KeyDerivationPrf.HMACSHA256);
            WriteNetworkByteOrder(outputBytes, ITERATION_SIZE_OFFSET, ITERATION_COUNT);
            WriteNetworkByteOrder(outputBytes, SALT_SIZE_OFFSET, SALT_SIZE);

            Buffer.BlockCopy(salt, 0, outputBytes, HEADER_SIZE, SALT_SIZE);
            Buffer.BlockCopy(subkey, 0, outputBytes, HEADER_SIZE + SALT_SIZE, HASH_SIZE);

            return outputBytes;
        }

        private static bool verifyHash(byte[] hashedPassword, string password)
        {
            var success = false;

            try
            {
                // Read header information
                var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, ALGORITHM_OFFSET);
                var iterCount = (int)ReadNetworkByteOrder(hashedPassword, ITERATION_SIZE_OFFSET);
                var saltLength = (int)ReadNetworkByteOrder(hashedPassword, SALT_SIZE_OFFSET);

                if (iterCount == ITERATION_COUNT &&
                    saltLength == SALT_SIZE &&
                    hashedPassword.Length == HEADER_SIZE + SALT_SIZE + HASH_SIZE)
                {
                    var salt = new byte[SALT_SIZE];
                    Buffer.BlockCopy(hashedPassword, HEADER_SIZE, salt, 0, SALT_SIZE);

                    var expectedSubkey = new byte[HASH_SIZE];
                    Buffer.BlockCopy(hashedPassword, HEADER_SIZE + SALT_SIZE, expectedSubkey, 0, HASH_SIZE);

                    // Hash the incoming password and verify it
                    var actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, HASH_SIZE);
                    success = ByteArraysEqual(actualSubkey, expectedSubkey);
                }
            }
            catch { }

            return success;
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset) => ((uint)(buffer[offset + 0]) << 24)
                | ((uint)(buffer[offset + 1]) << 16)
                | ((uint)(buffer[offset + 2]) << 8)
                | buffer[offset + 3];

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            var isEqual = true;

            if (a != null || b != null)
            {
                if (a != null && b != null && a.Length == b.Length)
                {
                    for (var i = 0; i < a.Length; i++)
                        isEqual &= (a[i] == b[i]);
                }
                else
                    isEqual = false;
            }

            return isEqual;
        }
    }
}