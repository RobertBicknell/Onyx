using Onyx.API.Products;

namespace Onyx.Helpers.UnitTests
{
    public class LambdaFactoryTests
    {
        [Fact]
        public void LambdaValidForAllStringPropertiesOfProduct()
        {
            var greenProduct = new Product
            {
                Name = "Foo",   
                Colour = "Green"
            };
            var blueProduct = new Product
            {
                Name = "Bar",
                Colour = "Blue"
            };
            var products = new Product[] {
                greenProduct,
                blueProduct
            };
            _ = LambdaFactory<Product>.TryCreateFilter("Colour", "Green", out var colourGreenLambda);
            var colourGreenResult = products.Where(colourGreenLambda.Compile());
            Assert.True(colourGreenResult.Count() == 1 && colourGreenResult.First().Colour == "Green");

            _ = LambdaFactory<Product>.TryCreateFilter("Colour", "Blue", out var colourBlueLambda);
            var colourBlueResult = products.Where(colourBlueLambda.Compile());
            Assert.True(colourBlueResult.Count() == 1 && colourBlueResult.First().Colour == "Blue");

            _ = LambdaFactory<Product>.TryCreateFilter("Name", "Foo", out var nameFooLambda);
            var nameFooResult = products.Where(nameFooLambda.Compile());
            Assert.True(nameFooResult.Count() == 1 && nameFooResult.First().Name == "Foo");

            _ = LambdaFactory<Product>.TryCreateFilter("Name", "Bar", out var nameBarLambda);
            var nameBarResult = products.Where(nameBarLambda.Compile());
            Assert.True(nameBarResult.Count() == 1 && nameBarResult.First().Name == "Bar");
        }

        [Fact]
        public void LambdaFactoryReturnsFalseForNonStringProperty()
        {
            var result = LambdaFactory<Product>.TryCreateFilter("ID", "not an int", out var tryIntLambda);
            Assert.False(result);
        }
    }
}