using System.Security.Cryptography;
using System;

namespace CMC.Presentation.Application.Helpers
{
    /*
     * return new RandomNumberGenerator().Next((int)Math.Pow(10, numberOfDigits - 1), (int)(Math.Pow(10, numberOfDigits) - 1)).ToString();
     */
    /// <summary>
    /// Random Number Genrator
    /// </summary>
    public class RandomNumberGenerator : Random
    {
        /// <summary>
        /// Define RNG Crypto Service Provider
        /// </summary>
        private RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        /// <summary>
        /// Buffer
        /// </summary>
        private byte[] uint32Buffer = new byte[4];
        /// <summary>
        /// Random Number Generator constructor 
        /// </summary>
        public RandomNumberGenerator() { }
        /// <summary>
        /// To get a random number between too values 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override Int32 Next(Int32 minValue, Int32 maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");
            if (minValue == maxValue) return minValue;
            Int64 diff = maxValue - minValue;
            while (true)
            {
                rngCryptoServiceProvider.GetBytes(uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(uint32Buffer, 0);
                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(minValue + (rand % diff));
                }
            }
        }

    }
}
