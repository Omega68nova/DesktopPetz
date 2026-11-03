using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project desktop/DesktopPetz.BinarySurvey -- <Petz 5.exe> [additional DLLs]");
    return 2;
}

foreach (var path in args)
{
    Console.WriteLine($"\n=== {Path.GetFileName(path)} ===");
    if (!File.Exists(path)) { Console.WriteLine("Missing file"); continue; }
    using var stream = File.OpenRead(path);
    using var reader = new PEReader(stream);
    var pe = reader.PEHeaders;
    Console.WriteLine($"Machine: {pe.CoffHeader.Machine}; DLL: {pe.IsDll}; PE32+: {pe.PEHeader?.Magic == PEMagic.PE32Plus}");
    var headers = pe.PEHeader;
    if (headers is null) continue;
    Console.WriteLine($"Entry RVA: 0x{headers.AddressOfEntryPoint:X8}");
    Console.WriteLine($"Import directory RVA: 0x{headers.ImportTableDirectory.RelativeVirtualAddress:X8}");
    Console.WriteLine($"Export directory RVA: 0x{headers.ExportTableDirectory.RelativeVirtualAddress:X8}");
    Console.WriteLine($"Resource directory RVA: 0x{headers.ResourceTableDirectory.RelativeVirtualAddress:X8}");
    foreach (var section in pe.SectionHeaders)
        Console.WriteLine($"Section {section.Name,-8} RVA=0x{section.VirtualAddress:X8} size={section.VirtualSize}");
    // This is a passive PE header survey, not a loader or execution harness.
    // The next stage will resolve import/export entries and the PetzA addresses
    // against an exact matching executable build.
}
return 0;
