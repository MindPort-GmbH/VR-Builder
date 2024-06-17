using VRBuilder.Core.EntityOwners;

namespace VRBuilder.Core.Utils
{
    /// <summary>
    /// Collection of utilities for handling processes.
    /// </summary>
    public static class ProcessUtils
    {
        /// <summary>
        /// Returns the parent entity for the specified entity in the given process.
        /// </summary>
        public static IEntity GetParentEntity(IEntity entity, IProcess process)
        {
            if (entity == process)
            {
                return null;
            }

            return CheckChildrenRecursively(process, entity);
        }

        private static IEntity CheckChildrenRecursively(IEntity searchedEntity, IEntity entityToFind)
        {
            IDataOwner currentEntity = searchedEntity as IDataOwner;

            if (currentEntity != null && currentEntity.Data is IEntityCollectionData)
            {
                foreach (IEntity childEntity in ((IEntityCollectionData)currentEntity.Data).GetChildren())
                {
                    if (childEntity == entityToFind)
                    {
                        return searchedEntity;
                    }

                    IEntity parent = CheckChildrenRecursively(childEntity, entityToFind);

                    if (parent != null)
                    {
                        return parent;
                    }
                }
            }

            return null;
        }
    }
}
