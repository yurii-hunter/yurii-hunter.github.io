---
title: "Передавать параметры в конструкторе – не самая хорошая идея"
layout: post
date: 2018-03-17 12:00
image: /assets/images/builder_featured.jpg
headerImage: false
tag:
- patterns
star: false
category: blog
author: yurii-hunter
description: Передавать параметры в конструкторе – не самая хорошая идея
---
Заполнение объекта может быть реализовано в несколько способов. Один из них - это использование конструкторов или Helper методов. Но проблема в том, что таких методов может быть много, а также количество и тип входных параметров может меняться, что вызывает трудности при чтении.
```csharp
// Можно использовать конструкторы
Event event = new Event(EventType.Conference);

// Использование Helper методов, которые немного упрощают чтение, но не всегда нуждаются во всех параметрах
Event bigConference = Conference("Global AQA meetup", 500, Type.Public);
Event dailyMeeting = Meeting("Daily standup", 10, Regular);
Event discussion = Meeting("Strategy", 3, null);

// Использование боллее специфических методов
Event syncup = WeeklyMeeting("QA syncup", 5);
```
Решением является использование Builder классов, которые возвращают объект этого же класса с частично заполненными данными.
```csharp
Event bigConference = EventBuilder
                            .Conference()
                            .WithName("Global AQA meetup")
                            .WithType(Type.Public)
                            .WithCapacity(500)
                            .Build();
```
Реализация такого класса будет выглядеть так
```csharp
class EventBuilder
{
    Event event;

    private EventBuilder(EventType type)
    {
        event = new Event(type);
    }

    public static EventBuilder Conference()
    {
        return new EventBuilder(EventType.Conference);
    }

    public EventBuilder WithName(string name)
    {
        event.Name = name;
        return this;
    }

    public EventBuilder WithAccess(Access access)
    {
        event.Access = access;
        return this;
    }

    public EventBuilder WithCapacity(int capacity)
    {
        event.Capacity = capacity;
        return this;
    }

    public Event Build()
    {
        return event;
    }
}
```
Также в методе Build можно выполнять преобразования или проверку обязательных полей, если в этом есть необходимость.
