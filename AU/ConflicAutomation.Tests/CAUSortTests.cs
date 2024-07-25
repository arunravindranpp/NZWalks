using ConflictAutomation.Services.Sorting;

namespace ConflicAutomation.Tests;

#pragma warning disable IDE0305 // Simplify collection initialization
public class CAUSortTests
{
    private const char SEP = '|';

    [Theory]
    [InlineData("02|04.G01|03|04.C01|01|04.P01|06|05.D01.C01|09|05.D01.G01|08|04.A01.C01|07|04.A01.G01|05|04",
                "01|02|03|04|04.P01|04.G01|04.C01|04.A01.G01|04.A01.C01|05|05.D01.G01|05.D01.C01|06|07|08|09")]
    public void CAUSort_StdCase_ReturnSortedListByCauCriteria(string input, string expected)
    {
        List<string> inputList = input.Split(SEP).ToList();
        List<string> sortedList = inputList.CAUSort();
        string result = string.Join(SEP, sortedList);
        Assert.Equal(expected, result);
    }
}
#pragma warning restore IDE0305 // Simplify collection initialization
