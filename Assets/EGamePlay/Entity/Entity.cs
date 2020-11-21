﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EGamePlay
{
    public abstract partial class Entity : IDisposable
    {
#if !SERVER
        public UnityEngine.GameObject GameObject { get; set; }
#endif
        public long Id { get; set; }
        private Entity parent;
        public Entity Parent { get { return parent; } private set { parent = value; OnSetParent(value); } }
        public bool IsDispose { get { return Id == 0; } }
        public Dictionary<Type, Component> Components { get; set; } = new Dictionary<Type, Component>();
        private List<Entity> Children { get; set; } = new List<Entity>();
        private Dictionary<Type, List<Entity>> Type2Children { get; set; } = new Dictionary<Type, List<Entity>>();


        public Entity()
        {
#if !SERVER
            GameObject = new UnityEngine.GameObject(GetType().Name);
#endif
        }

        public virtual void Awake()
        {

        }

        public virtual void Awake(object paramObject)
        {

        }

        public virtual void Dispose()
        {
            Log.Debug($"{GetType().Name}->Dispose");
            foreach (var child in Children)
            {
                child.Dispose();
            }
            Children.Clear();
            Type2Children.Clear();
            foreach (Component component in this.Components.Values)
            {
                component.Dispose();
            }
            this.Components.Clear();
            Id = 0;
#if !SERVER
            UnityEngine.GameObject.Destroy(GameObject);
#endif
        }

        public virtual void OnDestroy()
        {

        }

        public virtual void OnSetParent(Entity parent)
        {

        }

        public T GetParent<T>() where T : Entity
        {
            return parent as T;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var c = new T();
            c.Entity = this;
            c.IsDispose = false;
            this.Components.Add(typeof(T), c);
            EntityFactory.GlobalEntity.AllComponents.Add(c);
            Log.Debug($"{GetType().Name}->AddComponent, {typeof(T).Name}");
            c.Setup();
            return c;
        }

        public void RemoveComponent<T>() where T : Component
        {
            this.Components[typeof(T)].Dispose();
            this.Components.Remove(typeof(T));
        }

        public T GetComponent<T>() where T : Component
        {
            if (this.Components.TryGetValue(typeof(T),  out var component))
            {
                return component as T;
            }
            return null;
        }

        public void SetParent(Entity parent)
        {
            Parent?.RemoveChild(this);
            parent?.AddChild(this);
        }

        public void AddChild(Entity child)
        {
            Children.Add(child);
            if (!Type2Children.ContainsKey(child.GetType()))
            {
                Type2Children.Add(child.GetType(), new List<Entity>());
            }
            Type2Children[child.GetType()].Add(child);
            child.Parent = this;
#if !SERVER
            child.GameObject.transform.SetParent(GameObject.transform);
#endif
        }

        public void RemoveChild(Entity child)
        {
            Children.Remove(child);
            if (Type2Children.ContainsKey(child.GetType()))
            {
                Type2Children[child.GetType()].Remove(child);
            }
            child.Parent = null;
#if !SERVER
            child.GameObject.transform.SetParent(null);
#endif
        }

        public Entity[] GetChildren()
        {
            return Children.ToArray();
        }

        public Entity[] GetTypeChildren<T>() where T : Entity
        {
            return Type2Children[typeof(T)].ToArray();
        }

        public T Publish<T>(T TEvent) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            eventComponent.Publish(TEvent);
            return TEvent;
        }

        public EventSubscribe<T> Subscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            return eventComponent.Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.UnSubscribe(action);
            }
        }
    }
}