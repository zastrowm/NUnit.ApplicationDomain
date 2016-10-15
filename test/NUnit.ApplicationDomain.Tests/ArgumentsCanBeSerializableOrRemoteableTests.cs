using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  public class ArgumentsCanBeSerializableOrRemoteableTests
  {
    private static readonly RemotableType[] RemotableTestData
      =
      {
        new RemotableType(1),
        new RemotableType(2),
        new RemotableType(3),
      };

    [Test]
    [TestCaseSource(nameof(RemotableTestData))]
    public void ArgumentsThatAreMarhsallByRefWork(RemotableType testData)
    {
      Assert.That(testData.Data, Is.Not.EqualTo(0));
    }

    public class RemotableType : MarshalByRefObject
    {
      public RemotableType(int data)
      {
        Data = data;
      }

      public int Data { get; }

      /// <inheritdoc />
      public override string ToString()
        => Data.ToString();
    }

    private static readonly SerializableType[] SerialableTestData
      =
      {
        new SerializableType(1),
        new SerializableType(2),
        new SerializableType(3),
      };

    [Test]
    [TestCaseSource(nameof(SerialableTestData))]
    public void ArgumentsThatAreSerializableWork(SerializableType testData)
    {
      Assert.That(testData.Data, Is.Not.EqualTo(0));
    }

    [Serializable]
    public class SerializableType
    {
      public SerializableType(int data)
      {
        Data = data;
      }

      public int Data { get; }

      /// <inheritdoc />
      public override string ToString()
        => Data.ToString();
    }
  }
}