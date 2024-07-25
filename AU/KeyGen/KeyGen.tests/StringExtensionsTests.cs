using KeyGen.lib.Extensions;

namespace KeyGen.tests;

public class StringExtensionsTests
{
    [Fact]    
    public void ReplaceFirstOccurrence()
    {
        string input = "Aaaaa Bbbbb Ccccc bBBBB Aaaaa";
        string expected = "Aaaaa Xxxxx Ccccc bBBBB Aaaaa";
        var result = input.ReplaceFirstOccurrence("bBBBB", "Xxxxx");
        Assert.Equal(expected, result);
    }


    [Fact]
    public void ReplaceLastOccurrence()
    {
        string input = "Aaaaa bBBBB Ccccc Bbbbb Aaaaa";
        string expected = "Aaaaa bBBBB Ccccc Xxxxx Aaaaa";
        var result = input.ReplaceLastOccurrence("bBBBB", "Xxxxx");
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData(
        "ÁÀÃÂÄÅẦẢẬáàãâäåầảậÇČçčÉÈÊËĚĘỂéèêëěęểĠĞġğĦħÍÌĨÎÏíìĩîïŃÑńñÓÒÕÔÖŌỒỔỢóòõôöōồổợŚŠŞśšşÚÙÜŮƯỨúùüůưứÝỲŸýỳÿŻżÆæßĐđŁłŒœØø", 
        "AAAAAUAAAaaaaauaaaCCccEEEEEEEeeeeeeeGGggHhIIIIIiiiiiNNnnOOOOOOOOOoooooooooSSSsssUUUUUUuuuuuuYYYyyyZzAEaessDdLlOEoeOo")]
    public void ConvertDiacriticsToStandardAnsi_devDiacritics_ReturnsANSIString(string input, string expected)
    {
        var result = input.ConvertDiacriticsToStandardAnsi();
        Assert.Equal(expected, result);
    }
}
