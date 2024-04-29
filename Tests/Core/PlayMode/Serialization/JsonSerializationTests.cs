// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core.Serialization.NewtonsoftJson;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace VRBuilder.Core.Tests.Serialization
{
    public class JsonSerializationTests
    {
        private JsonSerializerSettings settings;

        [SetUp]
        protected void Setup()
        {
            settings = NewtonsoftJsonProcessSerializer.ProcessSerializerSettings;
        }

        [Test]
        public void CanSerializeVector2()
        {
            // Given the JsonProcessSerializer.SerializerSettings are used to serialize.
            // When a Vector2 is serialized
            string data = JsonConvert.SerializeObject(Vector2.up, settings);
            // Then the output is not null
            Assert.IsFalse(string.IsNullOrEmpty(data));
        }

        [Test]
        public void SerializedVector2CanBeReadAgain()
        {
            // Given a Vector2 input
            Vector2 input = Vector2.up;

            // When parsed into json and back
            string data = JsonConvert.SerializeObject(input, settings);
            Vector2 output = JsonConvert.DeserializeObject<Vector2>(data, settings);

            // Then the input and output are equal
            Assert.AreEqual(input, output);
        }

        [Test]
        public void CanSerializeVector3()
        {
            // Given the JsonProcessSerializer.SerializerSettings are used to serialize.
            // When a Vector3 is serialized
            string data = JsonConvert.SerializeObject(Vector3.up, settings);
            // Then the output is not null
            Assert.IsFalse(string.IsNullOrEmpty(data));
        }

        [Test]
        public void SerializedVector3CanBeReadAgain()
        {
            // Given a Vector3 input
            Vector3 input = Vector3.up;

            // When parsed into json and back
            string data = JsonConvert.SerializeObject(input, settings);
            Vector3 output = JsonConvert.DeserializeObject<Vector3>(data, settings);

            // Then the input and output are equal
            Assert.AreEqual(input, output);
        }

        [Test]
        public void CanSerializeVector4()
        {
            // Given the JsonProcessSerializer.SerializerSettings are used to serialize.
            // When a Vector4 is serialized
            string data = JsonConvert.SerializeObject(Vector4.one, settings);
            // Then the output is not null
            Assert.IsFalse(string.IsNullOrEmpty(data));
        }

        [Test]
        public void SerializedVector4CanBeReadAgain()
        {
            // Given a Vector4 input
            Vector4 input = Vector4.one;

            // When parsed into json and back
            string data = JsonConvert.SerializeObject(input, settings);
            Vector4 output = JsonConvert.DeserializeObject<Vector4>(data, settings);

            // Then the input and output are equal
            Assert.AreEqual(input, output);
        }


        [Test]
        public void CanSerializeColor()
        {
            // Given the JsonProcessSerializer.SerializerSettings are used to serialize.
            // When a unity color is serialized
            string data = JsonConvert.SerializeObject(Color.blue, settings);
            // Then the output is not null
            Assert.IsFalse(string.IsNullOrEmpty(data));
        }

        [Test]
        public void SerializedColorCanBeReadAgain()
        {
            // Given a Color input
            Color input = Color.blue;

            // When parsed into json and back
            string data = JsonConvert.SerializeObject(input, settings);
            Color output = JsonConvert.DeserializeObject<Color>(data, settings);

            // Then the input and output are equal
            Assert.AreEqual(input, output);
        }

        [Test]
        public void CanSerializeKeyframe()
        {
            // Given the JsonProcessSerializer.SerializerSettings are used to serialize.
            // When a unity keyframe is serialized
            Keyframe input = new Keyframe(0.1f, 0.5f, 1, 0.7f, 0.6f, 0.4f);
            input.weightedMode = WeightedMode.In;
            string data = JsonConvert.SerializeObject(new Keyframe(0.1f, 0.5f, 1, 0.7f, 0.6f, 0.4f), settings);
            // Then the output is not null
            Assert.IsFalse(string.IsNullOrEmpty(data));
        }

        [Test]
        public void SerializedKeyframeCanBeReadAgain()
        {
            // Given a Color input
            Keyframe input = new Keyframe(0.1f, 0.5f, 1, 0.7f, 0.6f, 0.4f);
            input.weightedMode = WeightedMode.In;

            // When parsed into json and back
            string data = JsonConvert.SerializeObject(input, settings);
            Keyframe output = JsonConvert.DeserializeObject<Keyframe>(data, settings);

            // Then the input and output are equal
            Assert.AreEqual(input, output);
        }

        [Test]
        public void CanSerializeAnimationCurve()
        {
            // Given the JsonProcessSerializer.SerializerSettings are used to serialize.
            // When a unity animation curve is serialized
            Keyframe first = new Keyframe(0.1f, 0.5f, 1, 0.7f, 0.6f, 0.4f);
            Keyframe second = new Keyframe(0.3f, 0.4f, 0.3f, 0.2f, 0.9f, 0.1f);
            AnimationCurve curve = new AnimationCurve(first, second);
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.Loop;
            string data = JsonConvert.SerializeObject(curve, settings);
            // Then the output is not null
            Assert.IsFalse(string.IsNullOrEmpty(data));
        }

        [Test]
        public void SerializedAnimationCurveCanBeReadAgain()
        {
            // Given an AnimationCurve input
            Keyframe first = new Keyframe(0.1f, 0.5f, 1, 0.7f, 0.6f, 0.4f);
            Keyframe second = new Keyframe(0.3f, 0.4f, 0.3f, 0.2f, 0.9f, 0.1f);
            AnimationCurve input = new AnimationCurve(new[] { first, second });
            input.preWrapMode = WrapMode.ClampForever;
            input.postWrapMode = WrapMode.Loop;

            // When parsed into json and back
            string data = JsonConvert.SerializeObject(input, settings);
            AnimationCurve output = JsonConvert.DeserializeObject<AnimationCurve>(data, settings);

            // Then the input and output are equal
            Assert.AreEqual(input, output);
        }
    }
}
