using System.IO;

namespace Card_Library_FM_WPF
{
    public class ReadMemCard
    {
        const int LIBRARY_BYTE_SIZE = 91;
        const int MEMCARD_BLOCK_SIZE = 0x2000;
        const int FRAME_OFFSET = 0xCBC;
        const int OFFSET = MEMCARD_BLOCK_SIZE + FRAME_OFFSET;
        const int MIN_MEMCARD_SIZE = 128 * 1024;
        const int LARGE_MEMCARD_SIZE = 256 * 1024;

        static void PrintHelp(string executableName)
        {
            Console.WriteLine($"Usage: {executableName} (options) [memory card image] [output file]");
            Console.WriteLine("\nOptions:\n--help\t\tPrints this help text\n");
            Console.WriteLine("--block [n]\tRead the nth block on the memory card. Default: 1. Values other than 1 are");
        }

        public static List<int>? Read(MemoryStream file)
        {
            int offset = OFFSET;

            byte[] buf = new byte[LIBRARY_BYTE_SIZE];


            file.Seek(0, SeekOrigin.End);
            int size = (int)file.Position;
            if (size < MIN_MEMCARD_SIZE)
            {
                Console.WriteLine("Memory card image is too small. Memory cards must be at least 128kb.");
                return null;
            }
            if (size != MIN_MEMCARD_SIZE && size != LARGE_MEMCARD_SIZE)
            {
                Console.WriteLine("Memory card image size is not an expected value. Memory cards should be 128kb or 256kb. Results may be inaccurate.\n");
            }
            file.Seek(offset, SeekOrigin.Begin);
            int countRead = file.Read(buf, 0, LIBRARY_BYTE_SIZE);
            if (countRead != LIBRARY_BYTE_SIZE)
            {
                Console.WriteLine("Unexpected number of elements in file stream. Results may be inaccurate.\n");
            }


            var cards = new List<int>();
            int card = 1;
            int total = 0;
            for (uint index = 0; index < LIBRARY_BYTE_SIZE; index++)
            {
                byte bitfield = buf[index];
                for (uint bit = 1; bit <= 8; bit++)
                {
                    if (card == 1) // the top bits of the 1st library byte are ignored
                    {
                        bit++;
                    }
                    byte currentBit;
                    Byte.TryParse((8 - bit).ToString(), out currentBit);
                    if ((bitfield >> currentBit & 0x01) != 0)
                    {
                        cards.Add(card);
                        total++;
                    }
                    card++;
                }
            }
            Console.WriteLine($"Total cards: {total}");
            return cards;
        }
    }
}
