using NUnit.Framework;

public class SomeUnitTests
{
    private int finalNumber;

    [Test]
    public void Maths_Add()
    {
        // Arrange
        int A = 1;
        int B = 2;
        finalNumber = 0;

        // Act
        Add(A, B);

        // Assert
        Assert.AreEqual(3, finalNumber);
    }

    private void Add(int a, int b)
    {
        finalNumber = a + b;
    }
}