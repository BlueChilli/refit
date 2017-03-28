using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Xunit;

namespace Refit.Tests
{

    public class MultiPartDataTests
    {
        private Func<T, MultiPartData<T>> Create<T>()
        {
            return MultiPartData<T>.Create;
        }

        public static TheoryDataSet<Stream> StreamData => new TheoryDataSet<Stream>()
        {
            new MemoryStream()
        };

        public static TheoryDataSet<FileInfo> FileInfoData => new TheoryDataSet<FileInfo>()
        {
            new FileInfo(@"C:\Temp\S.txt")
        };

        public static TheoryDataSet<byte[]> BytesData => new TheoryDataSet<byte[]>()
        {
            new byte[10]
        };

        public static TheoryDataSet<List<string>> StringListData => new TheoryDataSet<List<string>>()
        {
            new List<string>()
        };

     
        [Fact]
        public void Ctr_ShouldThrow_IfDataIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => MultiPartData<string>.Create(null));
        }

        [Theory]
        [TestDataSet(typeof(MultiPartDataTests), nameof(StreamData))]
        [TestDataSet(typeof(MultiPartDataTests), nameof(FileInfoData))]
        [TestDataSet(typeof(MultiPartDataTests), nameof(BytesData))]
        public void Ctr_ShouldThrow_IfAnyOfDataContainsFileRelatedObjectTypes(object data)
        {
            var taskFuncMi = GetType().GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Instance);
            var taskFunc = (MulticastDelegate)taskFuncMi.MakeGenericMethod(data.GetType()).Invoke(this, new object[] {  });

            try
            {
                taskFunc.DynamicInvoke(data);
            }
            catch (TargetInvocationException e)
            {
                Assert.IsType<ArgumentException>(e.InnerException);
            }
        }

        [Fact]
        public async Task ShouldThrow_WhenMethodIsNotMultiPartButUsedMultiPartData()
        {
            var api = RestService.For<ITestApi3>("http://www.test.com");

            await Assert.ThrowsAsync<ArgumentException>(async () => await api.ChangeTask(MultiPartData<TestObject>.Create(new TestObject())));
        }

    }
}
