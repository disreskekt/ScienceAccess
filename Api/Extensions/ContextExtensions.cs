using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Extensions
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Replaces
        /// .Entity().Property(propertyExpression).HasConversion();
        /// </summary>
        /// <param name="modelBuilder">Model Builder</param>
        /// <param name="propertyExpression">Expression for Property(entity => entity.Name)</param>
        public static PropertyBuilder<TEnumProperty> EnumToStringConversion<TEntity, TEnumProperty>(
            this ModelBuilder modelBuilder, [NotNull] Expression<Func<TEntity, TEnumProperty>> propertyExpression)
            where TEntity : class
        {
            return modelBuilder.Entity<TEntity>()
                .Property(propertyExpression)
                .HasConversion<string>();
        }
    }
}