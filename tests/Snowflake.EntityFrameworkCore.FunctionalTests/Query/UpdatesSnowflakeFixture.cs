using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.Updates;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class UpdatesSnowflakeFixture : SharedStoreFixtureBase<UpdatesContext>
    {
        protected override string StoreName
            => "UpdateTest";

        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;

        protected override void Seed(UpdatesContext context)
            => UpdatesSnowflakeContext.Seed(context);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Product>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);
            modelBuilder.Entity<ProductWithBytes>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(p => new { p.CategoryId, p.ProductId });

            modelBuilder.Entity<Product>().HasOne(p => p.DefaultCategory).WithMany()
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);

            modelBuilder.Entity<Person>(
                pb =>
                {
                    pb.HasOne(p => p.Parent)
                        .WithMany()
                        .OnDelete(DeleteBehavior.Restrict);
                    pb.OwnsOne(p => p.Address)
                        .Property(p => p.Country)
                        .HasConversion<string>();
                    pb.Property(p => p.ZipCode)
                        .HasConversion<int?>(v => v == null ? null : int.Parse(v),
                            v => v == null ? null : v.ToString()!);
                });

            modelBuilder.Entity<Category>().HasMany(e => e.ProductCategories).WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<SpecialCategory>();

            modelBuilder.Entity<AFewBytes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
                >(
                    eb =>
                    {
                        eb.HasKey(
                            l => new
                            {
                                l.ProfileId,
                                l.ProfileId1,
                                l.ProfileId3,
                                l.ProfileId4,
                                l.ProfileId5,
                                l.ProfileId6,
                                l.ProfileId7,
                                l.ProfileId8,
                                l.ProfileId10,
                                l.ProfileId11,
                                l.ProfileId12,
                                l.ProfileId13,
                                l.ProfileId14
                            });
                    });

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                >(
                    eb =>
                    {
                        eb.HasKey(l => new { l.ProfileId });
                        eb.HasOne(d => d.Login).WithOne()
                            .HasForeignKey<
                                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                            >(
                                l => new
                                {
                                    l.ProfileId,
                                    l.ProfileId1,
                                    l.ProfileId3,
                                    l.ProfileId4,
                                    l.ProfileId5,
                                    l.ProfileId6,
                                    l.ProfileId7,
                                    l.ProfileId8,
                                    l.ProfileId10,
                                    l.ProfileId11,
                                    l.ProfileId12,
                                    l.ProfileId13,
                                    l.ProfileId14
                                });
                    });

            modelBuilder.Entity<Profile>(
                pb =>
                {
                    pb.HasKey(
                        l => new
                        {
                            l.Id,
                            l.Id1,
                            l.Id3,
                            l.Id4,
                            l.Id5,
                            l.Id6,
                            l.Id7,
                            l.Id8,
                            l.Id10,
                            l.Id11,
                            l.Id12,
                            l.Id13,
                            l.Id14
                        });
                    pb.HasOne(p => p.User)
                        .WithOne(l => l.Profile)
                        .IsRequired();
                });

            modelBuilder.Entity<Gift>();
            modelBuilder.Entity<GiftObscurer>().HasOne<Gift>().WithOne(x => x.Obscurer)
                .HasForeignKey<GiftObscurer>(e => e.Id);
            modelBuilder.Entity<GiftBag>();
            modelBuilder.Entity<GiftPaper>();

            modelBuilder.Entity<Lift>();
            modelBuilder.Entity<LiftObscurer>().HasOne<Lift>().WithOne(x => x.Obscurer)
                .HasForeignKey<LiftObscurer>(e => e.LiftId);
            modelBuilder.Entity<LiftBag>();
            modelBuilder.Entity<LiftPaper>();

            modelBuilder.Entity<ProductViewTable>().HasBaseType((string)null!).ToTable("ProductView");
            modelBuilder.Entity<ProductTableWithView>().HasBaseType((string)null!).ToView("ProductView")
                .ToTable("ProductTable");
            modelBuilder.Entity<ProductTableView>().HasBaseType((string)null!).ToView("ProductTable");
            modelBuilder.Entity<Person>(
                pb =>
                {
                    pb.Property(p => p.Country)
                        .HasColumnName("Country");
                    pb.Property(p => p.ZipCode)
                        .HasColumnName("ZipCode");
                    pb.OwnsOne(p => p.Address)
                        .Property(p => p.Country)
                        .HasColumnName("Country");
                    pb.OwnsOne(p => p.Address)
                        .Property(p => p.ZipCode)
                        .HasColumnName("ZipCode");
                });

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                >(
                    eb =>
                    {
                        eb.HasKey(
                                l => new { l.ProfileId })
                            .HasName("PK_LoginDetails");

                        eb.HasOne(d => d.Login).WithOne()
                            .HasConstraintName("FK_LoginDetails_Login");
                    });

            modelBuilder.Entity<ProductBase>()
                .Property(p => p.Id).HasDefaultValueSql("UUID_STRING()");
        }
    }