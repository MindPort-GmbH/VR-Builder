// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

namespace VRBuilder.Tests.Builder
{
    public abstract class StepBuilder<TStep> : BuilderWithResourcePath<TStep>
    {
        public StepBuilder(string name) : base(name)
        {
        }
    }
}
