﻿using System;
using System.Collections.Generic;

namespace Entitas {

    public delegate void PoolChanged<TEntity>(IPool<TEntity> pool, TEntity entity)
        where TEntity : class, IEntity, new();

    public delegate void GroupChanged<TEntity>(IPool<TEntity> pool, Group<TEntity> group)
        where TEntity : class, IEntity, new();

    /// A pool manages the lifecycle of entities and groups.
    /// You can create and destroy entities and get groups of entities.
    /// The prefered way is to use the generated methods from the code generator
    /// to create a Pool, e.g. Pools.sharedInstance.pool = Pools.CreatePool();
    public interface IPool {

        /// The total amount of components an entity can possibly have.
        /// This value is generated by the code generator,
        /// e.g ComponentIds.TotalComponents.
        int totalComponents { get; }

        /// Returns all componentPools. componentPools is used to reuse
        /// removed components.
        /// Removed components will be pushed to the componentPool.
        /// Use entity.CreateComponent(index, type) to get a new or reusable
        /// component from the componentPool.
        Stack<IComponent>[] componentPools { get; }

        /// The metaData contains information about the pool.
        /// It's used to provide better error messages.
        PoolMetaData metaData { get; }

        /// Returns the number of entities in the pool.
        int count { get; }

        /// Returns the number of entities in the internal ObjectPool
        /// for entities which can be reused.
        int reusableEntitiesCount { get; }

        /// Returns the number of entities that are currently retained by
        /// other objects (e.g. Group, GroupObserver, ReactiveSystem).
        int retainedEntitiesCount { get; }

        /// Destroys all entities in the pool.
        /// Throws an exception if there are still retained entities.
        void DestroyAllEntities();

        /// Clears all groups. This is useful when you want to
        /// soft-restart your application.
        void ClearGroups();

        /// Adds the IEntityIndex for the specified name.
        /// There can only be one IEntityIndex per name.
        void AddEntityIndex(string name, IEntityIndex entityIndex);

        /// Gets the IEntityIndex for the specified name.
        IEntityIndex GetEntityIndex(string name);

        /// Deactivates and removes all entity indices.
        void DeactivateAndRemoveEntityIndices();

        /// Resets the creationIndex back to 0.
        void ResetCreationIndex();

        /// Clears the componentPool at the specified index.
        void ClearComponentPool(int index);

        /// Clears all componentPools.
        void ClearComponentPools();

        /// Resets the pool (clears all groups, destroys all entities and
        /// resets creationIndex back to 0).
        void Reset();
    }

    /// A pool manages the lifecycle of entities and groups.
    /// You can create and destroy entities and get groups of entities.
    /// The prefered way is to use the generated methods from the code generator
    /// to create a Pool, e.g. Pools.sharedInstance.pool = Pools.CreatePool();
    public interface IPool<TEntity> : IPool
        where TEntity : class, IEntity, new() {

        /// Occurs when an entity gets created.
        event PoolChanged<TEntity> OnEntityCreated;

        /// Occurs when an entity got destroyed.
        event PoolChanged<TEntity> OnEntityDestroyed;

        /// Occurs when a group gets created for the first time.
        event GroupChanged<TEntity> OnGroupCreated;

        /// Occurs when a group gets cleared.
        event GroupChanged<TEntity> OnGroupCleared;

        /// Creates a new entity or gets a reusable entity from the
        /// internal ObjectPool for entities.
        TEntity CreateEntity();

        /// Determines whether the pool has the specified entity.
        bool HasEntity(TEntity entity);

        /// Returns all entities which are currently in the pool.
        TEntity[] GetEntities();

        /// Returns a group for the specified matcher.
        /// Calling pool.GetGroup(matcher) with the same matcher will always
        /// return the same instance of the group.
        Group<TEntity> GetGroup(IMatcher<TEntity> matcher);
    }

    public class PoolDoesNotContainEntityException : EntitasException {

        public PoolDoesNotContainEntityException(string message, string hint) :
            base(message + "\nPool does not contain entity!", hint) {
        }
    }

    public class EntityIsNotDestroyedException : EntitasException {

        public EntityIsNotDestroyedException(string message) :
            base(message + "\nEntity is not destroyed yet!",
                     "Did you manually call entity.Release(pool) yourself? " +
                     "If so, please don't :)") {
        }
    }

    public class PoolStillHasRetainedEntitiesException : EntitasException {

        public PoolStillHasRetainedEntitiesException(IPool pool) : base(
            "'" + pool + "' detected retained entities " +
            "although all entities got destroyed!",
            "Did you release all entities? Try calling pool.ClearGroups() " +
            "and systems.ClearReactiveSystems() before calling " +
            "pool.DestroyAllEntities() to avoid memory leaks.") {
        }
    }

    public class PoolMetaDataException : EntitasException {

        public PoolMetaDataException(IPool pool, PoolMetaData poolMetaData) :
            base("Invalid PoolMetaData for '" + pool + "'!\nExpected " +
                     pool.totalComponents + " componentName(s) but got " +
                     poolMetaData.componentNames.Length + ":",
                     string.Join("\n", poolMetaData.componentNames)) {
        }
    }

    public class PoolEntityIndexDoesNotExistException : EntitasException {

        public PoolEntityIndexDoesNotExistException(IPool pool, string name) :
            base("Cannot get EntityIndex '" + name + "' from pool '" +
                 pool + "'!", "No EntityIndex with this name has been added.") {
        }
    }

    public class PoolEntityIndexDoesAlreadyExistException : EntitasException {

        public PoolEntityIndexDoesAlreadyExistException(IPool pool, string name) :
            base("Cannot add EntityIndex '" + name + "' to pool '" + pool + "'!",
                 "An EntityIndex with this name has already been added.") {
        }
    }

    /// The metaData contains information about the pool.
    /// It's used to provide better error messages.
    public class PoolMetaData {

        public readonly string poolName;
        public readonly string[] componentNames;
        public readonly Type[] componentTypes;

        public PoolMetaData(string poolName,
                                string[] componentNames,
                                Type[] componentTypes) {
            this.poolName = poolName;
            this.componentNames = componentNames;
            this.componentTypes = componentTypes;
        }
    }
}