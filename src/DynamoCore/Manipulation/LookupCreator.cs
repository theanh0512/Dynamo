﻿using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;

namespace Dynamo.Manipulation
{
    public abstract class LookupCreator<T> : INodeManipulatorCreator where T : NodeModel
    {
        protected Dictionary<string, IEnumerable<INodeManipulatorCreator>> ManipulatorCreators { get; private set; }

        protected LookupCreator(Dictionary<string, IEnumerable<INodeManipulatorCreator>> manipulatorCreators)
        {
            ManipulatorCreators = manipulatorCreators;
        }

        protected LookupCreator() : this(new Dictionary<string, IEnumerable<INodeManipulatorCreator>>()) { }

        public IManipulator Create(NodeModel node, DynamoManipulatorContext context)
        {
            var dsfunc = node as T;
            if (dsfunc == null) return null;

            var name = GetKey(dsfunc);

            return ManipulatorCreators.ContainsKey(name)
                ? new CompositeManipulator(ManipulatorCreators[name].Select(m => m.Create(node, context)).ToList())
                : null;
        }

        protected abstract string GetKey(T dsfunc);
    }
}