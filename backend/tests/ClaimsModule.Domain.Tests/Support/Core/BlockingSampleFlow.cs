namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class BlockingSampleFlow : SampleFlow<BlockingSampleFlow>
{
    public const string Blocked = "BLOCK";

    public BlockingSampleFlow()
    {
        Require(SampleRules.Fail(Blocked));
    }
}
