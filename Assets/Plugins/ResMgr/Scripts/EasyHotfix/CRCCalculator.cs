// CRCCalculator.cs
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EasyTools
{
    /// <summary>
    /// 优化的CRC计算工具类（支持性能分级）
    /// </summary>
    public static class CRCCalculator
    {
        private static readonly uint[] CRC_TABLE = GenerateCRCTable();

        private static uint[] GenerateCRCTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                        crc = (crc >> 1) ^ 0xEDB88320;
                    else
                        crc >>= 1;
                }
                table[i] = crc;
            }
            return table;
        }

        /// <summary>
        /// 异步流式CRC计算（在后台线程执行）
        /// </summary>
        public static async Task<uint> CalculateFileCRCAsync(string filePath)
        {
            return await Task.Run(() => CalculateFileCRCSync(filePath));
        }

        /// <summary>
        /// 同步流式CRC计算（用于后台线程）
        /// </summary>
        private static uint CalculateFileCRCSync(string filePath)
        {
            var config = PatchUpdater.GetCurrentConfig();
            uint crc = 0xFFFFFFFF;
            byte[] buffer = new byte[config.BufferSize];

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, config.BufferSize))
            {
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, config.BufferSize)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        crc = CRC_TABLE[(crc ^ buffer[i]) & 0xFF] ^ (crc >> 8);
                    }
                }
            }

            return ~crc;
        }

        /// <summary>
        /// 同步CRC计算（直接在当前线程执行，适用于小文件或不需要异步的场景）
        /// </summary>
        public static uint CalculateFileCRC(string filePath)
        {
            return CalculateFileCRCSync(filePath);
        }

        /// <summary>
        /// 批量并行计算CRC（根据性能配置限制并发数）
        /// </summary>
        public static async Task<Dictionary<string, uint>> CalculateMultipleCRCAsync(List<string> filePaths)
        {
            var config = PatchUpdater.GetCurrentConfig();
            var results = new Dictionary<string, uint>();
            var tasks = new List<Task>();
            var semaphore = new System.Threading.SemaphoreSlim(config.MaxConcurrency);

            foreach (var filePath in filePaths)
            {
                await semaphore.WaitAsync();
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        uint crc = await CalculateFileCRCAsync(filePath);
                        lock (results)
                        {
                            results[filePath] = crc;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            return results;
        }
    }
}