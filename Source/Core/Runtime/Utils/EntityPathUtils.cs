using System.Collections.Generic;
using System.Reflection;
using VRBuilder.Core;

namespace VRBuilder.Core.Utils
{
	/// <summary>
	/// Utilities to build human-readable entity breadcrumbs with Unity rich-text tags.
	/// Compiled for both Editor and player builds.
	/// </summary>
	public static class EntityPathUtils
	{
		/// <summary>
		/// Builds a breadcrumb path from the root entity down to the provided leaf,
		/// formatting each segment with <b>type name</b> and optional '<i>name</i>'.
		/// Example:
		/// <b>Process</b> '<i>My Process</i>' > <b>Chapter</b> '<i>Intro</i>' > <b>Step</b> '<i>Pick up object</i>' > <b>ScalingBehavior</b> '<i>Scale cube</i>'
		/// </summary>
		public static string BuildRichTextEntityPath(IEntity leaf)
		{
			if (leaf == null)
			{
				return "<b>Unknown Entity</b>";
			}

			var segments = new List<string>();
			IEntity current = leaf;

			while (current != null)
			{
				string typeName = current.GetType().Name;
				string name = TryGetEntityName(current);

				if (string.IsNullOrEmpty(name))
				{
					segments.Add($"<b>{typeName}</b>");
				}
				else
				{
					segments.Add($"<b>{typeName}</b> '<i>{name}</i>'");
				}

				current = current.Parent;
			}

			segments.Reverse();
			return string.Join(" > ", segments);
		}

		/// <summary>
		/// Tries to extract a human-readable name from an entity by looking for entity.Data.Name (public instance).
		/// Works for Process, Chapter, Step, Behaviors, etc. that follow the usual pattern.
		/// </summary>
		private static string TryGetEntityName(IEntity entity)
		{
			if (entity == null)
			{
				return null;
			}

			// Look for a public instance "Data" property
			PropertyInfo dataProperty = entity.GetType().GetProperty(
				"Data",
				BindingFlags.Instance | BindingFlags.Public
			);

			if (dataProperty == null)
			{
				return null;
			}

			object data = dataProperty.GetValue(entity);
			if (data == null)
			{
				return null;
			}

			// Look for a public instance "Name" property on Data
			PropertyInfo nameProperty = data.GetType().GetProperty(
				"Name",
				BindingFlags.Instance | BindingFlags.Public
			);

			if (nameProperty == null)
			{
				return null;
			}

			return nameProperty.GetValue(data) as string;
		}
	}
}