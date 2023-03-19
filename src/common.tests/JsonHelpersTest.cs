// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using k8s;
using Microsoft.BridgeToKubernetes.Common.Serialization;
using System;
using Xunit;

namespace Microsoft.BridgeToKubernetes.Common.Tests.Json
{
    public class JsonSerializerTest
    {
        private JsonSerializer _jsonSerializer;

        public JsonSerializerTest()
        {
            _jsonSerializer = new JsonSerializer();
        }

        [Fact]
        public void SerializeObjectSuccessful()
        {
            Person e = new Person()
            {
                Name = "Frodo"
            };
            var expected = "{\"name\":\"Frodo\",\"bestFriend\":null}";
            Assert.Equal(expected, _jsonSerializer.SerializeObject(e));
        }

        [Fact]
        public void SerializeObjectIndentedSuccessful()
        {
            Person e = new Person()
            {
                Name = "Frodo"
            };
            var expected = string.Format("{{{0}  \"name\": \"Frodo\",{0}  \"bestFriend\": null{0}}}", Environment.NewLine);
            var result = _jsonSerializer.SerializeObjectIndented(e);
            Assert.Equal(expected, result);
            Assert.True(result.Split("\n").Length > 1);
        }

        [Fact]
        public void SerializeObjectWithReferenceLoopDefaultSettings()
        {
            Person p1 = new Person()
            {
                Name = "Frodo"
            };

            Person p2 = new Person()
            {
                Name = "Samwise",
                BestFriend = p1
            };

            p1.BestFriend = p2;
            var expected = "{\"name\":\"Samwise\",\"bestFriend\":{\"name\":\"Frodo\",\"bestFriend\":null}}";
            Assert.Equal(expected, _jsonSerializer.SerializeObject(p2));
        }

        [Fact]
        public void SerializeAndDeserializeDefaultSettings()
        {
            Person p1 = new Person()
            {
                Name = "Frodo"
            };

            Person p2 = new Person()
            {
                Name = "Samwise",
                BestFriend = p1
            };

            Person p3 = new Person()
            {
                Name = "Pippin",
                BestFriend = p2
            };

            var serialized = _jsonSerializer.SerializeObject(p3);
            var deserialized = _jsonSerializer.DeserializeObject<object>(serialized);
            var expected = string.Format("{{\"name\":\"Pippin\",\"bestFriend\":{{"
                 + "\"name\":\"Samwise\",\"bestFriend\":{{\"name\":\"Frodo\","
                 + "\"bestFriend\":null}}}}}}", Environment.NewLine);
            Assert.Equal(expected, deserialized.ToString());
        }

        [Fact]
        public void SerializeWatchEventType()
        {
            var json = "{  \"type\": \"ADDED\",  \"object\": {\"kind\": \"Pod\", \"apiVersion\": \"v1\", \"metadata\": {\"resourceVersion\": \"10596\"}}}";
            var deserialized = _jsonSerializer.DeserializeObject<WatchEvent>(json);

            Assert.Equal(WatchEventType.Added, deserialized.Type);
        }

        private class Person
        {
            public string Name { get; set; }
            public Person BestFriend { get; set; }
        }

        private class WatchEvent
        {
            public WatchEventType Type { get; set; }
            public KubernetesObject Object { get; set; }
        }
    }
}