using System;

namespace VRBuilder.Core.Attributes
{
	/// <summary>
	/// An attribute used to specify a default concrete type for a specific scene object property interface.
	/// </summary>
	/// <remarks>
	/// The attribute is used to determine which concrete type should be used as the default wehn the <seealso cref="SceneObjectAutomaticSetup"/>,
	///  also known as the fix-it button, is activated. An example use case is if you inherit from an existing scene object property like 
	/// <seealso cref="GrabbableProperty"/> to create a grabbable with additional functionality. For example, a pencil.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultSceneObjectPropertyAttribute : Attribute
	{
		public Type ConcreteType { get; }

		public DefaultSceneObjectPropertyAttribute(Type concreteType)
		{
			ConcreteType = concreteType;
		}
	}
}