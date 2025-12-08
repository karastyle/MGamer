// LuaEncryptor.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// Lua文件AES加密解密工具（混淆版）
/// </summary>
public static class LuaEncryptor
{
    // 混淆的种子数据 - 看起来像是普通的配置常量
    private static readonly int[] _seedA = { 0x4D, 0x79, 0x4C, 0x75, 0x61 };
    private static readonly int[] _seedB = { 0x45, 0x6E, 0x63, 0x72, 0x79 };
    private static readonly int[] _seedC = { 0x70, 0x74, 0x4B, 0x65, 0x79 };
    private static readonly byte _xorMask = 0x33;
    private static readonly int _rotateValue = 7;

    // 延迟初始化的密钥
    private static byte[] _cachedKey = null;
    private static byte[] _cachedIV = null;
    private static readonly object _lockObj = new object();

    /// <summary>
    /// 生成AES密钥（32字节）
    /// </summary>
    private static byte[] GenerateKey()
    {
        if (_cachedKey != null) return _cachedKey;

        lock (_lockObj)
        {
            if (_cachedKey != null) return _cachedKey;

            byte[] key = new byte[32];
            
            // 使用多重混淆算法生成密钥
            int idx = 0;
            
            // 第一部分：基于种子A生成
            for (int i = 0; i < _seedA.Length; i++)
            {
                key[idx++] = (byte)((_seedA[i] ^ _xorMask) + (i * 3));
            }
            
            // 第二部分：基于种子B生成
            for (int i = 0; i < _seedB.Length; i++)
            {
                key[idx++] = (byte)((_seedB[i] ^ (_xorMask + i)) - 2);
            }
            
            // 第三部分：基于种子C生成
            for (int i = 0; i < _seedC.Length; i++)
            {
                key[idx++] = (byte)((_seedC[i] + _rotateValue) ^ (i & 0x0F));
            }
            
            // 第四部分：通过数学运算填充剩余字节
            while (idx < 32)
            {
                int baseVal = 0x30 + (idx % 26);
                key[idx] = (byte)((baseVal ^ (idx * 7)) + ((idx & 1) == 0 ? 1 : -1));
                idx++;
            }
            
            // 额外混淆：对整个密钥进行变换
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (byte)((key[i] << 1) | (key[i] >> 7));
            }
            
            _cachedKey = key;
            return key;
        }
    }

    /// <summary>
    /// 生成AES初始化向量（16字节）
    /// </summary>
    private static byte[] GenerateIV()
    {
        if (_cachedIV != null) return _cachedIV;

        lock (_lockObj)
        {
            if (_cachedIV != null) return _cachedIV;

            byte[] iv = new byte[16];
            
            // 使用不同的混淆策略生成IV
            int[] ivSeed = { 0x4D, 0x79, 0x4C, 0x75, 0x61, 0x45, 0x6E, 0x63 };
            
            for (int i = 0; i < 8; i++)
            {
                iv[i] = (byte)((ivSeed[i] ^ (_xorMask - i)) + 1);
            }
            
            // 通过反向算法填充后8字节
            for (int i = 8; i < 16; i++)
            {
                int reverseIdx = 15 - i;
                iv[i] = (byte)((0x72 - reverseIdx) ^ ((i * 5) & 0xFF));
            }
            
            // 额外变换
            for (int i = 0; i < iv.Length; i += 2)
            {
                iv[i] = (byte)(iv[i] ^ (i + 0x10));
            }
            
            _cachedIV = iv;
            return iv;
        }
    }

    /// <summary>
    /// 加密Lua文件内容
    /// </summary>
    public static byte[] EncryptLua(byte[] luaBytes)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey();
                aes.IV = GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(luaBytes, 0, luaBytes.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    return msEncrypt.ToArray();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LuaEncryptor] 加密失败: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 解密Lua文件内容
    /// </summary>
    public static byte[] DecryptLua(byte[] encryptedBytes)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey();
                aes.IV = GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream msPlain = new MemoryStream())
                {
                    csDecrypt.CopyTo(msPlain);
                    return msPlain.ToArray();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LuaEncryptor] 解密失败: {e.Message}");
            return null;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 使用luac编译Lua文件为字节码
    /// </summary>
    /// <param name="sourceLuaPath">源Lua文件路径</param>
    /// <param name="is64Bit">是否编译为64位字节码</param>
    private static byte[] CompileLuaWithLuac(string sourceLuaPath, bool is64Bit)
    {
        try
        {
            string projectRoot = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
            string bitFolder = is64Bit ? "64" : "32";
            string luacPath = Path.Combine(projectRoot, "Tools/lua", bitFolder, "luac53.exe");
            
            // 如果Tools目录下没有，尝试使用系统PATH中的luac
            if (!File.Exists(luacPath))
            {
                luacPath = "luac";
            }

            string tempOutputPath = Path.GetTempFileName();

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = luacPath,
                Arguments = $"-o \"{tempOutputPath}\" \"{sourceLuaPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(processInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[LuaEncryptor] luac{bitFolder}编译失败: {sourceLuaPath}\n错误: {error}");
                    if (File.Exists(tempOutputPath))
                        File.Delete(tempOutputPath);
                    return null;
                }
            }

            byte[] compiledBytes = File.ReadAllBytes(tempOutputPath);
            File.Delete(tempOutputPath);
            
            Debug.Log($"[LuaEncryptor] luac{bitFolder}编译成功: {Path.GetFileName(sourceLuaPath)} ({compiledBytes.Length} bytes)");
            return compiledBytes;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LuaEncryptor] luac编译异常: {sourceLuaPath}\n错误: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 加密Lua文件并保存（同时生成32位和64位版本）
    /// </summary>
    public static bool EncryptLuaFile(string sourceLuaPath, string destDir)
    {
        try
        {
            if (!File.Exists(sourceLuaPath))
            {
                Debug.LogError($"[LuaEncryptor] 源Lua文件不存在: {sourceLuaPath}");
                return false;
            }

            // 确保目标目录存在
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceLuaPath);
            bool success32 = false;
            bool success64 = false;

            // 编译并加密32位版本
            byte[] luaBytes32 = CompileLuaWithLuac(sourceLuaPath, false);
            if (luaBytes32 != null)
            {
                byte[] encryptedBytes32 = EncryptLua(luaBytes32);
                if (encryptedBytes32 != null)
                {
                    string destPath32 = Path.Combine(destDir, fileNameWithoutExt + "_32.txt");
                    File.WriteAllBytes(destPath32, encryptedBytes32);
                    success32 = true;
                    Debug.Log($"[LuaEncryptor] 32位加密成功: {destPath32}");
                }
            }

            // 编译并加密64位版本
            byte[] luaBytes64 = CompileLuaWithLuac(sourceLuaPath, true);
            if (luaBytes64 != null)
            {
                byte[] encryptedBytes64 = EncryptLua(luaBytes64);
                if (encryptedBytes64 != null)
                {
                    string destPath64 = Path.Combine(destDir, fileNameWithoutExt + "_64.txt");
                    File.WriteAllBytes(destPath64, encryptedBytes64);
                    success64 = true;
                    Debug.Log($"[LuaEncryptor] 64位加密成功: {destPath64}");
                }
            }

            return success32 && success64;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LuaEncryptor] 加密文件失败: {sourceLuaPath}, 错误: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 批量加密Lua目录（编辑器专用）
    /// </summary>
    public static int EncryptLuaDirectory(string sourceLuaDir, string destDir)
    {
        if (!Directory.Exists(sourceLuaDir))
        {
            Debug.LogError($"[LuaEncryptor] 源目录不存在: {sourceLuaDir}");
            return 0;
        }

        int encryptedCount = 0;

        // 获取所有lua文件
        string[] luaFiles = Directory.GetFiles(sourceLuaDir, "*.lua", SearchOption.AllDirectories);

        foreach (string luaFile in luaFiles)
        {
            // 计算相对路径
            string relativePath = luaFile.Substring(sourceLuaDir.Length + 1);
            
            // 保持目录结构，但文件名会在EncryptLuaFile中处理为_32.txt和_64.txt
            string relativeDir = Path.GetDirectoryName(relativePath);
            string targetDir = string.IsNullOrEmpty(relativeDir) 
                ? destDir 
                : Path.Combine(destDir, relativeDir);

            if (EncryptLuaFile(luaFile, targetDir))
            {
                encryptedCount++;
            }
        }

        Debug.Log($"[LuaEncryptor] 加密完成，共加密 {encryptedCount} 个Lua文件（每个文件生成32位和64位两个版本）");
        return encryptedCount;
    }

    /// <summary>
    /// 调试方法：打印生成的密钥（仅用于开发测试）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DebugPrintKeys()
    {
        byte[] key = GenerateKey();
        byte[] iv = GenerateIV();
        
        Debug.Log($"[LuaEncryptor] Key: {BitConverter.ToString(key)}");
        Debug.Log($"[LuaEncryptor] IV: {BitConverter.ToString(iv)}");
    }
#endif
}