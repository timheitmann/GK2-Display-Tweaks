using Mono.Cecil;

namespace Gk2UltrawidePatcher;

internal static class UltrawidePatcher
{
    private const string AssemblyRelativePath =
        "GraveyardKeeper2_Data/Managed/Assembly-CSharp.dll";

    private const string BackupSuffix =
        ".ultrawide-backup";

    public static int Patch(
        string gamePath,
        bool verbose)
    {
        string assemblyPath =
            GetAssemblyPath(gamePath);

        if (!File.Exists(assemblyPath))
        {
            Console.Error.WriteLine(
                "[ERROR] Assembly-CSharp.dll not found.");

            return 1;
        }

        return PatchAssembly(
            assemblyPath,
            verbose);
    }

    public static int Restore(
        string gamePath)
    {
        string assemblyPath =
            GetAssemblyPath(gamePath);

        string backupPath =
            assemblyPath + BackupSuffix;

        if (!File.Exists(backupPath))
        {
            Console.Error.WriteLine(
                "[ERROR] No backup found.");

            return 1;
        }

        File.Copy(
            backupPath,
            assemblyPath,
            overwrite: true);

        Console.WriteLine(
            "[+] Original Assembly-CSharp.dll restored.");

        return 0;
    }

    private static string GetAssemblyPath(
        string gamePath)
    {
        return Path.Combine(
            Path.GetFullPath(gamePath),
            AssemblyRelativePath);
    }

    private static bool ValidateMethod(
        MethodDefinition method)
    {
        if (!method.HasBody)
        {
            return false;
        }

        var instructions = method.Body.Instructions;

        bool hasWidthGetter = instructions.Any(
            instruction =>
                instruction.Operand is MethodReference reference &&
                reference.Name == "get_width");

        bool hasHeightGetter = instructions.Any(
            instruction =>
                instruction.Operand is MethodReference reference &&
                reference.Name == "get_height");

        return hasWidthGetter && hasHeightGetter;
    }

    private static int PatchAssembly(
        string assemblyPath,
        bool verbose)
    {
        Console.WriteLine();
        Console.WriteLine($"Game assembly:");
        Console.WriteLine(assemblyPath);
        Console.WriteLine();

        using var assembly = AssemblyDefinition.ReadAssembly(
            assemblyPath,
            new ReaderParameters
            {
                ReadingMode = ReadingMode.Deferred,
                ReadSymbols = false
            });

        TypeDefinition? resolutionConfig =
            assembly.MainModule.Types.FirstOrDefault(
                type => type.Name == "ResolutionConfig");

        if (resolutionConfig == null)
        {
            Console.Error.WriteLine(
                "[ERROR] ResolutionConfig not found.");

            return 1;
        }

        MethodDefinition? isUltraWide =
            resolutionConfig.Methods.FirstOrDefault(
                method =>
                    method.Name == "IsUltraWide" &&
                    method.IsStatic &&
                    method.ReturnType.FullName ==
                    "System.Boolean");

        if (isUltraWide == null)
        {
            Console.Error.WriteLine(
                "[ERROR] ResolutionConfig.IsUltraWide not found.");

            return 1;
        }

        if (verbose)
        {
            Console.WriteLine(
                $"[+] Found ResolutionConfig.IsUltraWide");

            Console.WriteLine(
                $"[+] RVA: 0x{isUltraWide.RVA:X}");
        }

        // We deliberately do not let Mono.Cecil rewrite the assembly.
        // Some GK2 method bodies cannot currently be serialized
        // correctly by Cecil. The RVA is only used to locate the
        // existing method body for a minimal binary patch.

        uint fileOffset = RvaToFileOffset(
            assemblyPath,
            (uint)isUltraWide.RVA);

        if (verbose)
        {
            Console.WriteLine(
                $"[+] File offset: 0x{fileOffset:X}");
        }

        return PatchMethodBody(
            assemblyPath,
            fileOffset,
            isUltraWide,
            verbose);
    }

    private static int PatchMethodBody(
        string assemblyPath,
        uint methodOffset,
        MethodDefinition method,
        bool verbose)
    {
        using var stream = new FileStream(
            assemblyPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.Read);

        stream.Position = methodOffset;

        int header = stream.ReadByte();

        if (header < 0)
        {
            throw new InvalidDataException(
                "Unable to read method header.");
        }

        // ECMA-335 tiny method header:
        //
        // bits 0..1 = 0b10
        // bits 2..7 = code size

        if ((header & 0x03) != 0x02)
        {
            throw new InvalidDataException(
                "IsUltraWide does not use the expected tiny method header.");
        }

        int codeSize = header >> 2;

        if (verbose)
        {
            Console.WriteLine(
                $"[+] Method code size: {codeSize} bytes");
        }

        if (codeSize < 2)
        {
            throw new InvalidDataException(
                "Unexpected IsUltraWide method size.");
        }

        byte[] code = new byte[codeSize];

        int bytesRead = stream.Read(
            code,
            0,
            code.Length);

        if (bytesRead != code.Length)
        {
            throw new EndOfStreamException(
                "Unable to read complete method body.");
        }

        if (IsPatched(code))
        {
            Console.WriteLine();
            Console.WriteLine(
                "[+] Ultrawide fix is already installed.");

            return 0;
        }

        if (!ValidateMethod(method) ||
            !IsExpectedOriginal(code))
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine(
                "[ERROR] Unknown IsUltraWide implementation.");

            Console.Error.WriteLine(
                "The game may have been updated.");

            Console.Error.WriteLine(
                "No changes were made.");

            return 1;
        }

        string backupPath =
            assemblyPath + BackupSuffix;

        if (!File.Exists(backupPath))
        {
            File.Copy(
                assemblyPath,
                backupPath);

            Console.WriteLine(
                $"[+] Backup created:");
            Console.WriteLine(
                $"    {backupPath}");
        }
        else
        {
            Console.WriteLine(
                "[+] Existing backup preserved.");
        }

        byte[] patchedCode = new byte[codeSize];

        // ldc.i4.0
        patchedCode[0] = 0x16;

        // ret
        patchedCode[1] = 0x2A;

        // Remaining bytes are 0x00 = nop.

        stream.Position = methodOffset + 1;

        stream.Write(
            patchedCode,
            0,
            patchedCode.Length);

        stream.Flush(true);

        Console.WriteLine();
        Console.WriteLine(
            "[+] Ultrawide resolution filter disabled.");
        Console.WriteLine(
            "[+] Patch completed successfully.");

        return 0;
    }

    private static bool IsPatched(byte[] code)
    {
        if (code.Length < 2 ||
            code[0] != 0x16 ||
            code[1] != 0x2A)
        {
            return false;
        }

        return code
            .Skip(2)
            .All(value => value == 0x00);
    }

    private static bool IsExpectedOriginal(byte[] code)
    {
        // Expected IL structure:
        //
        // ldarga.s 0
        // call     Resolution.get_width()
        // conv.r4
        // ldarga.s 0
        // call     Resolution.get_height()
        // conv.r4
        // div
        // ldc.r4   2.0
        // cgt
        // ret
        //
        // The metadata tokens used by the two call instructions are
        // deliberately ignored because they may change between builds.

        if (code.Length != 25)
        {
            return false;
        }

        return
            code[0] == 0x0F &&
            code[1] == 0x00 &&

            code[2] == 0x28 &&

            code[7] == 0x6B &&

            code[8] == 0x0F &&
            code[9] == 0x00 &&

            code[10] == 0x28 &&

            code[15] == 0x6B &&
            code[16] == 0x5B &&

            code[17] == 0x22 &&
            code[18] == 0x00 &&
            code[19] == 0x00 &&
            code[20] == 0x00 &&
            code[21] == 0x40 &&

            code[22] == 0xFE &&
            code[23] == 0x02 &&

            code[24] == 0x2A;
    }

    private static uint RvaToFileOffset(
        string assemblyPath,
        uint rva)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var reader = new BinaryReader(stream);

        // DOS header -> PE header offset.
        stream.Position = 0x3C;
        uint peOffset = reader.ReadUInt32();

        stream.Position = peOffset;

        uint signature = reader.ReadUInt32();

        if (signature != 0x00004550)
        {
            throw new InvalidDataException(
                "Invalid PE signature.");
        }

        // IMAGE_FILE_HEADER
        reader.ReadUInt16(); // Machine

        ushort numberOfSections =
            reader.ReadUInt16();

        reader.ReadUInt32(); // TimeDateStamp
        reader.ReadUInt32(); // PointerToSymbolTable
        reader.ReadUInt32(); // NumberOfSymbols

        ushort optionalHeaderSize =
            reader.ReadUInt16();

        reader.ReadUInt16(); // Characteristics

        // IMAGE_OPTIONAL_HEADER starts here.
        long sectionTable =
            stream.Position + optionalHeaderSize;

        stream.Position = sectionTable;

        for (int i = 0; i < numberOfSections; ++i)
        {
            // IMAGE_SECTION_HEADER.Name
            stream.Position += 8;

            uint virtualSize =
                reader.ReadUInt32();

            uint virtualAddress =
                reader.ReadUInt32();

            uint rawSize =
                reader.ReadUInt32();

            uint rawAddress =
                reader.ReadUInt32();

            uint sectionSize =
                Math.Max(virtualSize, rawSize);

            if (rva >= virtualAddress &&
                rva < virtualAddress + sectionSize)
            {
                return rawAddress +
                    (rva - virtualAddress);
            }

            // Relocations, line numbers and characteristics.
            stream.Position += 16;
        }

        throw new InvalidDataException(
            $"Unable to map RVA 0x{rva:X}.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Usage:");

        Console.WriteLine(
            "  gk2-ultrawide-patcher <game-directory>");

        Console.WriteLine(
            "  gk2-ultrawide-patcher --restore <game-directory>");
    }
}