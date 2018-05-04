// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class CachedExpressionCompilerTest
    {
        [Fact]
        public void Process_IdentityExpression()
        {
            // Arrange
            var model = new TestModel();
            var expression = GetTestModelExpression(m => m);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Same(model, result);
        }

        [Fact]
        public void Process_ConstLookup()
        {
            // Arrange
            var model = new TestModel();
            var differentModel = new DifferentModel();
            var expression = GetTestModelExpression(m => differentModel);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Same(differentModel, result);
        }

        [Fact]
        public void Process_ConstLookup_ReturningNull()
        {
            // Arrange
            var model = new TestModel();
            var expression = GetTestModelExpression(m => (DifferentModel)null);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ConstLookup_WithNullModel()
        {
            // Arrange
            var model = new TestModel();
            var differentModel = new DifferentModel();
            var expression = GetTestModelExpression(m => differentModel);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(null);
            Assert.Same(differentModel, result);
        }

        [Fact]
        public void Process_ConstLookup_WithPrimitiveConstant()
        {
            // Arrange
            var model = new TestModel();
            var expression = GetTestModelExpression(m => 10);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(10, result);
        }

        [Fact]
        public void Process_StaticFieldAccess()
        {
            // Arrange
            var model = new TestModel();
            var expression = GetTestModelExpression(m => TestModel.StaticField);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal("StaticValue", result);
        }

        [Fact]
        public void Process_ConstantMemberAccess_WithNullModel()
        {
            // Arrange
            var differentModel = new DifferentModel { Name = "Test" };
            var expression = GetTestModelExpression(m => differentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(null);
            Assert.Equal("Test", result);
        }

        [Fact]
        public void Process_ConstFieldLookup()
        {
            // Arrange
            var model = new TestModel();
            var expression = GetTestModelExpression(m => DifferentModel.Constant);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(10, result);
        }

        [Fact]
        public void Process_ConstFieldLookup_WthNullModel()
        {
            // Arrange
            var expression = GetTestModelExpression(m => DifferentModel.Constant);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(null);
            Assert.Equal(10, result);
        }

        [Fact]
        public void Process_ConstantMember_WithNullConstant()
        {
            // Arrange
            var differentModel = (DifferentModel)null;
            var model = new TestModel();
            var expression = GetTestModelExpression(m => differentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            Assert.Throws<NullReferenceException>(() => func(model));
        }

        [Fact]
        public void Process_SimpleMemberAccess()
        {
            // Arrange
            var model = new TestModel { Name = "Test" };
            var expression = GetTestModelExpression(m => m.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal("Test", result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_ToPrimitive()
        {
            // Arrange
            var model = new TestModel { Age = 12 };
            var expression = GetTestModelExpression(m => m.Age);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(12, result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_WithNullModel()
        {
            // Arrange
            var model = (TestModel)null;
            var expression = GetTestModelExpression(m => m.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_ToPrimitive_WithNullModel()
        {
            // Arrange
            var model = (TestModel)null;
            var expression = GetTestModelExpression(m => m.Age);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_OnTypeWithBadEqualityComparer()
        {
            // Arrange
            var model = new BadEqualityModel { Id = 7 };
            var expression = GetExpression<BadEqualityModel, int>(m => m.Id);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(model.Id, result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_OnTypeWithBadEqualityComparer_WithNullModel()
        {
            // Arrange
            var model = (BadEqualityModel)null;
            var expression = GetExpression<BadEqualityModel, int>(m => m.Id);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_OnValueType()
        {
            // Arrange
            var model = new DateTime(2000, 1, 1);
            var expression = GetExpression<DateTime, int>(m => m.Year);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(model.Year, result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_OnValueType_WithDefaultValue()
        {
            // Arrange
            var model = (DateTime)default;
            var expression = GetExpression<DateTime, int>(m => m.Year);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(model.Year, result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_OnNullableValueType()
        {
            // Arrange
            var model = new DateTime(2000, 1, 1);
            var nullableModel = (DateTime?)model;
            var expression = GetExpression<DateTime?, DateTime>(m => m.Value);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(nullableModel);
            Assert.Equal(model, result);
        }

        [Fact]
        public void Process_SimpleMemberAccess_OnNullableValueType_WithNullValue()
        {
            // Arrange
            var nullableModel = (DateTime?)null;
            var expression = GetExpression<DateTime?, DateTime>(m => m.Value);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(nullableModel);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ArrayMemberAccess()
        {
            // Arrange
            var model = new TestModel { Sizes = new[] { 28, 30, 31 } };
            var expression = GetTestModelExpression(m => m.Sizes[1]);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(30, result);
        }

        [Fact]
        public void Process_ArrayMemberAccess_WithNullModel()
        {
            // Arrange
            var model = (TestModel)null;
            var expression = GetTestModelExpression(m => m.Sizes[1]);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            Assert.Throws<NullReferenceException>(() => func(model));
        }

        [Fact]
        public void Process_ArrayMemberAccess_OutOfBounds()
        {
            // Arrange
            var model = new TestModel { Sizes = new[] { 28, 30, 31 } };
            var expression = GetTestModelExpression(m => m.Sizes[4]);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            Assert.Throws<IndexOutOfRangeException>(() => func(model));
        }

        [Fact]
        public void Process_ChainedMemberAccess_ToValueType()
        {
            // Arrange
            var dateTime = new DateTime(2000, 1, 1);
            var model = new TestModel { Date = dateTime };
            var expression = GetTestModelExpression(m => m.Date.Year);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(dateTime.Year, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_ToValueType_WithNullModel()
        {
            // Arrange
            var model = (TestModel)null;
            var expression = GetTestModelExpression(m => m.Date.Year);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_ToReferenceType()
        {
            // Arrange
            var expected = "Test1";
            var model = new TestModel { DifferentModel = new DifferentModel { Name = expected } };
            var expression = GetTestModelExpression(m => m.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_LongChain_WithReferenceType()
        {
            // Arrange
            var expected = "TestVal";
            var model = new Chain0Model
            {
                Chain1 = new Chain1Model
                {
                    Chain2 = new Chain2Model
                    {
                        TestModel = new TestModel { DifferentModel = new DifferentModel { Name = expected } }
                    }
                }
            };

            var expression = GetExpression<Chain0Model, string>(m => m.Chain1.Chain2.TestModel.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_LongChain_WithNullIntermediary()
        {
            // Arrange
            var model = new Chain0Model
            {
                Chain1 = new Chain1Model
                {
                    Chain2 = new Chain2Model { TestModel = null },
                }
            };

            var expression = GetExpression<Chain0Model, string>(m => m.Chain1.Chain2.TestModel.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_LongChain_WithNullValueTypeAccessor()
        {
            // Arrange
            // Chain2 is a value type
            var model = new Chain0Model
            {
                Chain1 = null
            };

            var expression = GetExpression<Chain0Model, string>(m => m.Chain1.Chain2.TestModel.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_LongChain_WithNullableValueType()
        {
            // Arrange
            var expected = "TestVal";
            var model = new Chain0Model
            {
                Chain1 = new Chain1Model
                {
                    Chain2Nullable = new Chain2Model
                    {
                        TestModel = new TestModel { DifferentModel = new DifferentModel { Name = expected } }
                    }
                }
            };

            var expression = GetExpression<Chain0Model, string>(m => m.Chain1.Chain2Nullable.Value.TestModel.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_LongChain_WithNullValuedNullableValueType()
        {
            // Arrange
            var model = new Chain0Model
            {
                Chain1 = new Chain1Model
                {
                    Chain2Nullable = null
                }
            };

            var expression = GetExpression<Chain0Model, string>(m => m.Chain1.Chain2Nullable.Value.TestModel.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_ToReferenceType_WithNullIntermediary()
        {
            // Arrange
            var model = new TestModel { DifferentModel = null };
            var expression = GetTestModelExpression(m => m.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_ToReferenceType_WithNullModel()
        {
            // Arrange
            var model = (TestModel)null;
            var expression = GetTestModelExpression(m => m.DifferentModel.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_OfValueTypes_ReturningReferenceTypeMember()
        {
            // Arrange
            var expected = "TestName";
            var model = new ValueType1
            {
                ValueType2 = new ValueType2 { Name = expected },
            };
            var expression = GetExpression<ValueType1, string>(m => m.ValueType2.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_OfValueTypes_ReturningValueType()
        {
            // Arrange
            var expected = new DateTime(2001, 1, 1);
            var model = new ValueType1
            {
                ValueType2 = new ValueType2 { Date = expected },
            };
            var expression = GetExpression<ValueType1, DateTime>(m => m.ValueType2.Date);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_OfValueTypes_IncludingNullableType()
        {
            // Arrange
            var expected = "TestName";
            var model = new ValueType1
            {
                NullableValueType2 = new ValueType2 { Name = expected },
            };
            var expression = GetExpression<ValueType1, string>(m => m.NullableValueType2.Value.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_OfValueTypes_WithNullValuedNullable()
        {
            // Arrange
            var model = new ValueType1 { NullableValueType2 = null };
            var expression = GetExpression<ValueType1, string>(m => m.NullableValueType2.Value.Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ChainedMemberAccess_OfValueTypes_WithNullValuedNullable_ReturningValueType()
        {
            // Arrange
            var model = new ValueType1 { NullableValueType2 = null };
            var expression = GetExpression<ValueType1, DateTime>(m => m.NullableValueType2.Value.Date);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Null(result);
        }

        [Fact]
        public void Process_ComplexChainedMemberAccess()
        {
            // Arrange
            var expected = "SomeName";
            var model = new TestModel { DifferentModels = new[] { new DifferentModel { Name = expected } } };
            var expression = GetTestModelExpression(m => m.DifferentModels[0].Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            var result = func(model);
            Assert.Equal("SomeName", result);
        }

        [Fact]
        public void Process_ComplexChainedArrayAccessor_WithNullIntermediaryModel()
        {
            // Arrange
            var model = new TestModel { DifferentModels = new DifferentModel[1] };
            var expression = GetTestModelExpression(m => m.DifferentModels[0].Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            Assert.Throws<NullReferenceException>(() => func(model));
        }

        [Fact]
        public void Process_ComplexChainedMemberAccess_WithNullModel()
        {
            // Arrange
            var model = (TestModel)null;
            var expression = GetTestModelExpression(m => m.DifferentModels[0].Name);

            // Act
            var func = CachedExpressionCompiler.Process(expression);

            // Assert
            Assert.Throws<NullReferenceException>(() => func(model));
        }

        private static Expression<Func<TModel, TResult>> GetExpression<TModel, TResult>(Expression<Func<TModel, TResult>> expression)
            => expression;

        private static Expression<Func<TestModel, TResult>> GetTestModelExpression<TResult>(Expression<Func<TestModel, TResult>> expression)
            => GetExpression(expression);

        public class TestModel
        {
            public static readonly string StaticField = "StaticValue";

            public int Age { get; set; }

            public string Name { get; set; }

            public DateTime Date { get; set; }

            public DifferentModel DifferentModel { get; set; }

            public int[] Sizes { get; set; }

            public DifferentModel[] DifferentModels { get; set; }
        }

        public class DifferentModel
        {
            public const int Constant = 10;

            public string Name { get; set; }
        }

        public class Chain0Model
        {
            public Chain1Model Chain1 { get; set; }
        }

        public class Chain1Model
        {
            public Chain2Model Chain2 { get; set; }

            public Chain2Model? Chain2Nullable { get; set; }
        }

        public struct Chain2Model
        {
            public TestModel TestModel { get; set; }
        }

        public struct ValueType1
        {
            public ValueType2 ValueType2 { get; set; }

            public ValueType2? NullableValueType2 { get; set; }
        }

        public struct ValueType2
        {
            public string Name { get; set; }

            public DateTime Date { get; set; }
        }

        public class BadEqualityModel
        {
            public int Id { get; set; }

            public override bool Equals(object obj)
            {
                return this == obj;
            }

            public static bool operator ==(BadEqualityModel a, object b)
            {
                if (a is null || b is null)
                {
                    throw new TimeZoneNotFoundException();
                }

                return true;
            }

            public static bool operator !=(BadEqualityModel a, object b)
            {
                return !(a == b);
            }

            public override int GetHashCode() => 0;
        }
    }
}