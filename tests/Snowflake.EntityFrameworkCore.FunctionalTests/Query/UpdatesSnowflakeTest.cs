using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

/// <summary>
/// This class does not inherit from the EFCore (UpdatesRelationalTestBase) class, because they
/// to do not set some test as `virtual`, so it is not possible to override them.
/// </summary>
public class UpdatesSnowflakeTest : IClassFixture<UpdatesSnowflakeFixture>
{
    public UpdatesSnowflakeTest(UpdatesSnowflakeFixture fixture)
    {
        Fixture = fixture;
    }
    
    #region Settings

    protected UpdatesSnowflakeFixture Fixture { get; }

    protected virtual void ExecuteWithStrategyInTransaction(
        Action<UpdatesContext> testOperation,
        Action<UpdatesContext>? nestedTestOperation1 = null,
        Action<UpdatesContext>? nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransaction(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<UpdatesContext, Task> testOperation,
        Func<UpdatesContext, Task>? nestedTestOperation1 = null,
        Func<UpdatesContext, Task>? nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction) =>
        facade.UseTransaction(transaction.GetDbTransaction());

    protected UpdatesContext CreateContext()
        => Fixture.CreateContext();

    protected virtual string UpdateConcurrencyMessage
        => RelationalStrings.UpdateConcurrencyException(1, 0);

    protected virtual string UpdateConcurrencyTokenMessage
        => RelationalStrings.UpdateConcurrencyException(1, 0);

    #endregion

    #region Null Value Issue (SNOW-1883691)

    [ConditionalFact]
    public virtual void Can_use_shared_columns_with_conversion()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var person = new Person("1", null)
                {
                    Address = new Address { Country = Country.Eswatini, City = "Bulembu" }, Country = "Eswatini"
                };
                context.Add(person);
                
                context.SaveChanges();
            },
            context =>
            {
                var person = context.Set<Person>().Single();
                person.Address = new Address
                {
                    Country = Country.Türkiye,
                    City = "Konya",
                    ZipCode = 42100
                };

                context.SaveChanges();
            },
            context =>
            {
                var person = context.Set<Person>().Single();

                Assert.Equal(Country.Türkiye, person.Address!.Country);
                Assert.Equal("Konya", person.Address.City);
                Assert.Equal(42100, person.Address.ZipCode);
                Assert.Equal("Türkiye", person.Country);
                Assert.Equal("42100", person.ZipCode);
            });


    [ConditionalFact(Skip="Transactional context issue")]
    public virtual void Save_with_shared_foreign_key()
    {
        var productId = Guid.Empty;
        ExecuteWithStrategyInTransaction(
            context =>
            {
                var product = new ProductWithBytes();
                context.Add(product);

                context.SaveChanges();

                productId = product.Id;
            },
            context =>
            {
                var product = context.ProductWithBytes.Find(productId)!;
                var category = new SpecialCategory { PrincipalId = 777 };
                var productCategory = new ProductCategory { Category = category };
                product.ProductCategories = new List<ProductCategory> { productCategory };

                context.SaveChanges();

                Assert.True(category.Id > 0);
                Assert.Equal(category.Id, productCategory.CategoryId);
            },
            context =>
            {
                var product = context.Set<ProductBase>()
                    .Include(p => ((ProductWithBytes)p).ProductCategories)
                    .Include(p => ((Product)p).ProductCategories)
                    .OfType<ProductWithBytes>()
                    .Single();
                var productCategory = product.ProductCategories.Single();
                Assert.Equal(productCategory.CategoryId, context.Set<ProductCategory>().Single().CategoryId);
                Assert.Equal(productCategory.CategoryId,
                    context.Set<SpecialCategory>().Single(c => c.PrincipalId == 777).Id);
            });
    }

    [ConditionalFact]
    public virtual void Swap_filtered_unique_index_values()
    {
        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                product2.Name = null;
                product2.Price = product1.Price;

                context.SaveChanges();
            },
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                product2.Name = product1.Name;
                product1.Name = null;

                context.SaveChanges();
            },
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                Assert.Equal(1.49M, product1.Price);
                Assert.Null(product1.Name);
                Assert.Equal(1.49M, product2.Price);
                Assert.Equal("Apple Cider", product2.Name);
            });
    }

    [ConditionalFact]
    public virtual void Update_non_indexed_values()
    {
        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                product2.Price = product1.Price;

                context.SaveChanges();
            },
            context =>
            {
                var product1 = new Product
                {
                    Id = productId1,
                    Name = "",
                    Price = 1.49M
                };
                var product2 = new Product
                {
                    Id = productId2,
                    Name = "",
                    Price = 1.49M
                };

                context.Attach(product1).Property(p => p.DependentId).IsModified = true;
                context.Attach(product2).Property(p => p.DependentId).IsModified = true;

                context.SaveChanges();
            },
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                Assert.Equal(1.49M, product1.Price);
                Assert.Null(product1.DependentId);
                Assert.Equal(1.49M, product2.Price);
                Assert.Null(product2.DependentId);
            });
    }

    [ConditionalFact(Skip = "Tracking issue with generated keys")]
    public virtual void Can_add_and_remove_self_refs()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var parent = new Person("1", null);
                var child1 = new Person("2", parent);
                var child2 = new Person("3", parent);
                var grandchild1 = new Person("4", child1);
                var grandchild2 = new Person("5", child1);
                var grandchild3 = new Person("6", child2);
                var grandchild4 = new Person("7", child2);

                context.Add(parent);
                context.Add(child1);
                context.Add(child2);
                context.Add(grandchild1);
                context.Add(grandchild2);
                context.Add(grandchild3);
                context.Add(grandchild4);

                context.SaveChanges();

                context.Remove(parent);
                context.Remove(child1);
                context.Remove(child2);
                context.Remove(grandchild1);
                context.Remove(grandchild2);
                context.Remove(grandchild3);
                context.Remove(grandchild4);

                parent = new Person("1", null);
                child1 = new Person("2", parent);
                child2 = new Person("3", parent);
                grandchild1 = new Person("4", child1);
                grandchild2 = new Person("5", child1);
                grandchild3 = new Person("6", child2);
                grandchild4 = new Person("7", child2);

                context.Add(parent);
                context.Add(child1);
                context.Add(child2);
                context.Add(grandchild1);
                context.Add(grandchild2);
                context.Add(grandchild3);
                context.Add(grandchild4);

                context.SaveChanges();
            },
            context =>
            {
                var people = context.Set<Person>()
                    .Include(p => p.Parent!).ThenInclude(c => c.Parent!).ThenInclude(c => c.Parent)
                    .ToList();
                Assert.Equal(7, people.Count);
                Assert.Equal("1", people.Single(p => p.Parent == null).Name);
            });

    [ConditionalFact]
    public virtual void Can_change_enums_with_conversion()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var person = new Person("1", null)
                {
                    Address = new Address { Country = Country.Eswatini, City = "Bulembu" }, Country = "Eswatini"
                };

                context.Add(person);

                context.SaveChanges();
            },
            context =>
            {
                var person = context.Set<Person>().Single();
                person.Address = new Address
                {
                    Country = Country.Türkiye,
                    City = "Konya",
                    ZipCode = 42100
                };
                person.Country = "Türkiye";
                person.ZipCode = "42100";

                context.SaveChanges();
            },
            context =>
            {
                var person = context.Set<Person>().Single();

                Assert.Equal(Country.Türkiye, person.Address!.Country);
                Assert.Equal("Konya", person.Address.City);
                Assert.Equal(42100, person.Address.ZipCode);
                Assert.Equal("Türkiye", person.Country);
                Assert.Equal("42100", person.ZipCode);
            });

    [ConditionalTheory(Skip = "Issue with unique index and null values")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_change_type_of__dependent_by_replacing_with_new_dependent(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var lift = new Lift { Recipient = "Alice", Obscurer = new LiftPaper { Pattern = "Stripes" } };
                await context.AddAsync(lift);
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var lift = await context.Set<Lift>().Include(e => e.Obscurer).SingleAsync();
                var bag = new LiftBag { Pattern = "Gold stars" };
                lift.Obscurer = bag;
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var lift = await context.Set<Lift>().Include(e => e.Obscurer).SingleAsync();

                Assert.IsType<LiftBag>(lift.Obscurer);
                Assert.Equal(lift.Id, lift.Obscurer.LiftId);
                Assert.Single(context.Set<LiftObscurer>());
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_change_type_of_pk_to_pk_dependent_by_replacing_with_new_dependent(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gift = new Gift { Recipient = "Alice", Obscurer = new GiftPaper { Pattern = "Stripes" } };
                await context.AddAsync(gift);
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var gift = await context.Set<Gift>().Include(e => e.Obscurer).SingleAsync();
                var bag = new GiftBag { Pattern = "Gold stars" };
                gift.Obscurer = bag;
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var gift = await context.Set<Gift>().Include(e => e.Obscurer).SingleAsync();

                Assert.IsType<GiftBag>(gift.Obscurer);
                Assert.Equal(gift.Id, gift.Obscurer.Id);
                Assert.Single(context.Set<GiftObscurer>());
            });

    #endregion

    #region DbUpdateConcurrencyException not thrown (SNOW-1883695)

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Remove_on_bytes_concurrency_token_original_value_mismatch_throws()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [8, 7, 6, 5, 4, 3, 2, 1]
                    });

                entry.State = EntityState.Deleted;

                Assert.Throws<DbUpdateConcurrencyException>(
                    () => context.SaveChanges());
            },
            context => Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId)!.Name));
    }

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Remove_partial_on_concurrency_token_original_value_mismatch_throws()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Products.Remove(
                    new Product
                    {
                        Id = productId, Price = 3.49M // Not the same as the value stored in the database
                    });

                Assert.Equal(
                    UpdateConcurrencyTokenMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });
    }

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Remove_partial_on_missing_record_throws()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Products.Remove(
                    new Product { Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10") });

                Assert.Equal(
                    UpdateConcurrencyMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Save_partial_update_on_concurrency_token_original_value_mismatch_throws()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                var entry = context.Products.Attach(
                    new Product
                    {
                        Id = productId,
                        Name = "Apple Fritter",
                        Price = 3.49M // Not the same as the value stored in the database
                    });

                entry.Property(c => c.Name).IsModified = true;

                Assert.Equal(
                    UpdateConcurrencyTokenMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });
    }

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Save_partial_update_on_missing_record_throws()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var entry = context.Products.Attach(
                    new Product { Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10"), Name = "Apple Fritter" });

                entry.Property(c => c.Name).IsModified = true;

                Assert.Equal(
                    UpdateConcurrencyMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Update_on_bytes_concurrency_token_original_value_mismatch_throws()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [8, 7, 6, 5, 4, 3, 2, 1]
                    });

                entry.Entity.Name = "GigaChips";

                Assert.Throws<DbUpdateConcurrencyException>(
                    () => context.SaveChanges());
            },
            context => Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId)!.Name));
    }

    #endregion

    #region Hybrid Table Index Issues (SNOW-1883696)

    [ConditionalFact(Skip = "Skipped due not supported concurrency token")]
    public virtual void Identifiers_are_generated_correctly()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(
            typeof(
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
            ))!;
        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorking~",
            entityType.GetTableName());
        Assert.Equal(
            "PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType.GetKeys().Single().GetName());
        Assert.Equal(
            "FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType.GetForeignKeys().Single().GetConstraintName());
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType.GetIndexes().Single().GetDatabaseName());

        var entityType2 = context.Model.FindEntityType(
            typeof(
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
            ))!;

        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkin~1",
            entityType2.GetTableName());
        Assert.Equal(
            "PK_LoginDetails",
            entityType2.GetKeys().Single().GetName());
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCo~",
            entityType2.GetProperties().ElementAt(1)
                .GetColumnName(StoreObjectIdentifier.Table(entityType2.GetTableName()!)));
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingC~1",
            entityType2.GetProperties().ElementAt(2)
                .GetColumnName(StoreObjectIdentifier.Table(entityType2.GetTableName()!)));
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType2.GetIndexes().Single().GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void SaveChanges_works_for_entities_also_mapped_to_view()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var category = context.Categories.Single();

                context.Add(
                    new ProductTableWithView
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pear Cider",
                        Price = 1.39M,
                        DependentId = category.Id
                    });
                context.Add(
                    new ProductViewTable
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pear Cobler",
                        Price = 2.39M,
                        DependentId = category.Id
                    });

                context.SaveChanges();
            },
            context =>
            {
                var viewProduct = context.Set<ProductTableWithView>().Single();
                var tableProduct = context.Set<ProductTableView>().Single();

                Assert.Equal("Pear Cider", tableProduct.Name);
                Assert.Equal("Pear Cobler", viewProduct.Name);
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_delete_and_add_for_same_key(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var rodney1 = new Rodney { Id = "SnotAndMarmite", Concurrency = new DateTime(1973, 9, 3) };
                if (async)
                {
                    await context.AddAsync(rodney1);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(rodney1);
                    context.SaveChanges();
                }

                context.Remove(rodney1);

                var rodney2 = new Rodney { Id = "SnotAndMarmite", Concurrency = new DateTime(1973, 9, 4) };
                if (async)
                {
                    await context.AddAsync(rodney2);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(rodney2);
                    context.SaveChanges();
                }

                Assert.Single(context.ChangeTracker.Entries());
                Assert.Equal(EntityState.Unchanged, context.Entry(rodney2).State);
                Assert.Equal(EntityState.Detached, context.Entry(rodney1).State);
            });

    [ConditionalFact]
    public virtual void Can_remove_partial()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Products.Remove(
                    new Product { Id = productId, Price = 1.49M });

                context.SaveChanges();
            },
            context =>
            {
                var product = context.Products.FirstOrDefault(f => f.Id == productId);

                Assert.Null(product);
            });
    }

    [ConditionalFact]
    public virtual void Mutation_of_tracked_values_does_not_mutate_values_in_store()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var bytes = new byte[] { 1, 2, 3, 4 };

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.AFewBytes.AddRange(
                    new AFewBytes { Id = id1, Bytes = bytes },
                    new AFewBytes { Id = id2, Bytes = bytes });

                context.SaveChanges();
            },
            context =>
            {
                bytes[1] = 22;

                var fromStore1 = context.AFewBytes.First(p => p.Id == id1);
                var fromStore2 = context.AFewBytes.First(p => p.Id == id2);

                Assert.Equal(2, fromStore1.Bytes[1]);
                Assert.Equal(2, fromStore2.Bytes[1]);

                fromStore1.Bytes[1] = 222;
                fromStore2.Bytes[1] = 222;

                context.Entry(fromStore1).State = EntityState.Modified;

                context.SaveChanges();
            },
            context =>
            {
                var fromStore1 = context.AFewBytes.First(p => p.Id == id1);
                var fromStore2 = context.AFewBytes.First(p => p.Id == id2);

                Assert.Equal(222, fromStore1.Bytes[1]);
                Assert.Equal(2, fromStore2.Bytes[1]);
            });
    }


    [ConditionalFact]
    public virtual void Remove_on_bytes_concurrency_token_original_value_matches_does_not_throw()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                entry.State = EntityState.Deleted;

                Assert.Equal(1, context.SaveChanges());
            },
            context => Assert.Null(context.ProductWithBytes.Find(productId)));
    }

    [ConditionalFact]
    public virtual void Save_partial_update()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                var entry = context.Products.Attach(
                    new Product { Id = productId, Price = 1.49M });

                entry.Property(c => c.Price).CurrentValue = 1.99M;
                entry.Property(p => p.Price).IsModified = true;

                Assert.False(entry.Property(p => p.DependentId).IsModified);
                Assert.False(entry.Property(p => p.Name).IsModified);

                context.SaveChanges();
            },
            context =>
            {
                var product = context.Products.First(p => p.Id == productId);

                Assert.Equal(1.99M, product.Price);
                Assert.Equal("Apple Cider", product.Name);
            });
    }

    [ConditionalFact]
    public virtual void Save_replaced_principal()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var category = context.Categories.AsNoTracking().Single();
                var products = context.Products.AsNoTracking().Where(p => p.DependentId == category.PrincipalId)
                    .ToList();

                Assert.Equal(2, products.Count);

                var newCategory = new Category
                {
                    Id = category.Id,
                    PrincipalId = category.PrincipalId,
                    Name = "New Category"
                };
                context.Remove(category);
                context.Add(newCategory);

                context.SaveChanges();
            },
            context =>
            {
                var category = context.Categories.Single();
                var products = context.Products.Where(p => p.DependentId == category.PrincipalId).ToList();

                Assert.Equal("New Category", category.Name);
                Assert.Equal(2, products.Count);
            });

    [ConditionalFact]
    public virtual void Update_on_bytes_concurrency_token_original_value_matches_does_not_throw()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                entry.Entity.Name = "GigaChips";

                Assert.Equal(1, context.SaveChanges());
            },
            context => Assert.Equal("GigaChips", context.ProductWithBytes.Find(productId)!.Name));
    }

    [ConditionalFact]
    public virtual void SaveChanges_throws_for_entities_only_mapped_to_view()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var category = context.Categories.Single();
                context.Add(
                    new ProductTableView
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pear Cider",
                        Price = 1.39M,
                        DependentId = category.Id
                    });

                Assert.Equal(
                    RelationalStrings.ReadonlyEntitySaved(nameof(ProductTableView)),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            });

    #endregion
}