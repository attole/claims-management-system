using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace ClaimsModule.Application.Tests.Support.Assemblies;

public static class AssemblyStrings
{
    public static IReadOnlyList<string> Literals(Assembly assembly)
    {
        using var stream = File.OpenRead(assembly.Location);
        using var reader = new PEReader(stream);
        var metadata = reader.GetMetadataReader();
        List<string> literals = [];

        for (var handle = metadata.GetNextHandle(default(UserStringHandle)); !handle.IsNil;)
        {
            literals.Add(metadata.GetUserString(handle));
            handle = metadata.GetNextHandle(handle);
        }

        return literals;
    }
}
