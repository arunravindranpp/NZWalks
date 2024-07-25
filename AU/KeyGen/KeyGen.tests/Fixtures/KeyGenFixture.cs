namespace KeyGen.tests.Fixtures;

public class KeyGenFixture : IDisposable
{
    private static readonly AppSettings _appSettings = new();
    public static readonly KeyGenFactory KeyGenFactory = new(_appSettings.ConnectionString);
    public readonly KeyGenForEntities KeyGenForEntities = KeyGenFactory.KeyGenForEntities;
    public readonly KeyGenForIndividuals KeyGenForIndividuals = KeyGenFactory.KeyGenForIndividuals;

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() { }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
}
