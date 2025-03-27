using System;

namespace VRBuilder.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultImplementationAttribute : Attribute
	{
		public Type ConcreteType { get; }

		public DefaultImplementationAttribute(Type concreteType)
		{
			ConcreteType = concreteType;
		}
	}
}