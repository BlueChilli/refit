// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// A base class for test data.  A <see cref="TestData"/> instance is associated with a given type, and the <see cref="TestData"/> instance can
    /// provide instances of the given type to use as data in tests.  The same <see cref="TestData"/> instance can also provide instances
    /// of types related to the given type, such as a <see cref="List<>"/> of the type.  See the <see cref="TestDataVariations"/> enum for all the
    /// variations of test data that a <see cref="TestData"/> instance can provide.
    /// </summary>
    public abstract class TestData
    {
        
        private Dictionary<TestDataVariations, TestDataVariationProvider> registeredTestDataVariations;


        /// <summary>
        /// Initializes a new instance of the <see cref="TestData"/> class.
        /// </summary>
        /// <param name="type">The type associated with the <see cref="TestData"/> instance.</param>
        protected TestData(Type type)
        {
            if (type.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Only closed generic types are supported.");
            }

            this.Type = type;
            this.registeredTestDataVariations = new Dictionary<TestDataVariations, TestDataVariationProvider>();
        }

        /// <summary>
        /// Gets the type associated with the <see cref="TestData"/> instance.
        /// </summary>
        public Type Type { get; private set; }


        /// <summary>
        /// Gets the supported test data variations.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TestDataVariations> GetSupportedTestDataVariations()
        {
            return this.registeredTestDataVariations.Keys;
        }

        /// <summary>
        /// Gets the related type for the given test data variation or returns null if the <see cref="TestData"/> instance
        /// doesn't support the given variation.
        /// </summary>
        /// <param name="variation">The test data variation with which to create the related <see cref="Type"/>.</param>
        /// <returns>The related <see cref="Type"/> for the <see cref="TestData.Type"/> as given by the test data variation.</returns>
        /// <example>
        /// For example, if the given <see cref="TestData"/> was created for <see cref="string"/> test data and the varation parameter
        /// was <see cref="TestDataVariations.AsList"/> then the returned type would be <see cref="List<string>"/>.
        /// </example>
        public Type GetAsTypeOrNull(TestDataVariations variation)
        {
            TestDataVariationProvider testDataVariation = null;
            if (this.registeredTestDataVariations.TryGetValue(variation, out testDataVariation))
            {
                return testDataVariation.Type;
            }

            return null;
        }

        /// <summary>
        /// Gets test data for the given test data variation or returns null if the <see cref="TestData"/> instance
        /// doesn't support the given variation.
        /// </summary>
        /// <param name="variation">The test data variation with which to create the related test data.</param>
        /// <returns>Test data of the type specified by the <see cref="TestData.GetAsTypeOrNull"/> method.</returns>
        public object GetAsTestDataOrNull(TestDataVariations variation)
        {
            TestDataVariationProvider testDataVariation = null;
            if (this.registeredTestDataVariations.TryGetValue(variation, out testDataVariation))
            {
                return testDataVariation.TestDataProvider();
            }

            return null;
        }


        /// <summary>
        /// Allows derived classes to register a <paramref name="testDataProvider "/> <see cref="Func<>"/> that will 
        /// provide test data for a given variation.
        /// </summary>
        /// <param name="variation">The variation with which to register the <paramref name="testDataProvider "/>r.</param>
        /// <param name="type">The type of the test data created by the <paramref name="testDataProvider "/></param>
        /// <param name="testDataProvider">A <see cref="Func<>"/> that will provide test data.</param>
        protected void RegisterTestDataVariation(TestDataVariations variation, Type type, Func<object> testDataProvider)
        {
            this.registeredTestDataVariations.Add(variation, new TestDataVariationProvider(type, testDataProvider));
        }

        private class TestDataVariationProvider
        {
            public TestDataVariationProvider(Type type, Func<object> testDataProvider)
            {
                this.Type = type;
                this.TestDataProvider = testDataProvider;
            }


            public Func<object> TestDataProvider { get; private set; }

            public Type Type { get; private set; }
        }
    }


    /// <summary>
    /// A generic base class for test data. 
    /// </summary>
    /// <typeparam name="T">The type associated with the test data.</typeparam>
    public abstract class TestData<T> : TestData, IEnumerable<T>
    {
        private static readonly Type OpenIEnumerableType = typeof(IEnumerable<>);
        private static readonly Type OpenListType = typeof(List<>);
        private static readonly Type OpenIQueryableType = typeof(IQueryable<>);

        /// <summary>
        /// Initializes a new instance of the <see cref="TestData&lt;T&gt;"/> class.
        /// </summary>
        protected TestData()
            : base(typeof(T))
        {
            Type[] typeParams = new Type[] { this.Type };

            Type arrayType = this.Type.MakeArrayType();
            Type listType = OpenListType.MakeGenericType(typeParams);
            Type iEnumerableType = OpenIEnumerableType.MakeGenericType(typeParams);
            Type iQueryableType = OpenIQueryableType.MakeGenericType(typeParams);

            Type[] typeArrayParams = new Type[] { arrayType };
            Type[] typeListParams = new Type[] { listType };
            Type[] typeIEnumerableParams = new Type[] { iEnumerableType };
            Type[] typeIQueryableParams = new Type[] { iQueryableType };

            this.RegisterTestDataVariation(TestDataVariations.AsInstance, this.Type, () => GetTypedTestData());
            this.RegisterTestDataVariation(TestDataVariations.AsArray, arrayType, GetTestDataAsArray);
            this.RegisterTestDataVariation(TestDataVariations.AsIEnumerable, iEnumerableType, GetTestDataAsIEnumerable);
            this.RegisterTestDataVariation(TestDataVariations.AsIQueryable, iQueryableType, GetTestDataAsIQueryable);
            this.RegisterTestDataVariation(TestDataVariations.AsList, listType, GetTestDataAsList);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)this.GetTypedTestData().ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetTypedTestData().ToList().GetEnumerator();
        }

        /// <summary>
        /// Gets the test data as an array.
        /// </summary>
        /// <returns>An array of test data of the given type.</returns>
        public T[] GetTestDataAsArray()
        {
            return this.GetTypedTestData().ToArray();
        }

        /// <summary>
        /// Gets the test data as a <see cref="List<>"/>.
        /// </summary>
        /// <returns>A <see cref="List<>"/> of test data of the given type.</returns>
        public List<T> GetTestDataAsList()
        {
            return this.GetTypedTestData().ToList();
        }

        /// <summary>
        /// Gets the test data as an <see cref="IEnumerable<>"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable<>"/> of test data of the given type.</returns>
        public IEnumerable<T> GetTestDataAsIEnumerable()
        {
            return this.GetTypedTestData().AsEnumerable();
        }

        /// <summary>
        /// Gets the test data as an <see cref="IQueryable<>"/>.
        /// </summary>
        /// <returns>An <see cref="IQueryable<>"/> of test data of the given type.</returns>
        public IQueryable<T> GetTestDataAsIQueryable()
        {
            return this.GetTypedTestData().AsQueryable();
        }

        /// <summary>
        /// Must be implemented by derived types to return test data of the given type.
        /// </summary>
        /// <returns>Test data of the given type.</returns>
        protected abstract IEnumerable<T> GetTypedTestData();
    }
}
