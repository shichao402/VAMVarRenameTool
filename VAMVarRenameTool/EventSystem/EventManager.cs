using System;
using System.Collections.Generic;

namespace VAMVarRenameTool.EventSystem
{
    public class EventManager
    {
        private readonly Dictionary<Type, Delegate> _events = new();

        public void DefineEvent<TEvent>() where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (!_events.ContainsKey(eventType))
            {
                _events[eventType] = null;
            }
        }

        public void RegisterEvent<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_events.ContainsKey(eventType))
            {
                _events[eventType] = (Action<TEvent>)_events[eventType] + handler;
            }
            else
            {
                throw new ArgumentException($"Event {eventType.Name} is not defined.");
            }
        }

        public void UnregisterEvent<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_events.ContainsKey(eventType))
            {
                _events[eventType] = (Action<TEvent>)_events[eventType] - handler;
            }
            else
            {
                throw new ArgumentException($"Event {eventType.Name} is not defined.");
            }
        }

        public void TriggerEvent<TEvent>(TEvent args) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_events.ContainsKey(eventType))
            {
                var handler = (Action<TEvent>)_events[eventType];
                handler?.Invoke(args);
            }
            else
            {
                throw new ArgumentException($"Event {eventType.Name} is not defined.");
            }
        }
    }
}
