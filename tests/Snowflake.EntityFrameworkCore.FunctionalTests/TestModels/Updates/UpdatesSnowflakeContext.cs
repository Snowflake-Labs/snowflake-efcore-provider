using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.Updates;

public class UpdatesSnowflakeContext(DbContextOptions options) : UpdatesContext(options)
{
    public new static void Seed(UpdatesContext context)
    {
        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        context.Add(
            new Category { PrincipalId = 778, Name = "Juice", Id = 1});
        context.Add(
            new Product
            {
                Id = productId1,
                Name = "Apple Cider",
                Price = 1.49M,
                DependentId = 778
            });
        context.Add(
            new Product
            {
                Id = productId2,
                Name = "Apple Cobler",
                Price = 2.49M,
                DependentId = 778
            });

        context.SaveChanges();
    }
}