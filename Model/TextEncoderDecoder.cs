using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketLibNetStandard.Model
{
    public static class TextEncoderDecoder
    {
        private static char INDENTYFIER = '§';
        private static int KEY_LENGTH = 5;
        private static int KEY_MOD = 4;
        private static string key = "xYz;3hHg%2";

        // Encoder
        public static string Encode(string input)
        {
            // Reverse Input
            input = Reverse(input);

            // Salt string until last char then identifier
            input = Salting(input, key);

            // Create Encoding Based on Formula
            input = EncoderDecoder(input, key, true);

            // Put it all together
            input = Concat(input, key);

            return input;
        }

        // Decoder
        public static string Decode(string input)
        {
            // First decode
            input = EncoderDecoder(input, key, false).Substring(0, (input.Length - KEY_LENGTH) - 1);

            // Then salted this
            input = DeSalting(input);

            // Last Reverse again
            input = Reverse(input);

            return input;
        }

        private static string Reverse(string input)
        {
            string outPut = "";
            for (int i = input.Length - 1; i >= 0; i--)
            {
                outPut += input[i];
            }
            return outPut;
        }

        private static string Salting(string input, string salt)
        {
            string salted = "";
            if (salt.Length <= KEY_LENGTH)
            {
                salt = salt;
            }
            else
            {
                salt = salt.Substring(0, salt.Length - KEY_LENGTH);
            }

            int j = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (j < salt.Length)
                {
                    salted += input[i].ToString() + salt[j].ToString();
                    j++;
                }
                else
                    salted += input[i];
            }
            return salted + salt.Length.ToString();
        }

        private static string DeSalting(string input)
        {
            int saltLength = (int)Convert.ToInt16(input[input.Length - 1].ToString()); // Get the latest in the char array wich is the saltLength
            int saltindex = 0;
            string desalted = "";

            for (int i = 0; i < input.Length; i++)
            {

                if ((i % 2 != 0) && saltindex < saltLength)
                {
                    saltindex++;
                }
                else
                    desalted += input[i];

            }
            return desalted.Substring(0, desalted.Length - 1); // Get ridd of the latest number before reversal
        }

        // Di = Wi ^ KC(index++ % KCl)
        private static string EncoderDecoder(string input, string key, bool encode)
        {
            int calculated = 0;
            int index = 0;
            key = KeyPart(key);

            byte[] bytesInput = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] bytesKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] encoded = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (index >= key.Length)
                    index = 0;

                if (encode)
                    calculated = (int)bytesInput[i] ^ ((int)bytesKey[index] % KEY_MOD); // Encoder
                else
                    calculated = ((int)bytesKey[index] % KEY_MOD) ^ (int)bytesInput[i]; // Decoder

                encoded[i] = (byte)calculated;

                index++;
            }

            return System.Text.Encoding.UTF8.GetString(encoded);
        }


        private static string Concat(string input, string key)
        {
            return input + INDENTYFIER + KeyPart(key);
        }

        private static string KeyPart(string key)
        {
            return key.Substring(KEY_LENGTH, KEY_LENGTH);
        }
    }
}

